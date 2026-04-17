using System;

namespace SwoopMarketplaceProject.Models
{
    public partial class Message
    {
        public long Id { get; set; }
        public long FromUserId { get; set; }
        public long ToUserId { get; set; }
        public long? ListingId { get; set; }    // optional, link message to a listing
        public string Content { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        // Soft-delete per participant: true = that user hid the conversation
        public bool DeletedBySender { get; set; } = false;
        public bool DeletedByRecipient { get; set; } = false;
        public bool IsEdited { get; set; }
    }
}