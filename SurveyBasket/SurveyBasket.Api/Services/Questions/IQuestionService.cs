using SurveyBasket.Api.Contracts.Questions;

namespace SurveyBasket.Api.Services.Questions;

public interface IQuestionService
{
    Task<Result<QuestionResponse>> AddAsync(int pollId, QuestionRequest request, CancellationToken cancellationToken= default);
}
