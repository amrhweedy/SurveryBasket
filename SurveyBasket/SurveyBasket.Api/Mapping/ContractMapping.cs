namespace SurveyBasket.Api.Mapping;


// manual mapping using extensions methods
public static class ContractMapping
{
    public static PollResponse MapToPollResponse(this Poll poll)
    {
        return new PollResponse(poll.Id, poll.Title, poll.Description);
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
