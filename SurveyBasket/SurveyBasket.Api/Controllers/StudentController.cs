using SurveyBasket.Api.Contracts.Students;

namespace SurveyBasket.Api.Controllers;
[Route("api/[controller]")]
[ApiController]
public class StudentController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {

        var student = new Student()
        {
            Id = 1,
            FirstName = "amr",
            MiddleName = "ahmed",
            LastName = "hweedy",
            DateOfBirth = new DateTime(1997, 8, 27),
            Department = new Department
            {
                Id = 1,
                Name = "IT"
            }

        };

        var studentResponse = student.Adapt<StudentResponse>();
        return Ok(studentResponse);
    }



    [HttpPost]
    public IActionResult Post([FromBody] Student student)
    {
        return Ok(student);
    }

}
