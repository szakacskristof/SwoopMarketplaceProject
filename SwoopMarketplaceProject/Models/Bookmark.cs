using System;

namespace SwoopMarketplaceProject.Models
{
    public partial class Bookmark
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public long ListingId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}