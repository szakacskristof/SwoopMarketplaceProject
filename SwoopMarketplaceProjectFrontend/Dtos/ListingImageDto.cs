namespace SwoopMarketplaceProjectFrontend.Dtos
{
    public class ListingImageDto
    {
        public long Id { get; set; }
        public long ListingId { get; set; }

        public string ImageUrl { get; set; } = null!;

        public bool? IsPrimary { get; set; }
    }
}
