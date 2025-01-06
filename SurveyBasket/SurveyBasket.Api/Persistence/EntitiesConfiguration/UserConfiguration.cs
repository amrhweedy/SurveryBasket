
using SurveyBasket.Api.Abstractions.Consts;

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

        var passwordHasher = new PasswordHasher<ApplicationUser>();

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
            PasswordHash = passwordHasher.HashPassword(null!, DefaultUsers.AdminPassword),
        });
    }
}
