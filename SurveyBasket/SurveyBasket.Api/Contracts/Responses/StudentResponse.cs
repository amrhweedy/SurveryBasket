namespace SurveyBasket.Api.Contracts.Responses;

public class StudentResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int? Age { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
}
