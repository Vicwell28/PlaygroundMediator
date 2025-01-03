namespace PlaygroundMediator.DTOs
{
    public class SearchResponseDto<T> : PaginatedResponseDto<T>
    {
        public string? SearchTermUsed { get; set; }
        public Dictionary<string, string>? AppliedFilters { get; set; }
    }
}
