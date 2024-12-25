namespace SurveyBasket.Api.Entities;

public sealed class Answer
{
    public int Id { get; set; }
    public string Content { get; set; } = default!;
    public bool IsActive { get; set; } = true;  
    public int QuestionId { get; set; }
    public Question Question { get; set; } = default!;
   
}
