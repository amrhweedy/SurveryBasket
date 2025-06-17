namespace SurveyBasket.Api.Abstractions.Consts;

public static class Permissions
{
    // there is a property called ClaimType in the RoleClaims table every claim has a type
    // so when we add any permission like "polls:read" in the column "ClaimValue" we set the ClaimType to "permissions"
    // it means that it may there are many ClaimTypes another "permissions" os we can filter by ClaimType to get all the permissions
    public static string Type { get; } = "permissions";

    public const string GetPolls = "polls:read";
    public const string AddPolls = "polls:add";
    public const string UpdatePolls = "polls:update";
    public const string DeletePolls = "polls:delete";

    public const string GetQuestions = "questions:read";
    public const string AddQuestions = "questions:add";
    public const string updateQuestions = "questions:update";

    public const string GetUsers = "users:read";
    public const string AddUsers = "users:add";
    public const string UpdateUsers = "users:update";

    public const string GetRoles = "roles:read";
    public const string AddRoles = "roles:add";
    public const string UpdateRoles = "roles:update";


    public const string Results = "results:read";

    // get the values of the fields =>  ["polls:read","polls:add","polls:update","polls:delete",....]
    public static IList<string?> GetAllPermissions() =>
        typeof(Permissions).GetFields().Select(f => f.GetValue(f) as string).ToList();

}
