namespace SwoopMarketplaceProjectFrontend.Dtos
{
    public class ReportDto
    {
        public int ReportId { get; set; }

        public int ListingId { get; set; }

        public int UserId { get; set; }

        public string Description { get; set; } = null!;
    }
}
