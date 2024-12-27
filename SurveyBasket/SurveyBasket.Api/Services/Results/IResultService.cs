using SurveyBasket.Api.Contracts.Results;

namespace SurveyBasket.Api.Services.Results;

public interface IResultService
{
    Task<Result<PollVotesResponse>> GetPollVotesAsync(int pollId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<VotesPerDayResponse>>> GetPollVotesPerDayAsync(int pollId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<VotesPerQuestionResponse>>> GetPollVotesPerQuestionAsync(int pollId, CancellationToken cancellationToken = default);
}
