using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Site.Services.DataBaseServices.Tables;
using System.Collections.Generic;

namespace Site.Services.DataBaseServices.UsersService
{
    public class UsersRepository : IRepository<Users>
    {
        private readonly UsersDbContextMySql dbContext;

        public UsersRepository(UsersDbContextMySql dbContext)
        {
            this.dbContext = dbContext;
        }

        public IEnumerable<Users> GetList()
        {
            return dbContext.Users;
        }

        public Users Get(int id)
        {
            return dbContext.Users.Find(id);
        }

        public void Create(Users item)
        {
            dbContext.Users.Add(item);
        }

        public void Update(Users item)
        {
            dbContext.Entry(item).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            Users item = dbContext.Users.Find(id);
            if (item != null)
                dbContext.Users.Remove(item);
        }
    }
}
