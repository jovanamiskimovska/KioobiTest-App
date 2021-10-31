using Dtos.Users;

namespace Services.Interfaces
{
    public interface IUserService
    {
        void Register(RegisterUserDto registerUserDto);
        string Login(LoginDto loginDto);
        void Delete(int id);
        void Update(UpdateUserDto userDto, int id);
    }
}
