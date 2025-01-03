using Hangfire;
using SurveyBasket.Api.Services.BackgroundJobs;

namespace SurveyBasket.Api.Services.Polls;

public class PollService(ApplicationDbContext context, INotificationService notificationService) : IPollService
{

    private readonly ApplicationDbContext _context = context;
    private readonly INotificationService _notificationService = notificationService;

    public async Task<IEnumerable<PollResponse>> GetAllAsync(CancellationToken cancellationToken) =>
         await _context.Polls
         .ProjectToType<PollResponse>()   // it make this projection in the database to get specific columns not all columns
         .AsNoTracking()
         .ToListAsync(cancellationToken);


    public async Task<IEnumerable<PollResponse>> GetCurrentAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Polls
             .Where(poll => poll.IsPublished && poll.StartsAt <= DateOnly.FromDateTime(DateTime.UtcNow) && poll.EndsAt >= DateOnly.FromDateTime(DateTime.UtcNow))
             .ProjectToType<PollResponse>()
             .AsNoTracking()
             .ToListAsync(cancellationToken);
    }

    public async Task<Result<PollResponse>> GetAsync(int Id, CancellationToken cancellationToken)
    {
        var poll = await _context.Polls.FindAsync(Id, cancellationToken);
        return poll is null ? Result.Failure<PollResponse>(PollErrors.PollNotFound) : Result.Success(poll.Adapt<PollResponse>());
    }

    public async Task<Result<PollResponse>> AddAsync(CreatePollRequest request, CancellationToken cancellationToken)
    {
        if (await _context.Polls.AnyAsync(poll => poll.Title == request.Title, cancellationToken))
        {
            return Result.Failure<PollResponse>(PollErrors.DuplicatedPollTitle);
        }

        var poll = request.Adapt<Poll>();
        await _context.Polls.AddAsync(poll, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(poll.Adapt<PollResponse>());

    }

    public async Task<Result> UpdateAsync(int Id, CreatePollRequest request, CancellationToken cancellationToken)
    {
        var isExistingTitle = await _context.Polls.AnyAsync(poll => poll.Title == request.Title && poll.Id != Id, cancellationToken);

        if (isExistingTitle)
            return Result.Failure<PollResponse>(PollErrors.DuplicatedPollTitle);

        var currentPoll = await _context.Polls.FindAsync(Id, cancellationToken);

        if (currentPoll is null)
            return Result.Failure(PollErrors.PollNotFound);

        currentPoll.Title = request.Title;
        currentPoll.Summary = request.Summary;
        currentPoll.StartsAt = request.StartsAt;
        currentPoll.EndsAt = request.EndsAt;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }


    public async Task<Result> DeleteAsync(int Id, CancellationToken cancellationToken)
    {
        var poll = await _context.Polls.FindAsync(Id, cancellationToken);

        if (poll is null)
            return Result.Failure(PollErrors.PollNotFound);

        _context.Polls.Remove(poll);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }


    public async Task<Result> TogglePublishStatusAsync(int Id, CancellationToken cancellationToken = default)
    {
        var poll = await _context.Polls.FindAsync(Id, cancellationToken);

        if (poll is null)
            return Result.Failure(PollErrors.PollNotFound);

        poll.IsPublished = !poll.IsPublished;

        await _context.SaveChangesAsync(cancellationToken);

        if (poll.IsPublished && poll.StartsAt == DateOnly.FromDateTime(DateTime.UtcNow))
        {
            // this job will be executed immediately when this method is executed
            BackgroundJob.Enqueue(() => _notificationService.SendNewPollsNotification(poll.Id));
        }

        return Result.Success();
    }


}
