using Microsoft.EntityFrameworkCore;

namespace SwoopMarketplaceProject.Models
{
    // keep SwoopContext unchanged, add DbSet in a partial file to avoid large diffs
    public partial class SwoopContext
    {
        public DbSet<Bookmark> Bookmarks { get; set; } = null!;
    }
}