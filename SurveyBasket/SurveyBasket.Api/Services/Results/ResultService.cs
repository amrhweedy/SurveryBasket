using SurveyBasket.Api.Contracts.Results;

namespace SurveyBasket.Api.Services.Results;

public class ResultService(ApplicationDbContext context) : IResultService
{
    private readonly ApplicationDbContext _context = context;

    public async Task<Result<PollVotesResponse>> GetPollVotesAsync(int pollId, CancellationToken cancellationToken = default)
    {

        // we need to return the poll title and the list of votes which occur for this poll with the voter name and the questions and the voter answers for these questions
        var pollVotes = await _context.Polls
            .Where(p => p.Id == pollId)
            .Select(p => new PollVotesResponse(
                p.Title,
                p.Votes.Select(v => new VoteResponse(
                     $"{v.User.FirstName} {v.User.LastName}",
                      v.SubmittedOn,
                      v.VoteAnswers.Select(va => new QuestionAnswerResponse(
                        va.Question.Content,
                        va.Answer.Content
                        ))
                    )
                ))).FirstOrDefaultAsync(cancellationToken);

        return pollVotes is null
            ? Result.Failure<PollVotesResponse>(PollErrors.PollNotFound)
            : Result.Success(pollVotes);
    }

    public async Task<Result<IEnumerable<VotesPerDayResponse>>> GetPollVotesPerDayAsync(int pollId, CancellationToken cancellationToken = default)
    {

        var isExistingPoll = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken);

        if (!isExistingPoll)
            return Result.Failure<IEnumerable<VotesPerDayResponse>>(PollErrors.PollNotFound);

        var votesPerDay = await _context.Votes
            .Where(v => v.PollId == pollId)
            .GroupBy(v => new { Date = DateOnly.FromDateTime(v.SubmittedOn) })
            .Select(g => new VotesPerDayResponse(
                g.Key.Date,
                g.Count()
                ))
                  .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<VotesPerDayResponse>>(votesPerDay);


    }

    public async Task<Result<IEnumerable<VotesPerQuestionResponse>>> GetPollVotesPerQuestionAsync(int pollId, CancellationToken cancellationToken = default)
    {
        var isExistingPoll = await _context.Polls.AnyAsync(p => p.Id == pollId, cancellationToken);

        if (!isExistingPoll)
            return Result.Failure<IEnumerable<VotesPerQuestionResponse>>(PollErrors.PollNotFound);


        // we need to return the questions and the list of ( answer content and the number of votes for this question ) for all votes which occur for this poll
        var votesPerQuestion = await _context.VoteAnswers
            .Where(va => va.Vote.PollId == pollId)
            .Select(va => new VotesPerQuestionResponse(
                va.Question.Content,
                va.Question.VoteAnswers.GroupBy(va => new { AnswerId = va.AnswerId, AnswerContent = va.Answer.Content })
                .Select(g => new VotesPerAnswerResponse(
                    g.Key.AnswerContent,
                    g.Count()
                    ))
                )).ToListAsync(cancellationToken);


        return Result.Success<IEnumerable<VotesPerQuestionResponse>>(votesPerQuestion);

    }
}
