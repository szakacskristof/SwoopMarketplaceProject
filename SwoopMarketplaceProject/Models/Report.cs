using System;
using System.Collections.Generic;

namespace SwoopMarketplaceProject.Models;

public partial class Report
{
    public long ReportId { get; set; }

    public long ListingId { get; set; }

    public long UserId { get; set; }

    public string Description { get; set; } = null!;

    public virtual User? User { get; set; }

    public virtual Listing? Listing { get; set; }
}
