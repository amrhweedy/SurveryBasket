namespace SurveyBasket.Api.Abstractions;

public class PaginatedList<T>(List<T> items, int count, int pageNumber, int pageSize)
{
    public List<T> Items { get; private set; } = items;
    public int PageNumber { get; private set; } = pageNumber;
    public int TotalCount { get; private set; } = count;  // the number of total items
    public int TotalPages { get; private set; } = (int)Math.Ceiling(count / (double)pageSize);  // the front needs this property to display the number of pages

    public bool HasPreviousPage => PageNumber > 1; // the front needs this property to display the previous button 
    public bool HasNextPage => PageNumber < TotalPages; // the front needs this property to display the next button

    public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
    {
        var count = await source.CountAsync();
        var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PaginatedList<T>(items, count, pageNumber, pageSize);
    }
}
