namespace SwoopMarketplaceProjectFrontend.Dtos
{
    public class ListingWithOwnerDto
    {
        public ListingDto Listing { get; set; } = new();
        public string? OwnerEmail { get; set; }
    }
}