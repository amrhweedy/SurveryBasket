namespace SurveyBasket.Api.Contracts.Requests;

public record CreatePollRequest(string Title, string Description);  // we make this record to make this DTO immutable

