using SurveyBasket.Api.Contracts.Votes;

namespace SurveyBasket.Api.Services.Votes;

public class VoteService(ApplicationDbContext context) : IVoteService
{
    private readonly ApplicationDbContext _context = context;
    public async Task<Result> AddAsync(int pollId, string userId, VoteRequest request, CancellationToken cancellationToken = default)
    {
        // 1- check for this user votes before in this poll
        var hasVote = await _context.Votes.AnyAsync(v => v.PollId == pollId && v.UserId == userId, cancellationToken);

        if (hasVote)
            return Result.Failure(VoteErrors.DuplicatedVote);

        //2- check for existing poll
        var isExistingPoll = await _context.Polls
            .AnyAsync(p => p.Id == pollId && p.IsPublished && p.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && p.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow), cancellationToken);

        if (!isExistingPoll)
            return Result.Failure(PollErrors.PollNotFound);

        //3- compare between the questions in the poll in the database and the questions in the request (compare between the ids only)
        List<int>? availableQuestions = await _context.Questions
            .Where(q => q.PollId == pollId && q.IsActive)
            .Select(q => q.Id)
            .ToListAsync(cancellationToken);

        // the questions in the request must be equal to the available questions and the order must be the same

        if (!request.Answers.Select(a => a.QuestionId).SequenceEqual(availableQuestions))
            return Result.Failure(VoteErrors.InvalidQuestions);


        var vote = new Vote
        {
            PollId = pollId,
            UserId = userId,
            VoteAnswers = request.Answers.Select(a => new VoteAnswer
            {
                QuestionId = a.QuestionId,
                AnswerId = a.AnswerId

            }).ToList()

        };

        await _context.Votes.AddAsync(vote, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();


    }
}
