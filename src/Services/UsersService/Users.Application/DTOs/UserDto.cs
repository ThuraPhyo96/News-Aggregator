using System.ComponentModel.DataAnnotations;

namespace Users.Application.DTOs
{
    public class UserDto
    {
        public string? Id { get; set; }
        public string? Username { get; set; }
    }

    public class CreateUserDto
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }
    }

    public class LoginUserDto
    {
        [Required]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
