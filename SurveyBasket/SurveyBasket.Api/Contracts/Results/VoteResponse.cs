namespace SurveyBasket.Api.Contracts.Results;

public record VoteResponse(
    string VoterName,
    DateTime VotedDate,
    IEnumerable<QuestionAnswerResponse> SelectedAnswers
    );

