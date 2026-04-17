using System;

namespace SwoopMarketplaceProjectFrontend.Dtos
{
    public class ReportDto
    {
        public long ReportId { get; set; }

        public long ListingId { get; set; }

        public long UserId { get; set; }

        public string Description { get; set; } = null!;

        // Frontend-only enrichment fields (populated by the admin page after fetching reports)
        public string? ReporterEmail { get; set; }
        public string? ReporterUsername { get; set; }
        public DateTime? ReportedAt { get; set; }
    }
}
