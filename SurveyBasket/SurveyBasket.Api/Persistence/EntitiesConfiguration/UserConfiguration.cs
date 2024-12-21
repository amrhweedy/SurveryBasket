
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

    }
}
