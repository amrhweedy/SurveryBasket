﻿using SurveyBasket.Api.Contracts.Polls;
using SurveyBasket.Api.Errors;

namespace SurveyBasket.Api.Services.Polls;

public class PollService(ApplicationDbContext context) : IPollService
{

    private readonly ApplicationDbContext _context = context;

    public async Task<IEnumerable<Poll>> GetAllAsync(CancellationToken cancellationToken) =>
        await _context.Polls.AsNoTracking().ToListAsync(cancellationToken);


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

        return Result.Success();
    }
}
