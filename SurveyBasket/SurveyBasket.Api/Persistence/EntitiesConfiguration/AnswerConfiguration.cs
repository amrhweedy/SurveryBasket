
namespace SurveyBasket.Api.Persistence.EntitiesConfiguration;

public class AnswerConfiguration : IEntityTypeConfiguration<Answer>
{
    public void Configure(EntityTypeBuilder<Answer> builder)
    {
        builder.Property(x => x.Content).HasMaxLength(1000);

        builder.HasIndex(x => new { x.QuestionId, x.Content }).IsUnique();
        // the content of the answer must be unique for the same question
        // it means the same answer can't be added twice for the same question       
    }
}
