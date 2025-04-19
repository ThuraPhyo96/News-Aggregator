using NewsAggregator.Application.DTOs;
using NewsAggregator.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAggregator.Application.Mappers
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

            return new User(user.Username, user.Password);
        }
    }
}
