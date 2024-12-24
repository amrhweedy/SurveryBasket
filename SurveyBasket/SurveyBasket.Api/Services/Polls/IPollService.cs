using SurveyBasket.Api.Contracts.Polls;

namespace SurveyBasket.Api.Services.Polls;

public interface IPollService
{
    Task<IEnumerable<Poll>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<PollResponse>> GetAsync(int Id, CancellationToken cancellationToken = default);
    Task<Result<PollResponse>> AddAsync(CreatePollRequest poll, CancellationToken cancellationToken = default);
    Task<Result> UpdateAsync(int Id, CreatePollRequest poll, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int Id, CancellationToken cancellationToken = default);
    Task<Result> TogglePublishStatusAsync(int Id, CancellationToken cancellationToken = default);
}
