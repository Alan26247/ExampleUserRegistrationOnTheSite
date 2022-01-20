using Microsoft.EntityFrameworkCore;
using Site.Services.DataBaseServices.Tables;
using System.Collections.Generic;

namespace Site.Services.DataBaseServices.UsersService
{
    public class UsersRecoveryRepository : IRepository<UsersRecovery>
    {
        private readonly UsersDbContextMySql dbContext;

        public UsersRecoveryRepository(UsersDbContextMySql dbContext)
        {
            this.dbContext = dbContext;
        }

        public IEnumerable<UsersRecovery> GetList()
        {
            return dbContext.UsersRecovery;
        }

        public UsersRecovery Get(int id)
        {
            return dbContext.UsersRecovery.Find(id);
        }

        public void Create(UsersRecovery item)
        {
            dbContext.UsersRecovery.Add(item);
        }

        public void Update(UsersRecovery item)
        {
            dbContext.Entry(item).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            UsersRecovery item = dbContext.UsersRecovery.Find(id);
            if (item != null)
                dbContext.UsersRecovery.Remove(item);
        }
    }
}