namespace SurveyBasket.Api.Entities;

public class ApplicationRole : IdentityRole
{
    public bool IsDefault { get; set; }  // when the users register they take by default role "Member" so i will make this role default because i do not need anyone to modify this role like add or remove permissions
    public bool IsDeleted { get; set; }
}

