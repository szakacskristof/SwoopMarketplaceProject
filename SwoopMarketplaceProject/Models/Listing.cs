using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SwoopMarketplaceProject.Models;

public partial class Listing
{
    public long Id { get; set; }

    public long UserId { get; set; }

    public long? CategoryId { get; set; }

    public string Title { get; set; } = null!;

    public string Description { get; set; } = null!;

    public decimal Price { get; set; }

    public string Condition { get; set; } = null!;

    public string? Status { get; set; }

    public string? Location { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual Category? Category { get; set; }

    public virtual ICollection<ListingImage> ListingImages { get; set; } = new List<ListingImage>();

    [JsonIgnore]
    public virtual ICollection<ListingView> ListingViews { get; set; } = new List<ListingView>();
    [JsonIgnore]
    public virtual User? User { get; set; }
}
