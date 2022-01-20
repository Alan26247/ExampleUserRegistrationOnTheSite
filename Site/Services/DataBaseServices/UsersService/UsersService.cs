using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System;

namespace Site.Services.DataBaseServices.UsersService
{
    public class UsersService : IUserService, IDisposable
    {
        public UsersService(IConfiguration configuration)
        {
            dbContext = new UsersDbContextMySql(configuration);

            //------ Users
            UsersRegistration = new UsersRegistrationRepository(dbContext);
            Users = new UsersRepository(dbContext);
            UsersRecovery = new UsersRecoveryRepository(dbContext);
            UsersDeleted = new UsersDeletedRepository(dbContext);
            UsersRemoted = new UsersRemoteRepository(dbContext);
    }

        private readonly UsersDbContextMySql dbContext;

        //------- Users
        public UsersRegistrationRepository UsersRegistration { get; private set; }
        public UsersRepository Users { get; private set; }
        public UsersRecoveryRepository UsersRecovery { get; private set; }
        public UsersDeletedRepository UsersDeleted { get; private set; }
        public UsersRemoteRepository UsersRemoted { get; private set; }



        public async Task SaveAsync()
        {
            await dbContext.SaveChangesAsync();
        }

        private bool disposed = false;

        public virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    dbContext.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
