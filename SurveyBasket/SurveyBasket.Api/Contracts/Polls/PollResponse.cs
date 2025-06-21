namespace SurveyBasket.Api.Contracts.Polls;

public record PollResponse(int Id,
    string Title,
    string Summary,
    bool IsPublished,
    DateOnly StartsAt,
    DateOnly EndsAt); // we make it record to make this Dto Immutable



// we make this to test the api version
public record PollResponseV2(int Id,
    string Title,
    string Summary,
    DateOnly StartsAt,
    DateOnly EndsAt); // we make it record to make this Dto Immutable
