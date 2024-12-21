namespace SurveyBasket.Api.Entities;

[Owned]
public class RefreshToken
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresOn { get; set; }
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    public DateTime? RevokedOn { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresOn;
    public bool IsActive => RevokedOn is null && !IsExpired;

}


// note => although we make this class owned to the ApplicationUser class, the entity will create a table for this class in the database
// because the ApplicationUser class contains list of RefreshTokens so the relationship is one to many 
// look at the migration file




// IsExpired and IsActive not mapped to column in the database  because  EF Core only maps properties that:
//Have a getter and setter.
//Are not ignored explicitly via configuration.
//Are not derived or calculated at runtime.

// and these 2 properties are calculated at runtime

//If You Want It to Stay Calculated but Still Be a Column
//You can use a computed column in the database if you want the Date property to always represent the current time but still have a database column.

//Fluent API Example:

//protected override void OnModelCreating(ModelBuilder modelBuilder)
//{
//    modelBuilder.Entity<User>()
//    .Property(u => u.Date)
//        .HasComputedColumnSql("GETUTCDATE()"); // SQL to compute the value
//}

//This configuration tells EF Core that:

//The Date column should be a computed column in the database, using the SQL function GETUTCDATE().
//In this case:

//The Date property is backed by a column in the database.
//The value is dynamically calculated by the database whenever a row is accessed.

