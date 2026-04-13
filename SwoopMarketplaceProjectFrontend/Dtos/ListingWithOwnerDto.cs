namespace SwoopMarketplaceProjectFrontend.Dtos
{
    public class ListingWithOwnerDto
    {
        public ListingDto Listing { get; set; } = new();
        public string? OwnerEmail { get; set; }
        public string? OwnerProfileImageUrl { get; set; } // optional absolute URL to owner's profile image (frontend will prefer this)
        // frontend-only helper: whether current user bookmarked this listing
        public bool IsBookmarked { get; set; }
    }
}