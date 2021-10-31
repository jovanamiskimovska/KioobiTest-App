using Domain.Models;
using System.Collections.Generic;

namespace DataAccess.Interfaces
{
    public interface IRepository
    {
        List<User> GetAll();
        User GetById(int id);
        void Insert(User entity);
        void Update(User entity);
        void Delete(User entity);
        User GetUserByEmail(string email);
        User LoginUser(string email, string password);
    }
}
