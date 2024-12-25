namespace SurveyBasket.Api.Services.Questions;

public class QuestionService(ApplicationDbContext context) : IQuestionService
{
    private readonly ApplicationDbContext _context = context;


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

        return Result.Success();
    }


}
