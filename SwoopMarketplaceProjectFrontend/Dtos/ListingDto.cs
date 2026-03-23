namespace SwoopMarketplaceProjectFrontend.Dtos
{
    public class ListingDto
    {
        public long Id { get; set; }

        public long UserId { get; set; }

        public long? CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public string Title { get; set; } = null!;

        public string Description { get; set; } = null!;

        public decimal Price { get; set; }

        public string Condition { get; set; } = null!;

        public string? Status { get; set; }

        public string? Location { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        // 👉 EZ HIÁNYZott A DTO-BÓL, DE NAGYON FONTOS LESZ A KÉPEK MEGJELENÍTÉSÉHEZ
        public List<string> ImageUrls { get; set; } = new();
    }
}