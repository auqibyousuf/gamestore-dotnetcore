namespace GameStore.Backend.Dtos
{
    public class PagedResultDto<T>
    {
        public int Page { get; set; }
        public int Limit { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<T> Items { get; set; } = [];
    }
}
