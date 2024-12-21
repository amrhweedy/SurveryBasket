namespace SurveyBasket.Api.Contracts.Polls;

public record CreatePollRequest(string Title,
    string Summary,
    DateOnly StartsAt,
    DateOnly EndsAt);  // we make this record to make this DTO immutable

