using SwoopMarketplaceProject.Models;

namespace SwoopMarketplaceProjectBackendAPI.Data
{
    public class ListingSeeder
    {

        public static async Task SeedAsync()
        {
            SwoopContext _sdbc = new SwoopContext();

            string[] conditions = new string[]
            {
                "Új",
                "Kiváló",
                "Kielégítő",
                "Használt",
                "Hibás"

            };




            Listing[] listings = new Listing[]
            {
                new Listing
                {
                    Id = 1,
                    Title = "Eladó Mercedes Benz C63 AMG 2016",
                    Description = "Eladó szeretett Mercedes Benz C63 AMG-m!\nMinden amit tudni kell róla röviden:\n120.000km\nBenzines\nGarázsban Tárolt\nAlufelnivel együtt\tTovábbi infókért keressen meg telefonon!",
                    Price = 5000000,

                    CreatedAt = DateTime.Now,
                    Condition= "Kiváló",
                    Status = "active",
                    Location="Sopron",

                    CategoryId = 1,
                    UserId = 3
                },

            };



            foreach (Listing listing in listings)
            {
                if (!_sdbc.Listings.Any(l => l.Id == listing.Id))
                {
                    await _sdbc.Listings.AddAsync(listing);
                }
            }

            await _sdbc.SaveChangesAsync();



        }
    }
}
