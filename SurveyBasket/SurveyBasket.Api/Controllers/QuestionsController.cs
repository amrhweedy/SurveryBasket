﻿

using SurveyBasket.Api.Contracts.Common;

namespace SurveyBasket.Api.Controllers;
[Route("api/polls/{pollId}/[controller]")]
[ApiController]
//[Authorize]
public class QuestionsController(IQuestionService questionService) : ControllerBase
{
    private readonly IQuestionService _questionService = questionService;


    [HttpGet]
    [HasPermission(Permissions.GetQuestions)]
    public async Task<IActionResult> GetAll([FromRoute] int pollId, [FromQuery] RequestFilters filters, CancellationToken cancellationToken)
    {
        var result = await _questionService.GetAllAsync(pollId, filters, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpGet("{id}")]
    [HasPermission(Permissions.GetQuestions)]
    public async Task<IActionResult> Get([FromRoute] int pollId, [FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _questionService.GetAsync(pollId, id, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpPost]
    [HasPermission(Permissions.AddQuestions)]
    public async Task<IActionResult> Add([FromRoute] int pollId, [FromBody] QuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _questionService.AddAsync(pollId, request, cancellationToken);

        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { pollId = pollId, id = result.Value.Id }, result.Value)
            : result.ToProblem();

    }

    [HttpPut("{id}")]
    [HasPermission(Permissions.updateQuestions)]
    public async Task<IActionResult> Update([FromRoute] int pollId, [FromRoute] int id, [FromBody] QuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _questionService.UpdateAsync(pollId, id, request, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblem();
    }


    [HttpPut("{id}/toggleStatus")]
    [HasPermission(Permissions.updateQuestions)]
    public async Task<IActionResult> ToggleStatus([FromRoute] int pollId, [FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await _questionService.ToggleStatsAsync(pollId, id, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblem();
    }

}
