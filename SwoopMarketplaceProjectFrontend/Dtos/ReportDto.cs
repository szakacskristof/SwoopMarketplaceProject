namespace SwoopMarketplaceProjectFrontend.Dtos
{
    public class ReportDto
    {
        public long ReportId { get; set; }

        public long ListingId { get; set; }

        public long UserId { get; set; }

        public string Description { get; set; } = null!;
    }
}
