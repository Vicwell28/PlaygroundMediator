namespace PlaygroundMediator.DTOs
{
    public class PaginatedResponseDto<T> : ResponseDto<IEnumerable<T>>
    {
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

        // Métodos o propiedades auxiliares si lo deseas
        public void SetPaginationData(int currentPage, int pageSize, int totalCount)
        {
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalCount = totalCount;
        }
    }
}
