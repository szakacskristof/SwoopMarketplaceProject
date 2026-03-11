using System;
using System.Collections.Generic;

namespace SwoopMarketplaceProject.Models;

public partial class Report
{
    public int ReportId { get; set; }

    public int ListingId { get; set; }

    public int UserId { get; set; }

    public string Description { get; set; } = null!;
}
