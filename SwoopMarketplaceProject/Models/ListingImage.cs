using System;
using System.Collections.Generic;

namespace SwoopMarketplaceProject.Models;

public partial class ListingImage
{
    public long Id { get; set; }

    public long ListingId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public bool? IsPrimary { get; set; }

    public virtual Listing Listing { get; set; } = null!;
}
