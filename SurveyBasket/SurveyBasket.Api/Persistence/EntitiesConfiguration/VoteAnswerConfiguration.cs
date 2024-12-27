
namespace SurveyBasket.Api.Persistence.EntitiesConfiguration;

public class VoteAnswerConfiguration : IEntityTypeConfiguration<VoteAnswer>
{
    public void Configure(EntityTypeBuilder<VoteAnswer> builder)
    {
        // for the same vote the questionId must not repeat , the voteId and QuestionId must be unique
        // it means it must not send the same question twice for the same vote because the user must answer for the question only once
        builder.HasIndex(x => new { x.VoteId, x.QuestionId }).IsUnique();
    }
}
