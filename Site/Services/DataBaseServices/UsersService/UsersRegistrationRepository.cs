using Microsoft.EntityFrameworkCore;
using Site.Services.DataBaseServices.Tables;
using System.Collections.Generic;

namespace Site.Services.DataBaseServices.UsersService
{
    public class UsersRegistrationRepository : IRepository<UsersRegistration>
    {
        private readonly UsersDbContextMySql dbContext;

        public UsersRegistrationRepository(UsersDbContextMySql dbContext)
        {
            this.dbContext = dbContext;
        }

        public IEnumerable<UsersRegistration> GetList()
        {
            return dbContext.UserRegistration;
        }

        public UsersRegistration Get(int id)
        {
            return dbContext.UserRegistration.Find(id);
        }

        public void Create(UsersRegistration item)
        {
            dbContext.UserRegistration.Add(item);
        }

        public void Update(UsersRegistration item)
        {
            dbContext.Entry(item).State = EntityState.Modified;
        }

        public void Delete(int id)
        {
            UsersRegistration item = dbContext.UserRegistration.Find(id);
            if (item != null)
                dbContext.UserRegistration.Remove(item);
        }
    }
}
