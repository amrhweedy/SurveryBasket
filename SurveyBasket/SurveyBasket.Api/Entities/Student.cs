
namespace SurveyBasket.Api.Entities;

public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string MiddleName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    //[MinAge(18), Display(Name = "Date of Birth")]
    public DateTime? DateOfBirth { get; set; }
    public Department Department { get; set; } = default!;
}



public class Department
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

}

