using Microsoft.EntityFrameworkCore;

namespace Contractors.DbContractorsAuctioneerEF
{
    public class DatabaseInitializer
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public DatabaseInitializer(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task InitializeAsync()
        {
            using var serviceScope = _scopeFactory.CreateScope();
            var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await context.Database.MigrateAsync();
        }
    }

}
