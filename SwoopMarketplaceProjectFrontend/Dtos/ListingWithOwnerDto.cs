namespace SwoopMarketplaceProjectFrontend.Dtos
{
    public class ListingWithOwnerDto
    {
        public ListingDto Listing { get; set; } = new();
        public string? OwnerEmail { get; set; }
        public string? OwnerUserName { get; set; }
        public string? OwnerProfileImageUrl { get; set; }
        public bool IsBookmarked { get; set; }
    }
}