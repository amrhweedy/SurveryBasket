namespace SurveyBasket.Api.Contracts.Requests;

public record CreatePollRequest(string Title,
    string Summary,
    bool IsPublished,
    DateOnly StartsAt,
    DateOnly EndsAt);  // we make this record to make this DTO immutable

