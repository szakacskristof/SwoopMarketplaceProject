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

                new ListingImage { Id = 3, ImageUrl = "https://localhost:7000/images/mt07_1.jpg", IsPrimary = true, ListingId = 2 },
                new ListingImage { Id = 4, ImageUrl = "https://localhost:7000/images/mt07_2.jpg", ListingId = 2 },

                new ListingImage { Id = 5, ImageUrl = "https://localhost:7000/images/bike_1.jpg", IsPrimary = true, ListingId = 3 },
                new ListingImage { Id = 6, ImageUrl = "https://localhost:7000/images/bike_2.jpg", ListingId = 3 },

                new ListingImage { Id = 7, ImageUrl = "https://localhost:7000/images/scooter_1.jpg", IsPrimary = true, ListingId = 4 },
                new ListingImage { Id = 8, ImageUrl = "https://localhost:7000/images/scooter_2.jpg", ListingId = 4 },

                new ListingImage { Id = 9, ImageUrl = "https://localhost:7000/images/iphone_1.jpeg", IsPrimary = true, ListingId = 5 },
                new ListingImage { Id = 10, ImageUrl = "https://localhost:7000/images/iphone_2.jpg", ListingId = 5 },

                new ListingImage { Id = 11, ImageUrl = "https://localhost:7000/images/sofa_1.jpg", IsPrimary = true, ListingId = 6 },
               

                new ListingImage { Id = 12, ImageUrl = "https://localhost:7000/images/washing_1.jpg", IsPrimary = true, ListingId = 7 },
                

                new ListingImage { Id = 13, ImageUrl = "https://localhost:7000/images/jacket_1.jpg", IsPrimary = true, ListingId = 8 },
                new ListingImage { Id = 14, ImageUrl = "https://localhost:7000/images/jacket_2.jpg", ListingId = 8 },

                new ListingImage { Id = 15, ImageUrl = "https://localhost:7000/images/lego_1.jpg", IsPrimary = true, ListingId = 9 },
                new ListingImage { Id = 16, ImageUrl = "https://localhost:7000/images/lego_2.jpg", ListingId = 9 },

                new ListingImage { Id = 17, ImageUrl = "https://localhost:7000/images/tools_1.jpg", IsPrimary = true, ListingId = 10 },
                new ListingImage { Id = 18, ImageUrl = "https://localhost:7000/images/tools_2.jpg", ListingId = 10 },

                new ListingImage { Id = 19, ImageUrl = "https://localhost:7000/images/audi_1.jpg", IsPrimary = true, ListingId = 11 },
                new ListingImage { Id = 20, ImageUrl = "https://localhost:7000/images/audi_2.jpg", ListingId = 11 },

                new ListingImage { Id = 21, ImageUrl = "https://localhost:7000/images/keyboard_1.jpg", IsPrimary = true, ListingId = 12 },
                

                new ListingImage { Id = 22, ImageUrl = "https://localhost:7000/images/kidbike_1.jpg", IsPrimary = true, ListingId = 13 },
                new ListingImage { Id = 23, ImageUrl = "https://localhost:7000/images/kidbike_2.png", ListingId = 13 },

                new ListingImage { Id = 24, ImageUrl = "https://localhost:7000/images/chair_1.jpeg", IsPrimary = true, ListingId = 14 },
           

                new ListingImage { Id = 25, ImageUrl = "https://localhost:7000/images/ps4_1.jpg", IsPrimary = true, ListingId = 15 },
                new ListingImage { Id = 26, ImageUrl = "https://localhost:7000/images/ps4_2.jpg", ListingId = 15 }

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
