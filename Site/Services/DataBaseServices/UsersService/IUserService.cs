using System.Threading.Tasks;

namespace Site.Services.DataBaseServices.UsersService
{
    public interface IUserService
    {
        // Users
        UsersRegistrationRepository UsersRegistration { get; }
        UsersRepository Users { get; }
        UsersRecoveryRepository UsersRecovery { get; }
        UsersDeletedRepository UsersDeleted { get; }
        UsersRemoteRepository UsersRemoted { get; }

        Task SaveAsync();
    }
}
