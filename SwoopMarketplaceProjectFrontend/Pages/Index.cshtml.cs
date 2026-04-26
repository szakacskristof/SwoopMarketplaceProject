using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SwoopMarketplaceProjectFrontend.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        // Constructor: initialize home page model with logger.

        // OnGet: handle GET requests for the home page (no special logic).
        public void OnGet()
        {

        }
    }
}
