namespace SurveyBasket.Api.Abstractions;

public static class ResultExtensions
{
    public static ObjectResult ToProblem(this Result result)  // i need finally from this method to return objectResult with problemDetails
    {
        if (result.IsSuccess)
            throw new InvalidOperationException();

        var problem = Results.Problem(statusCode: result.Error.StatusCode);
        // the problem object contains the StatusCode and ProblemDetails Properties and inside the ProblemDetails property is an object contains some properties 
        // these properties are Title and Type and Extensions , if I send the status code 400 to this Problem , the Title which in the problemDetails will be initialize with "Bad Request" and the type will be initialize with the url automatically
        // so the rest is the Extensions property which we need to set the errors property inside it because it is a dictionary of <string, object?>

        ProblemDetails? problemDetails = problem.GetType().GetProperty(nameof(ProblemDetails))!.GetValue(problem) as ProblemDetails;  // cast to convert from object to ProblemDetails

        problemDetails!.Extensions.Add("errors", new[] {
          new
            {
                result.Error.Code,
                result.Error.Description
            }
        });   // to add property errors in the response

        return new ObjectResult(problemDetails);

    }
}
