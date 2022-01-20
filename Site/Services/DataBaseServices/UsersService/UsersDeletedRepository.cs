using Microsoft.EntityFrameworkCore;
using Site.Services.DataBaseServices.Tables;
using System.Collections.Generic;

namespace Site.Services.DataBaseServices.UsersService
{
    public class UsersDeletedRepository : IRepository<UsersDeleted>
    {
        private readonly UsersDbContextMySql dbContext;

        public UsersDeletedRepository(UsersDbContextMySql dbContext)
        {
            this.dbContext = dbContext;
        }

        public IEnumerable<UsersDeleted> GetList()
        {
            return dbContext.UserDeleted;
        }

        public UsersDeleted Get(int id)
        {
            return dbContext.UserDeleted.Find(id);
        }

        public void Create(UsersDeleted item)
        {
            dbContext.UserDeleted.Add(item);
        }

        public void Update(UsersDeleted item)
        {
            dbContext.Entry(item).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            UsersDeleted item = dbContext.UserDeleted.Find(id);
            if (item != null)
                dbContext.UserDeleted.Remove(item);
        }
    }
}
