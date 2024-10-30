
using Azure.Core;
using Contractors.DbContractorsAuctioneerEF;
using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using System.Diagnostics.Contracts;

namespace ContractorsAuctioneer.Services
{
    public class BidOfContractorCheckService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(3);
        public BidOfContractorCheckService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CheckBidOfContractorsAsync(stoppingToken);
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
        private async Task CheckBidOfContractorsAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var bidStatusService = scope.ServiceProvider.GetRequiredService<IBidStatusService>();
                var bidService = scope.ServiceProvider.GetRequiredService<IBidOfContractorService>();
                try
                {
                    var expiredBidsForClients = await dbContext.BidOfContractors
                        .Where(c => (c.ExpireAt.HasValue && c.ExpireAt <= DateTime.Now) &&
                            (c.BidStatuses != null && c.BidStatuses
                            .Any(x => x.Status != BidStatusEnum.BidApprovedByClient)))
                            .ToArrayAsync(stoppingToken);
                    var expiredBidsForContractors = await dbContext.BidOfContractors
                        .Where(b => (b.ExpireAt.HasValue && b.ExpireAt <= DateTime.Now) &&
                        (b.BidStatuses != null && b.BidStatuses
                        .Any(x => x.Status != BidStatusEnum.BidApprovedByContractor)))
                        .ToArrayAsync(stoppingToken);
                    var expiredBids = await dbContext.BidOfContractors
                        .Where(b => (b.ExpireAt.HasValue && b.ExpireAt <= DateTime.Now))
                        .ToListAsync(stoppingToken);
                    foreach (var bid in expiredBidsForContractors)
                    {
                        bid.ExpireAt = DateTime.Now.AddMinutes(7);
                        var expired = await bidStatusService
                            .AddAsync(new AddBidStatusDto
                            {
                                BidOfContractorId = bid.Id,
                                Status = BidStatusEnum.TimeForCheckingBidForContractorExpired,
                                CreatedBy = 100
                            }, stoppingToken);
                        if (expired.IsSuccessful)
                        {
                            dbContext.BidOfContractors.Update(bid);
                        }
                    }
                    foreach (var bid in expiredBids)
                    {
                        bid.ExpireAt = null;
                        bid.CanChangeBid = false;
                        var expired = await bidStatusService
                            .AddAsync(new AddBidStatusDto
                            {
                                BidOfContractorId = bid.Id,
                                Status = BidStatusEnum.TimeForCheckingBidForContractorExpired,
                                CreatedBy = 100
                            }, stoppingToken);
                        if (expired.IsSuccessful)
                        {
                            dbContext.BidOfContractors.Update(bid);
                        }
                    }

                    foreach (var bid in expiredBidsForClients)
                    {

                        bid.ExpireAt = null;
                        var expired = await bidStatusService
    
                        .AddAsync(new AddBidStatusDto
                        {
                            BidOfContractorId = bid.Id,
                            Status = BidStatusEnum.TimeForCheckingBidForClientExpired,
                            CreatedBy = 100
                        }, stoppingToken);
                        if (expired.IsSuccessful)
                        {
                            dbContext.BidOfContractors.Update(bid);
                        }
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}

