
namespace SurveyBasket.Api.Services;

public class PollService : IPollService
{

    // readonly => we can not assign this reference(_polls) to the another list of polls unless in the constructor
    // static => we can access it without creating an instance and make this list shared between all instances

    private static readonly List<Poll> _polls = [
        new Poll
        {
            Id = 1,
            Title = "Poll 1",
            Description = "My first poll"
        }
        ];


    public Poll Add(Poll poll)
    {
        poll.Id = _polls.Count + 1;
        _polls.Add(poll);
        return poll;

    }

    public bool Delete(int Id)
    {
        var poll = Get(Id);

        if (poll is null)
            return false;

        _polls.Remove(poll);

        return true;
    }

    public Poll? Get(int Id) => _polls.SingleOrDefault(pol => pol.Id == Id);


    public IEnumerable<Poll> GetAll() => _polls;


    public bool Update(int Id, Poll poll)
    {
        var existingPoll = Get(Id);

        if (existingPoll is null) return false;

        existingPoll.Title = poll.Title;
        existingPoll.Description = poll.Description;

        return true;
    }
}
