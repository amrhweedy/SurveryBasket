namespace SurveyBasket.Api.Helpers;

public static class EmailBodyBuilder
{
    // this method is used to replace the vairables in the email body like name and url with the actual values and return the final temaplate
    public static string GenerateEmailBody(string template, Dictionary<string , string> temaplateModel)
    {
        var templatePath = $"{Directory.GetCurrentDirectory()}/Templates/{template}.html";
        var streamReader = new StreamReader(templatePath);  // to read the content of the HTML template
        var body = streamReader.ReadToEnd();  //reads the entire file as a single string
        streamReader.Close();

        foreach (var item in temaplateModel)
        {
            body = body.Replace(item.Key, item.Value);
        }

        return body;

    }
}
