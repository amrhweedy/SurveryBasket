namespace SurveyBasket.Api.Contracts.Responses;

public record PollResponse(int Id,
     string Title,
    string Summary,
     bool IsPublished,
    DateOnly StartsAt,
    DateOnly EndsAt); // we make it record to make this Dto Immutable
