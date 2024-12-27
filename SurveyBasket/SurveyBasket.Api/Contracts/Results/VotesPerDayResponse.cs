namespace SurveyBasket.Api.Contracts.Results;

public record VotesPerDayResponse(DateOnly Day, int NumberOfVotes);
