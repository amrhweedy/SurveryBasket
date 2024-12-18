
namespace SurveyBasket.Api.Services;

public class PollService(ApplicationDbContext context) : IPollService
{

    private readonly ApplicationDbContext _context = context;


    public async Task<Poll> AddAsync(Poll poll, CancellationToken cancellationToken)
    {
        await _context.Polls.AddAsync(poll, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return poll;

    }

    public async Task<bool> DeleteAsync(int Id, CancellationToken cancellationToken)
    {
        var poll = await GetAsync(Id, cancellationToken);

        if (poll is null)
            return false;

        _context.Polls.Remove(poll);

        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<Poll?> GetAsync(int Id, CancellationToken cancellationToken) => await _context.Polls.FindAsync(Id, cancellationToken);


    public async Task<IEnumerable<Poll>> GetAllAsync(CancellationToken cancellationToken) => await _context.Polls.AsNoTracking().ToListAsync(cancellationToken);


    public async Task<bool> UpdateAsync(int Id, Poll poll, CancellationToken cancellationToken)
    {
        var existingPoll = await GetAsync(Id, cancellationToken);

        if (existingPoll is null) return false;

        existingPoll.Title = poll.Title;
        existingPoll.Summary = poll.Summary;
        existingPoll.StartsAt = poll.StartsAt;
        existingPoll.EndsAt = poll.EndsAt;
        existingPoll.IsPublished = poll.IsPublished;

        await _context.SaveChangesAsync(cancellationToken);


        return true;
    }

    public async Task<bool> TogglePublishStatusAsync(int Id, CancellationToken cancellationToken = default)
    {
        var poll = await GetAsync(Id, cancellationToken);
        if (poll is null) return false;

        poll.IsPublished = !poll.IsPublished;
        await _context.SaveChangesAsync(cancellationToken);
        return true;


    }
}
