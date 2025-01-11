namespace SurveyBasket.Api.Contracts.Common;

public record RequestFilters
{
    private const int _maxPageSize = 50;
    private int _pageNumber = 1;
    private int _pageSize = 10;

    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 10 : (value > _maxPageSize ? _maxPageSize : value);
    }

    public string? SearchValue { get; init; }
    public string? SortColumn { get; init; }
    public string? SortDirection { get; init; } = "ASC"; // we write ASC or asc or DESC or desc or Asc or DeSc or ..., it's not case sensitive

}
