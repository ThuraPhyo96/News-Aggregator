using Users.Application.DTOs;
using Users.Domain.Models;

namespace Users.Application.Mappers
{
    public static class UserMapper
    {
        public static UserDto? ToDto(User user)
        {
            if (user == null) return null;

            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
            };
        }

        public static List<UserDto> ToDtoList(List<User> users)
        {
            if (users == null) return [];

            return users.Select(ToDto).ToList()!;
        }

        public static User? ToEntity(CreateUserDto user)
        {
            if (user == null) return null;

            return new User(user.Username?.ToLowerInvariant(), user.Password);
        }
    }
}
