namespace SurveyBasket.Api.Contracts.Responses;

public record PollResponse(int Id, string Title, string Description); // we make it record to make this Dto Immutable 
