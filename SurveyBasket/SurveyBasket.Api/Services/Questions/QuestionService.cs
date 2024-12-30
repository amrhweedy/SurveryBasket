using Microsoft.Extensions.Caching.Memory;
using SurveyBasket.Api.Contracts.Answers;

namespace SurveyBasket.Api.Services.Questions;

public class QuestionService(ApplicationDbContext context, IMemoryCache memoryCache) : IQuestionService
{
    private readonly ApplicationDbContext _context = context;
    private readonly IMemoryCache _memoryCache = memoryCache;
    private const string _cachePrefix = "AvailableQuestions";

    public async Task<Result<IEnumerable<QuestionResponse>>> GetAllAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var isExistingPoll = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken);

        if (!isExistingPoll)
            return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);

        var questions = await _context.Questions
            .Where(q => q.PollId == pollId)
            .Include(q => q.Answers)
            //.Select(q => new QuestionResponse(               // we use select method to get a specific columns from the database , not all columns
            //    q.Id,
            //    q.Content,
            //    q.Answers.Select(a => new AnswerResponse(a.Id, a.Content))
            //    ))
            .ProjectToType<QuestionResponse>()    // or use mapster to make the mapping and select the specific columns from the database, it will map every question to QuestionResponse 
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<QuestionResponse>>(questions);
    }

    // we need here to get the available questions with answers for the specific poll to make the user votes on this poll (vote on every question inside this poll)
    public async Task<Result<IEnumerable<QuestionResponse>>> GetAvailableAsync(int pollId, string userId, CancellationToken cancellationToken = default)
    {
        // 1- check for this user votes before for this poll
        var hasVote = await _context.Votes.AnyAsync(v => v.PollId == pollId && v.UserId == userId, cancellationToken);

        if (hasVote)
            return Result.Failure<IEnumerable<QuestionResponse>>(VoteErrors.DuplicatedVote);

        //2- check for existing poll
        var isExistingPoll = await _context.Polls
            .AnyAsync(p => p.Id == pollId && p.IsPublished && p.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && p.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

        if (!isExistingPoll)
            return Result.Failure<IEnumerable<QuestionResponse>>(PollErrors.PollNotFound);

        var cacheKey = $"{_cachePrefix}-{pollId}"; // the cache key must be unique so i use the pollId in the key to make every poll has a different cache about the another polls

        // 3- get the available active questions with the active answers
        var questions = await _memoryCache.GetOrCreateAsync(
            cacheKey,
             casheEntry =>
             {

                 casheEntry.SlidingExpiration = TimeSpan.FromMinutes(5);
                 return _context.Questions
               .Where(q => q.PollId == pollId && q.IsActive)
               .Include(q => q.Answers)
               .Select(q => new QuestionResponse
               (
                   q.Id,
                   q.Content,
                   q.Answers.Where(a => a.IsActive).Select(a => new AnswerResponse(a.Id, a.Content))

               ))
               .AsNoTracking()
               .ToListAsync(cancellationToken);
             }
            );


        return Result.Success<IEnumerable<QuestionResponse>>(questions!);

    }


    public async Task<Result<QuestionResponse>> GetAsync(int pollId, int id, CancellationToken cancellationToken = default)
    {
        var isExistingPoll = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken);

        if (!isExistingPoll)
            return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

        var currentQuestion = await _context.Questions
            .Where(q => q.Id == id && q.PollId == pollId)
            .Include(q => q.Answers)
            .ProjectToType<QuestionResponse>()
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (currentQuestion is null)
            return Result.Failure<QuestionResponse>(QuestionErrors.QuestionNotFound);


        return Result.Success<QuestionResponse>(currentQuestion);
    }


    public async Task<Result<QuestionResponse>> AddAsync(int pollId, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var isExistingPoll = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken);

        if (!isExistingPoll)
            return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

        // check if there is a question with the same content in the same poll
        var isExistingQuestion = await _context.Questions.AnyAsync(q => q.Content == request.Content && q.PollId == pollId, cancellationToken);

        if (isExistingQuestion)
            return Result.Failure<QuestionResponse>(QuestionErrors.DuplicatedQuestionContent);

        // this gives me error because it can not convert from list<string> Answers which in the QuestionRequest to list<Answer> in the Question
        // so there are 2 solutions => 1- ignore mapping list<string> Answers which in the QuestionRequest to list<Answer> in the Question and still write this line =>
        //request.Answers.ForEach(answer => question.Answers.Add(new Answer { Content = answer }));
        // 2 - mapping list<string> Answers which in the QuestionRequest to list<Answer> in the Question in the mappingConfigurations file and remove this line
        //request.Answers.ForEach(answer => question.Answers.Add(new Answer { Content = answer })); because this mapping will be add the answers in the question


        var question = request.Adapt<Question>();
        question.PollId = pollId;

        //request.Answers.ForEach(answer => question.Answers.Add(new Answer { Content = answer }));

        await _context.Questions.AddAsync(question, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _memoryCache.Remove(key: $"{_cachePrefix}-{pollId}");

        return Result.Success<QuestionResponse>(question.Adapt<QuestionResponse>());
    }


    public async Task<Result<QuestionResponse>> UpdateAsync(int pollId, int id, QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var isExistingPoll = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken);

        if (!isExistingPoll)
            return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

        // check if there is a question with the same content in the same poll
        var isExistingQuestionWithSameContent = await _context.Questions
            .AnyAsync(q => q.Content == request.Content && q.Id != id && q.PollId == pollId, cancellationToken);

        if (isExistingQuestionWithSameContent)
            return Result.Failure<QuestionResponse>(QuestionErrors.DuplicatedQuestionContent);

        // get the current question
        var question = await _context.Questions
            .Include(q => q.Answers)
            .FirstOrDefaultAsync(q => q.Id == id && q.PollId == pollId, cancellationToken);

        if (question is null)
            return Result.Failure<QuestionResponse>(QuestionErrors.QuestionNotFound);

        // update the question
        question.Content = request.Content;

        // update the answers (current Answers in the database)

        //1-  convert from list<Answer> to list<string>
        var currentAnswers = question.Answers.Select(a => a.Content).ToList();

        //2-  add the new answers to the current answers for the same request ,for example if the request has 2 answers ["a","b","c"] and the current answers has answers ["a","b] then the new answers will be ["c"] then add these new answers to the question
        var newAnswers = request.Answers.Except(currentAnswers).ToList();

        newAnswers.ForEach(answer => question.Answers.Add(new Answer { Content = answer }));

        // 3- if there are answers in the current answers that are not in the request then make them inactive

        question.Answers.ToList().ForEach(answer =>
        {
            answer.IsActive = request.Answers.Contains(answer.Content);
        });


        await _context.SaveChangesAsync(cancellationToken);
        _memoryCache.Remove(key: $"{_cachePrefix}-{pollId}");


        return Result.Success<QuestionResponse>(question.Adapt<QuestionResponse>());
    }


    public async Task<Result> ToggleStatsAsync(int pollId, int Id, CancellationToken cancellationToken = default)
    {
        var isExistingPoll = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken);

        if (!isExistingPoll)
            return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

        var question = await _context.Questions
            .FirstOrDefaultAsync(q => q.Id == Id && q.PollId == pollId, cancellationToken);

        if (question is null)
            return Result.Failure(QuestionErrors.QuestionNotFound);

        question.IsActive = !question.IsActive;

        await _context.SaveChangesAsync(cancellationToken);
        _memoryCache.Remove(key: $"{_cachePrefix}-{pollId}");


        return Result.Success();
    }


}
