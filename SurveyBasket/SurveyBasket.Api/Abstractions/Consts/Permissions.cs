﻿namespace SurveyBasket.Api.Abstractions.Consts;

public static class Permissions
{
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
