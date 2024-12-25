

namespace SurveyBasket.Api.Controllers;
[Route("api/polls/{pollId}/[controller]")]
[ApiController]
[Authorize]
public class QuestionsController(IQuestionService questionService) : ControllerBase
{
    private readonly IQuestionService _questionService = questionService;


    [HttpGet("{id}")]

    public IActionResult Get()
    {
        return Ok();
    }


    [HttpPost]
    public async Task<IActionResult> Add([FromRoute] int pollId,[FromBody] QuestionRequest request , CancellationToken cancellationToken)
    {

        var result =await  _questionService.AddAsync(pollId, request, cancellationToken);

        if (result.IsSuccess)
            return CreatedAtAction(nameof(Get), new {pollId = pollId ,id = result.Value.Id} , result.Value);

        return result.Error.Equals(QuestionErrors.DuplicatedQuestionContent)
            ? result.ToProblem(status: StatusCodes.Status409Conflict)
            : result.ToProblem(status: StatusCodes.Status404NotFound);

    }
}
