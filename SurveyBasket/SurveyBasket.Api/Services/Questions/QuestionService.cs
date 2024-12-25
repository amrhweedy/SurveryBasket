using SurveyBasket.Api.Contracts.Questions;
using SurveyBasket.Api.Errors;

namespace SurveyBasket.Api.Services.Questions;

public class QuestionService(ApplicationDbContext context) : IQuestionService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<QuestionResponse>> AddAsync(int pollId ,QuestionRequest request, CancellationToken cancellationToken = default)
    {
        var isExistingPoll=   await _context.Polls.AnyAsync(p => p.Id == pollId,cancellationToken);

        if(!isExistingPoll)
            return Result.Failure<QuestionResponse>(PollErrors.PollNotFound);

        var isExistingQuestion = await _context.Questions.AnyAsync(q=> q.Content == request.Content && q.PollId == pollId,cancellationToken);

        if (isExistingQuestion)
            return Result.Failure<QuestionResponse>(QuestionErrors.DuplicatedQuestionContent);

        var question = request.Adapt<Question>();
        question.PollId = pollId;

        request.Answers.ForEach(answer => question.Answers.Add(new Answer { Content = answer }));

        await _context.Questions.AddAsync(question,cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(question.Adapt<QuestionResponse>());

     }
}
