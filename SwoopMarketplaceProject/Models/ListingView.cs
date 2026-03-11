using System;
using System.Collections.Generic;

namespace SwoopMarketplaceProject.Models;

public partial class ListingView
{
    public int Id { get; set; }

    public long? ViewsCount { get; set; }

    public long? ListingId { get; set; }

    public virtual Listing? Listing { get; set; }
}
