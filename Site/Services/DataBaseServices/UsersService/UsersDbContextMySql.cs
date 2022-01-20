using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Site.Services.DataBaseServices.Tables;
 
namespace Site.Services.DataBaseServices.UsersService
{
    public class UsersDbContextMySql : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<UsersRegistration> UserRegistration { get; set; }      // пользователи на подтверждение регистации
        public DbSet<Users> Users { get; set; }                             // пользователи
        public DbSet<UsersRecovery> UsersRecovery { get; set; }             // пользователи на восстановление
        public DbSet<UsersDeleted> UserDeleted { get; set; }                // пользователи на подтверждение на удаление
        public DbSet<UsersRemote> UserRemoted { get; set; }                 // удаленные пользователи

        //------ Items
        readonly string connectionString;
        readonly Version version;



        public UsersDbContextMySql(IConfiguration configuration)
        {
            this.connectionString = configuration.GetConnectionString("ConnectionStringMySql");

            this.version = new Version(int.Parse(configuration[$"VersionMySql:Major"]),
                                        int.Parse(configuration[$"VersionMySql:Minor"]),
                                            int.Parse(configuration[$"VersionMySql:Build"]));

            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseMySql(connectionString, new MySqlServerVersion(version));
        }
    }
}