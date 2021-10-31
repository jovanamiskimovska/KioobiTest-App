using DataAccess.Interfaces;
using Domain.Enums;
using Domain.Models;
using Dtos.Users;
using Mappers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Services.Interfaces;
using Shared;
using Shared.CustomExceptions;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Services.Implementations
{
    public class UserService : IUserService
    {
        private IRepository _userRepository;
        IOptions<AppSettings> _options;
        public UserService(IRepository userRepository, IOptions<AppSettings> options)
        {
            _userRepository = userRepository;
            _options = options;
        }
        public void Register(RegisterUserDto registerUserDto)
        {
            ValidateUser(registerUserDto);

            MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
            byte[] passwordBytes = Encoding.ASCII.GetBytes(registerUserDto.Password);
            byte[] passwordHash = mD5CryptoServiceProvider.ComputeHash(passwordBytes);
            string hashedPasword = Encoding.ASCII.GetString(passwordHash);

            User newUser = new User
            {
                FirstName = registerUserDto.FirstName,
                LastName = registerUserDto.LastName,
                Email = registerUserDto.Email,
                Role = (Role)registerUserDto.Role,
                Password = hashedPasword
            };
            _userRepository.Insert(newUser);
        }
        public string Login(LoginDto loginDto)
        {
            MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
            byte[] hashedBytes = mD5CryptoServiceProvider.ComputeHash(Encoding.ASCII.GetBytes(loginDto.Password));
            string hashedPassword = Encoding.ASCII.GetString(hashedBytes);

            User userDb = _userRepository.LoginUser(loginDto.Email, hashedPassword);
            if (userDb == null)
            {
                throw new UserException($"Could not login user {loginDto.Email}");
            }


            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            byte[] secretKeyBytes = Encoding.ASCII.GetBytes(_options.Value.SecretKey);

            SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKeyBytes),
                    SecurityAlgorithms.HmacSha256Signature),
                Subject = new ClaimsIdentity(
                    new[]
                   {
                        new Claim(ClaimTypes.Name, userDb.Email),
                        new Claim(ClaimTypes.NameIdentifier, userDb.Id.ToString()),
                        new Claim("userFullName", $"{userDb.FirstName} {userDb.LastName}"),
                        new Claim(ClaimTypes.Role, userDb.Role.ToString())
                    }
                )
            };

            SecurityToken token = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);
            return jwtSecurityTokenHandler.WriteToken(token);
        }

        private void ValidateUser(RegisterUserDto registerUserDto)
        {
            if (string.IsNullOrEmpty(registerUserDto.Email) || string.IsNullOrEmpty(registerUserDto.Password))
            {
                throw new UserException("Email and password are required fields!");
            }
            if (string.IsNullOrEmpty(registerUserDto.Role.ToString()))
            {
                throw new UserException("Role is a required field!");
            }
            if (registerUserDto.Email.Length > 30)
            {
                throw new UserException("Username can contain maximum 30 characters!");
            }
            if (registerUserDto.FirstName.Length > 50 || registerUserDto.LastName.Length > 50)
            {
                throw new UserException("First name and last name can contain maximum 50 characters!");
            }
            if (!IsUserNameUnique(registerUserDto.Email))
            {
                throw new UserException("A user with this email already exists!");
            }
            if (registerUserDto.Password != registerUserDto.ConfirmedPassword)
            {
                throw new UserException("The passwords do not match!");
            }
            if (!IsPasswordValid(registerUserDto.Password))
            {
                throw new UserException("The password is not complex enough!");
            }
        }

        private bool IsUserNameUnique(string email)
        {
            return _userRepository.GetUserByEmail(email) == null;
        }

        private bool IsPasswordValid(string password)
        {
            Regex passwordRegex = new Regex("^(?=.*[0-9])(?=.*[a-z]).{6,20}$");
            return passwordRegex.Match(password).Success;
        }

        public void Delete(int id)
        {
            _userRepository.Delete(_userRepository.GetById(id));
        }

        public void Update(UpdateUserDto userDto, int id)
        {
            User userDb = _userRepository.GetById(id);
            User userUpdated = userDto.ToUser();

            MD5CryptoServiceProvider mD5CryptoServiceProvider = new MD5CryptoServiceProvider();
            byte[] hashedBytes = mD5CryptoServiceProvider.ComputeHash(Encoding.ASCII.GetBytes(userUpdated.Password));
            string hashedPassword = Encoding.ASCII.GetString(hashedBytes);

            userDb.Email = userUpdated.Email;
            userDb.Password = hashedPassword;
            userDb.FirstName = userUpdated.FirstName;
            userDb.LastName = userUpdated.LastName;

            _userRepository.Update(userDb);
        }
    }
}
