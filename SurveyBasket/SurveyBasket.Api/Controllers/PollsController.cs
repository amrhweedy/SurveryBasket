    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Cors;
    using SurveyBasket.Api.Contracts.Polls;
    using SurveyBasket.Api.Services.Polls;

    namespace SurveyBasket.Api.Controllers;
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]  // it means that the user must be authenticated (have a valid token) to access any endpoint in this controller

    public class PollsController(IPollService pollService) : ControllerBase
    {
        private readonly IPollService _pollService = pollService;


        [HttpGet]
         public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var polls = await _pollService.GetAllAsync(cancellationToken);
            var response = polls.Adapt<IEnumerable<PollResponse>>();
            return Ok(response);
        }

        [HttpGet("{id}")]
         public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
        {
            var poll = await _pollService.GetAsync(id, cancellationToken);

            if (poll is null)
                return NotFound();

            var response = poll.Adapt<PollResponse>();  // Mapster

            return Ok(response);

        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] CreatePollRequest request, CancellationToken cancellationToken)
        {
            var newPoll = await _pollService.AddAsync(request.Adapt<Poll>(), cancellationToken);

            return CreatedAtAction(nameof(Get), new { id = newPoll.Id }, newPoll.Adapt<PollResponse>());  // 201


            // one of the rest guidelines says that when you create a new resource in the server the client must know how can access this resource 
            // so if return ok() the client can't know where the new resource is located
            // so when we use CreatedAtAction we let the client know where the new resource is located it returns status code 201
            // This response indicates that a resource has been successfully created, and it includes a location header that points to the URI of the newly created resource.
            //  location: https://localhost:7265/api/Polls/2   => when i create a new poll this location property appears in the response header to know the client how can access this new poll
            // CreatedAtAction(nameof(Get), new { id = newPoll.Id }, newPoll) => will generate a URL for the Get action, passing in the ID of the new poll (newPoll.Id).
            // it means when i access or call this url https://localhost:7265/api/Polls/2 it will access the Get Action to get the new poll
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] CreatePollRequest request, CancellationToken cancellationToken)
        {
            var isUpdated = await _pollService.UpdateAsync(id, request.Adapt<Poll>(), cancellationToken);
            if (!isUpdated)
                return NotFound();

            return NoContent();  //204

        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
        {
            var isDeleted = await _pollService.DeleteAsync(id, cancellationToken);
            if (!isDeleted)
                return NotFound();

            return NoContent();

        }



        [HttpPut("{id}/togglePublish")]
        public async Task<IActionResult> TogglePublish([FromRoute] int id, CancellationToken cancellationToken)
        {
            var isUpdated = await _pollService.TogglePublishStatusAsync(id, cancellationToken);

            if (!isUpdated)
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
