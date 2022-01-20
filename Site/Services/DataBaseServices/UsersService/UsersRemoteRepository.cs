using Microsoft.EntityFrameworkCore;
using Site.Services.DataBaseServices.Tables;
using System.Collections.Generic;

namespace Site.Services.DataBaseServices.UsersService
{
    public class UsersRemoteRepository : IRepository<UsersRemote>
    {
        private readonly UsersDbContextMySql dbContext;

        public UsersRemoteRepository(UsersDbContextMySql dbContext)
        {
            this.dbContext = dbContext;
        }

        public IEnumerable<UsersRemote> GetList()
        {
            return dbContext.UserRemoted;
        }

        public UsersRemote Get(int id)
        {
            return dbContext.UserRemoted.Find(id);
        }

        public void Create(UsersRemote item)
        {
            dbContext.UserRemoted.Add(item);
        }

        public void Update(UsersRemote item)
        {
            dbContext.Entry(item).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            UsersRemote item = dbContext.UserRemoted.Find(id);
            if (item != null)
                dbContext.UserRemoted.Remove(item);
        }
    }
}
