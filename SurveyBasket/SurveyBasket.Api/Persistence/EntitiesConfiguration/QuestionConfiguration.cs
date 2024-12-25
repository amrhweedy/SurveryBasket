
namespace SurveyBasket.Api.Persistence.EntitiesConfiguration;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.Property(x=> x.Content).HasMaxLength(1000);

        builder.HasIndex(x=> new {x.PollId , x.Content }).IsUnique(); 
        // the content of the question must be unique for the same poll
        // it means the same question can't be added twice for the same poll
     }
}
