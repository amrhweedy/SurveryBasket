using SurveyBasket.Api.Contracts.Requests;
using SurveyBasket.Api.Contracts.Responses;

namespace SurveyBasket.Api.Mapping;

public static class ContractMapping
{
    public static PollResponse MapToPollResponse(this Poll poll)
    {
        return new()
        {
            Id = poll.Id,
            Title = poll.Title,
            Description = poll.Description,

        };
    }

    public static IEnumerable<PollResponse> MapToPollResponse(this IEnumerable<Poll> polls)
    {
        return polls.Select(MapToPollResponse);

    }

    public static Poll MapToPoll(this CreatePollRequest request)
    {
        return new()
        {
            Title = request.Title,
            Description = request.Description
        };
    }


}
