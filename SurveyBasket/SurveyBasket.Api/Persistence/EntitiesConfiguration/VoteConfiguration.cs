
namespace SurveyBasket.Api.Persistence.EntitiesConfiguration;

public class VoteConfiguration : IEntityTypeConfiguration<Vote>
{
    public void Configure(EntityTypeBuilder<Vote> builder)
    {
        // the user can vote once for the same poll
        builder.HasIndex(vote => new { vote.PollId, vote.UserId }).IsUnique();

    }
}
