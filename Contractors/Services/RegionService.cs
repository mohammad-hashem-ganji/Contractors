﻿
using Contractors.DbContractorsAuctioneerEF;
using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Utilities.Constants;
using Microsoft.EntityFrameworkCore;

namespace Contractors.Services
{
    public class RegionService : IRegionService
    {
        private readonly ApplicationDbContext _context;

        public RegionService(ApplicationDbContext? context)
        {
            _context = context;
        }
        public async Task<Result<AddRegionDto>> AddAsync(AddRegionDto regionDto, CancellationToken cancellationToken)
        {
            var region = new Region
            {
                CreatedAt = DateTime.Now,
                ContractorSystemCode = regionDto.ContractorSystemCode,
                IsDeleted = false,
                Title = regionDto.Title,
            };
            await _context.Regions.AddAsync(region, cancellationToken);
            var trackeNum = await _context.SaveChangesAsync(cancellationToken);
            if (trackeNum >= 1)
            {
                return new Result<AddRegionDto>().WithValue(regionDto).Success(SuccessMessages.RegionAdded);
            }
            else
            {
                return new Result<AddRegionDto>().WithValue(null).Failure(ErrorMessages.ErrorWhileAddingRegion);
                
            }
        }
        public async Task<Result<RegionDto>> GetByIdAsync(int regionId, CancellationToken cancellationToken)
        {
            try
			{
                var region = await _context.Regions
              .Where(x => x.Id == regionId)
              .Include(x => x.Requests)
              .FirstOrDefaultAsync(cancellationToken);
                if (region is null)
                {
                    return new Result<RegionDto>().WithValue(null).Failure(ErrorMessages.RegionNotFound);
                }
                else
                {
                    var regionDto = new RegionDto
                    {
                        Id = region.Id,
                        Title = region.Title,
                        ContractorSystemCode = region.ContractorSystemCode,
                    };
                    return new Result<RegionDto>().WithValue(regionDto).Success(SuccessMessages.Regionfound);
                }
            }
			catch (Exception ex)
			{
                return new Result<RegionDto>().WithValue(null).Failure(ex.Message);
            }
        }
    }
}
