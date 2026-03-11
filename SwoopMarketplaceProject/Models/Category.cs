using System;
using System.Collections.Generic;

namespace SwoopMarketplaceProject.Models;

public partial class Category
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Listing> Listings { get; set; } = new List<Listing>();
}
