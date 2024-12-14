namespace SurveyBasket.Api.Contracts.Requests;

public class CreatePollRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public static explicit operator Poll(CreatePollRequest request) // convert from CreatePollRequest to Poll
    {
        return new Poll()
        {
            Title = request.Title,
            Description = request.Description
        };
    }
}

