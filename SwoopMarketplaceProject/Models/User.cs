using System;
using System.Collections.Generic;

namespace SwoopMarketplaceProject.Models;

public partial class User
{
    public long Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? ProfileImageUrl { get; set; }

    public string? Bio { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<Listing> Listings { get; set; } = new List<Listing>();
}
