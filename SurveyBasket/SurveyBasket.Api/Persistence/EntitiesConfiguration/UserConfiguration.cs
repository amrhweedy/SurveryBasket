namespace SurveyBasket.Api.Persistence.EntitiesConfiguration;

public class UserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FirstName).HasMaxLength(100);
        builder.Property(u => u.LastName).HasMaxLength(100);

        builder.OwnsMany(u => u.RefreshTokens)    // we change the name of RefreshToken table to RefreshTokens
            .ToTable("RefreshTokens")
            .WithOwner()
            .HasForeignKey("UserId");   // we change the foreign key name form ApplicationUserId to UserId

        // seeding data

        //var passwordHasher = new PasswordHasher<ApplicationUser>(); =>
        //we use this in the first seeding data to make hash password
        // but there is a problem when we make a migration it will execute the seeding again
        // and update the password hash with every migration 
        // so we can use it in the first migration and after that take the password hash for the admin and use it in the second migration

        builder.HasData(new ApplicationUser
        {
            Id = DefaultUsers.AdminId,
            Email = DefaultUsers.AdminEmail,
            NormalizedEmail = DefaultUsers.AdminEmail.ToUpper(),
            UserName = DefaultUsers.AdminEmail,
            NormalizedUserName = DefaultUsers.AdminEmail.ToUpper(),
            FirstName = "Amr",
            LastName = "Hweedy",
            SecurityStamp = DefaultUsers.AdminSecurityStamp,
            ConcurrencyStamp = DefaultUsers.AdminConcurrencyStamp,
            EmailConfirmed = true, // to enable admin to login
            //  PasswordHash = passwordHasher.HashPassword(null!, DefaultUsers.AdminPassword),                    
            PasswordHash = DefaultUsers.AdminPassword

        });
    }
}
