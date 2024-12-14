
namespace SurveyBasket.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class PollsController(IPollService pollService) : ControllerBase
{
    private readonly IPollService _pollService = pollService;


    [HttpGet]
    public IActionResult GetAll()
    {
        var polls = _pollService.GetAll();
        return Ok(polls);
    }

    [HttpGet("{id}")]
    public IActionResult Get([FromRoute] int id)
    {
        var poll = _pollService.Get(id);
        if (poll is null)
            return NotFound();

        PollResponse response = poll;

        return Ok(response);

    }

    [HttpPost]
    public IActionResult Add([FromBody] CreatePollRequest request)
    {
        var newPoll = _pollService.Add(request);

        return CreatedAtAction(nameof(Get), new { id = newPoll.Id }, newPoll);

        // one of the rest guidelines says that when you create a new resource in the server the client must know how can access this resource 
        // so if return ok() the client can't know where the new resource is located
        // so when we use CreatedAtAction we let the client know where the new resource is located it returns status code 201
        // This response indicates that a resource has been successfully created, and it includes a location header that points to the URI of the newly created resource.
        //  location: https://localhost:7265/api/Polls/2   => when i create a new poll this location property appears in the response header to know the client how can access this new poll
        // CreatedAtAction(nameof(Get), new { id = newPoll.Id }, newPoll) => will generate a URL for the Get action, passing in the ID of the new poll (newPoll.Id).
        // it means when i access or call this url https://localhost:7265/api/Polls/2 it will access the Get Action to get the new poll
    }

    [HttpPut("{id}")]
    public IActionResult Update([FromRoute] int id, [FromBody] CreatePollRequest request)
    {
        var isUpdated = _pollService.Update(id, request);
        if (!isUpdated)
            return NotFound();

        return NoContent();

    }

    [HttpDelete("{id}")]
    public IActionResult Delete([FromRoute] int id)
    {
        var isDeleted = _pollService.Delete(id);
        if (!isDeleted)
            return NotFound();

        return NoContent();

    }

    [HttpGet("test-header")]
    public IActionResult TestHeader([FromHeader(Name = "x-lang")] string lang)
    {
        return Ok(lang);
    }

    [HttpGet("test-query")]
    public IActionResult TestQuery([FromQuery] int[] id)
    {
        return Ok(id);
    }

}
