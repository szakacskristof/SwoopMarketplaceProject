using SwoopMarketplaceProject.Models;

namespace SwoopMarketplaceProjectBackendAPI.Data
{
    public class ImageSeeder
    {
        public static async Task SeedAsync()
        {
            SwoopContext _sdbc= new SwoopContext();

            ListingImage[] images = new ListingImage[]
            {
                new ListingImage
                {
                    Id = 1,
                    ImageUrl = "https://localhost:7000/images/c63_1.jpg",
                    IsPrimary= true,
                    ListingId = 1
                },
                new ListingImage
                {
                    Id = 2,
                    ImageUrl = "https://localhost:7000/images/c63_2.jpg",
                    ListingId = 1
                },
               
            };


            foreach(ListingImage img in images)
            {
                if (!_sdbc.ListingImages.Any(i => i.Id == img.Id))
                {
                     await _sdbc.ListingImages.AddAsync(img);
                }
            }
           await  _sdbc.SaveChangesAsync();




        }
    }
}
