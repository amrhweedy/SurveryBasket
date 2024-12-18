namespace SurveyBasket.Api.Persistence.EntitiesConfiguration;

public class PollConfiguration : IEntityTypeConfiguration<Poll>
{
    public void Configure(EntityTypeBuilder<Poll> builder)
    {
        // to make the title unique , the title must have an index because, if i have a column in the database unique constraint the database under the hood create an index on it ,
        // so here using entity the title must have an index if i need to make it unique
        // we can not make column unique without an index,  Databases use unique indexes under the hood to enforce uniqueness. If you want the column to be unique, the database will always create an index to efficiently check for duplicates.
        // In relational databases, the uniqueness of a column is enforced using a unique constraint or a unique index.
        //When you declare that a column should be unique, the database creates an index behind the scenes to keep track of the values in that column and ensure no duplicates are allowed.
        //An index provides the database with a fast way to check if the value you are inserting or updating already exists.
        //Without an index, the database would need to scan the entire table row by row(a full table scan) to see if a duplicate value exists, which would be slow and inefficient.


        builder.HasIndex(p => p.Title).IsUnique();
        builder.Property(p => p.Title).HasMaxLength(100);

        builder.Property(p => p.Summary).HasMaxLength(1000);
    }
}
