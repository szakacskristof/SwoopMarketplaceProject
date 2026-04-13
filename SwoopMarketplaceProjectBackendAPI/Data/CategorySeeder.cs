using SwoopMarketplaceProject.Models;

namespace SwoopMarketplaceProjectBackendAPI.Data
{
    public class CategorySeeder
    {



        public static async Task SeedAsync()
        {
            SwoopContext _sdbc = new SwoopContext();
            string[] categories = new string[] {
            "Autó",
            "Motorkerékpár",
            "Kerékpár",
            "Egyéb Jármű",
            "Elektronikai Eszköz",
            "Bútor",
            "Háztartási Eszköz",
            "Ruházat",
            "Játék",
            "Egyéb",
        };
            int counter = 1;
            foreach (string cat in categories)
            {
                if (!_sdbc.Categories.Any(c => c.Name == cat))
                {
                    await _sdbc.Categories.AddAsync(new Category { Id = counter++, Name = cat });
                }
            }

            await _sdbc.SaveChangesAsync();

        }





    }
}
