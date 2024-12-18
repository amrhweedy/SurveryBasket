
namespace SurveyBasket.Api.Services;

public interface IPollService
{
    Task<IEnumerable<Poll>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Poll?> GetAsync(int Id, CancellationToken cancellationToken = default);
    Task<Poll> AddAsync(Poll poll, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(int Id, Poll poll, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int Id, CancellationToken cancellationToken = default);
    Task<bool> TogglePublishStatusAsync(int Id, CancellationToken cancellationToken = default);
}
