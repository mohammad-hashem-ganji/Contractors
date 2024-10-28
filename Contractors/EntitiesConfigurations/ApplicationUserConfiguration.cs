using Contractors.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Contractors.EntitiesConfigurations
{
    public class ApplicationUserConfiguration
    {
        public static void SeedUsers(ModelBuilder builder)
        {
            var hasher = new PasswordHasher<ApplicationUser>();
            #region seedUsers
            var users = new List<ApplicationUser>()
            {
                new ApplicationUser()
                {
                    Id = 1,
                    UserName = "Client123",
                    NormalizedUserName = "CLIENT123",
                    Email = "admin@gmail.com",
                    NormalizedEmail = "ADMIN@GMAIL.COM",

                    LockoutEnabled = false,
                    PhoneNumber ="09179",
                    SecurityStamp = Guid.NewGuid().ToString()
                }
            };
            #endregion
            #region password
            foreach (var user in users)
            {
                var passwordHasher = new PasswordHasher<ApplicationUser>();
                user.PasswordHash = passwordHasher.HashPassword(user, "Parsian123456ContractorAuthenear");

                builder.Entity<ApplicationUser>().HasData(user);
            }
            #endregion
            #region seed Role To user
            builder.Entity<IdentityUserRole<int>>().HasData(
            new IdentityUserRole<int>() { RoleId = 1, UserId = 1 }
            );
            #endregion
        }
    }
}
