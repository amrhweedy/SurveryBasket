using SurveyBasket.Api.Contracts.Votes;

namespace SurveyBasket.Api.Services.Votes;

public interface IVoteService
{
    Task<Result> AddAsync(int pollId, string userId, VoteRequest request, CancellationToken cancellationToken = default);
}
