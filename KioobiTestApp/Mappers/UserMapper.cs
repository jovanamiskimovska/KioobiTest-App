using Domain.Models;
using Dtos.Users;

namespace Mappers
{
    public static class UserMapper
    {
        public static User ToUser(this UpdateUserDto updateUserDto)
        {
            return new User()
            {
                Email = updateUserDto.Email,
                FirstName = updateUserDto.FirstName,
                LastName = updateUserDto.LastName,
                Password = updateUserDto.Password
            };
        }
    }
}
