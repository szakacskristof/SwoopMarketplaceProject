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
                new Listing {
                    Id = 2,
                    Title = "Yamaha MT-07 2018 eladó",
                    Description = "Megkímélt állapotú Yamaha MT-07.\n35.000 km\nFrissen szervizelve\nElső tulajdonostól",
                    Price = 2100000,
                    CreatedAt = DateTime.Now,
                    Condition = "Kiváló",
                    Status = "active",
                    Location = "Győr",
                    CategoryId = 2,
                    UserId = 4
                },
                new Listing {
                    Id = 3,
                    Title = "Mountain bike 26”",
                    Description = "Használt, de jó állapotú kerékpár.\n21 sebességes\nTerepre is alkalmas",
                    Price = 85000,
                    CreatedAt = DateTime.Now,
                    Condition = "Használt",
                    Status = "active",
                    Location = "Budapest",
                    CategoryId = 3,
                    UserId = 5
                },
                new Listing {
                    Id = 4,
                    Title = "Elektromos roller",
                    Description = "Kb. 2 éves roller.\nAkkumulátor gyenge, javításra szorul.",
                    Price = 40000,
                    CreatedAt = DateTime.Now,
                    Condition = "Hibás",
                    Status = "active",
                    Location = "Debrecen",
                    CategoryId = 4,
                    UserId = 6
                },
                new Listing {
                    Id = 5,
                    Title = "iPhone 13 128GB",
                    Description = "Újszerű állapotú iPhone.\nDobozzal, töltővel\nKarcmentes kijelző",
                    Price = 220000,
                    CreatedAt = DateTime.Now,
                    Condition = "Kiváló",
                    Status = "active",
                    Location = "Szeged",
                    CategoryId = 5,
                    UserId = 7
                },
                new Listing {
                    Id = 6,
                    Title = "Kanapé eladó",
                    Description = "3 személyes kanapé.\nKényelmes, kisebb kopásokkal.",
                    Price = 60000,
                    CreatedAt = DateTime.Now,
                    Condition = "Kielégítő",
                    Status = "active",
                    Location = "Pécs",
                    CategoryId = 6,
                    UserId = 3
                },
                new Listing {
                    Id = 7,
                    Title = "Mosógép működőképes",
                    Description = "Régebbi típus, de még használható.\nEnergiaosztály: B",
                    Price = 30000,
                    CreatedAt = DateTime.Now,
                    Condition = "Használt",
                    Status = "active",
                    Location = "Miskolc",
                    CategoryId = 7,
                    UserId = 4
                },
                new Listing {
                    Id = 8,
                    Title = "Férfi télikabát",
                    Description = "Alig használt kabát.\nMéret: L",
                    Price = 12000,
                    CreatedAt = DateTime.Now,
                    Condition = "Kiváló",
                    Status = "active",
                    Location = "Nyíregyháza",
                    CategoryId = 8,
                    UserId = 5
                },
                new Listing {
                    Id = 9,
                    Title = "Lego készlet",
                    Description = "Hiánytalan Lego City szett.\nDoboz nélkül.",
                    Price = 8000,
                    CreatedAt = DateTime.Now,
                    Condition = "Használt",
                    Status = "active",
                    Location = "Kecskemét",
                    CategoryId = 9,
                    UserId = 6
                },
                new Listing {
                    Id = 10,
                    Title = "Vegyes szerszámcsomag",
                    Description = "Kalapács, csavarhúzók, fogók.\nOtthoni használatra ideális.",
                    Price = 15000,
                    CreatedAt = DateTime.Now,
                    Condition = "Kielégítő",
                    Status = "active",
                    Location = "Szombathely",
                    CategoryId = 10,
                    UserId = 7
                },
                new Listing {
                    Id = 11,
                    Title = "Audi A4 2012",
                    Description = "Megbízható családi autó.\n200.000 km\nDízel",
                    Price = 2800000,
                    CreatedAt = DateTime.Now,
                    Condition = "Használt",
                    Status = "active",
                    Location = "Tatabánya",
                    CategoryId = 1,
                    UserId = 3
                },
                new Listing {
                    Id = 12,
                    Title = "Új gamer billentyűzet",
                    Description = "RGB világítás\nMechanikus kapcsolók\nTeljesen új",
                    Price = 25000,
                    CreatedAt = DateTime.Now,
                    Condition = "Új",
                    Status = "active",
                    Location = "Budapest",
                    CategoryId = 5,
                    UserId = 4
                },
                new Listing {
                    Id = 13,
                    Title = "Gyerek bicikli",
                    Description = "14”-os bicikli\nKisebb hibákkal, de használható",
                    Price = 10000,
                    CreatedAt = DateTime.Now,
                    Condition = "Kielégítő",
                    Status = "active",
                    Location = "Eger",
                    CategoryId = 3,
                    UserId = 5
                },
                new Listing {
                    Id = 14,
                    Title = "Régi fotel",
                    Description = "Felújítandó állapotban.\nKényelmes, de kopott.",
                    Price = 5000,
                    CreatedAt = DateTime.Now,
                    Condition = "Hibás",
                    Status = "active",
                    Location = "Békéscsaba",
                    CategoryId = 6,
                    UserId = 6
                },
                new Listing {
                    Id = 15,
                    Title = "PS4 konzol",
                    Description = "Működik, de hangos a ventilátor.\n1 kontrollerrel.",
                    Price = 60000,
                    CreatedAt = DateTime.Now,
                    Condition = "Kielégítő",
                    Status = "active",
                    Location = "Szolnok",
                    CategoryId = 5,
                    UserId = 7
                }

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
