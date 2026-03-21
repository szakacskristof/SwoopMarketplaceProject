namespace SwoopMarketplaceProjectFrontend.Dtos
{
    public class UserDto
    {
        public long Id { get; set; }
        public string Username { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string Phone { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string? ProfileImageUrl { get; set; }

        public string? Bio { get; set; }

       
    }
}
