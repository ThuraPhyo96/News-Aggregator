using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAggregator.Application.DTOs
{
    public class UserDto
    {
        public string? Id { get; set; }
        public string? Username { get; set; }
    }

    public class CreateUserDto
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }

    public class LoginUserDto
    {
        public string? Username { get; set; }
        public string? Password { get; set; }
    }
}
