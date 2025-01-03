﻿
using Azure.Core;
using Contractors.DbContractorsAuctioneerEF;
using Contractors.Dtos;
using Contractors.Entites;
using Contractors.Interfaces;
using Contractors.Results;
using Contractors.Utilities.Constants;
using ContractorsAuctioneer.Services;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

namespace Contractors.Services
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBidOfContractorService _bidOfContractorService;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, IBidOfContractorService bidOfContractorService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _bidOfContractorService = bidOfContractorService;
        }
        public async Task<Result<AddProjectDto>> AddAsync(AddProjectDto addProjectDto, CancellationToken cancellationToken)
        {
            if (addProjectDto == null)
            {
                return new Result<AddProjectDto>().WithValue(null).Failure(ErrorMessages.EntityIsNull);
            }

            try
            {

                var bid = await _context.BidOfContractors
                    .Include(b => b.Contractor)
                    .Include(b => b.Request)
                        .ThenInclude(r => r.Client)
                    .FirstOrDefaultAsync(b => b.Id == addProjectDto.ContractorBidId, cancellationToken);

                if (bid == null)
                {
                    return new Result<AddProjectDto>().WithValue(null).Failure("خطایی هنگام تعریف پروژه رخ داد.");
                }

                var contractorId = bid.ContractorId;
                var clientId = bid.Request?.ClientId;

               
                if (contractorId == 0 || clientId == null)
                {
                    return new Result<AddProjectDto>().WithValue(null).Failure("خطایی هنگام تعریف پروژه رخ داد.");
                }


                var project = new Entites.Project
                {
                    ContractorBidId = bid.Id,
                    ContractorId = contractorId,
                    ClientId = clientId.Value 
                };

                // Add and save the project
                await _context.Projects.AddAsync(project, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return new Result<AddProjectDto>().WithValue(addProjectDto).Success(SuccessMessages.OperationSuccessful);
            }
            catch (Exception ex)
            {
                // Log the exception as needed
                return new Result<AddProjectDto>().WithValue(null).Failure("خطایی هنگام تعریف پروژه رخ داد.");
            }
        }

        //public async Task<Result<AddProjectDto>> AddAsync(AddProjectDto addProjectDto, CancellationToken cancellationToken)
        //{
        //    // Validate the input DTO
        //    if (addProjectDto == null)
        //    {
        //        return new Result<AddProjectDto>().WithValue(null).Failure(ErrorMessages.EntityIsNull);
        //    }

        //    try
        //    {
        //        // Fetch the bid and associated entities
        //        var bid = await _context.BidOfContractors
        //            .Include(b => b.Contractor)
        //            .Include(b => b.Request)
        //                .ThenInclude(r => r.Client)
        //            .FirstOrDefaultAsync(b => b.Id == addProjectDto.ContractorBidId, cancellationToken);

        //        // Check if bid is found
        //        if (bid == null)
        //        {
        //            return new Result<AddProjectDto>().WithValue(null).Failure("Bid not found");
        //        }

        //        // Validate contractor and client
        //        var contractorId = bid.ContractorId;
        //        var clientId = bid.Request?.ClientId;

        //        if (contractorId == 0 || clientId == 0)
        //        {
        //            return new Result<AddProjectDto>().WithValue(null).Failure("Contractor or Client not found");
        //        }

        //        // Create the new project entity
        //        var project = new Entites.Project
        //        {
        //            ContractorBidId = bid.Id,
        //            ContractorId = contractorId,
        //            ClientId = clientId
        //        };

        //        // Add and save the project
        //        await _context.Projects.AddAsync(project, cancellationToken);
        //        await _context.SaveChangesAsync(cancellationToken);

        //        return new Result<AddProjectDto>().WithValue(addProjectDto).Success(SuccessMessages.OperationSuccessful);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception as needed
        //        return new Result<AddProjectDto>().WithValue(null).Failure(ex.Message);
        //    }
        //}

        //public async Task<Result<AddProjectDto>> AddAsync(AddProjectDto addProjectDto, CancellationToken cancellationToken)
        //{
        //    if (addProjectDto == null)
        //    {
        //        return new Result<AddProjectDto>().WithValue(null).Failure(ErrorMessages.EntityIsNull);
        //    }

        //    try
        //    {
        //        var bid = await _context.BidOfContractors
        //            .FirstOrDefaultAsync(b => b.Id == addProjectDto.ContractorBidId, cancellationToken);
        //        var contractor = await _context.Contractors
        //            .FirstOrDefaultAsync(c => c.Id == bid.ContractorId, cancellationToken);
        //        var request = await _context.Requests
        //            .FirstOrDefaultAsync(r => r.Id == bid.RequestId, cancellationToken);
        //        var client = await _context.Clients
        //            .FirstOrDefaultAsync(c => c.Id == request.ClientId, cancellationToken);

        //        if (contractor == null || client == null)
        //        {
        //            return new Result<AddProjectDto>().WithValue(null).Failure("Contractor or Client not found");
        //        }

        //        var project = new Entites.Project
        //        {
        //            ContractorBidId = bid.Id,
        //            ContractorId = contractor.ApplicationUserId,
        //            ClientId = client.ApplicationUserId
        //        };

        //        await _context.Projects.AddAsync(project, cancellationToken);
        //        await _context.SaveChangesAsync(cancellationToken);

        //        return new Result<AddProjectDto>().WithValue(addProjectDto).Success(SuccessMessages.OperationSuccessful);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new Result<AddProjectDto>().WithValue(null).Failure(ex.Message);
        //    }
        //}


        public async Task<Result<GetProjectDto>> GetByIdAsync(int projectId, CancellationToken cancellationToken)
        {
            try
            {
                var project = await _context.Projects
                    .Where(x => x.Id == projectId)
                    .Include(x => x.ContractorBid)
                    .Include(x => x.ProjectStatuses)
                    .FirstOrDefaultAsync(cancellationToken);
                if (project == null)
                {
                    return new Result<GetProjectDto>().WithValue(null).Failure(ErrorMessages.ProjectNotFound);
                }
                else
                {
                    var projectDto = new GetProjectDto
                    {
                        Id = project.Id,
                        ContractorBidId = project.ContractorBidId,
                        StartedAt = project.StartedAt,
                        CompletedAt = project.CompletedAt,
                        ProjectStatuses = project.ProjectStatuses.Select(p => new ProjectStatus
                        {
                            Id = p.Id,
                            Status = p.Status,
                            UpdatedAt = p.UpdatedAt,
                            UpdatedBy = p.UpdatedBy,
                        }).ToList(),

                    };
                    return new Result<GetProjectDto>().WithValue(projectDto).Success("پروژه پیدا شد");
                }
            }
            catch (Exception ex)
            {
                return new Result<GetProjectDto>().WithValue(null).Failure(ex.Message);
            }
        }



        public async Task<Result<GetProjectDto>> GetProjectOfbidAsync(int bidId, CancellationToken cancellationToken)
        {
            try
            {
                var user = await UserManagement.GetRoleBaseUserId(_httpContextAccessor.HttpContext, _context);

                if (!user.IsSuccessful)
                {
                    return new Result<GetProjectDto>().WithValue(null).Failure("خطا");
                }
                var contractorId = user.Data.UserId;
                var project = await _context.Projects
                    .Where(x => x.ContractorBidId == bidId
                    && x.ContractorBid.ContractorId == contractorId)
                    .FirstOrDefaultAsync(cancellationToken);
                if (project == null)
                {
                    return new Result<GetProjectDto>().WithValue(null).Failure(ErrorMessages.ProjectNotFound);
                }
                else
                {
                    var projectDto = new GetProjectDto
                    {
                        Id = project.Id,
                        ContractorBidId = project.ContractorBidId,
                        StartedAt = project.StartedAt,
                        CompletedAt = project.CompletedAt,
                      

                    };
                    return new Result<GetProjectDto>().WithValue(projectDto).Success("پروژه پیدا شد");
                }
            }
            catch (Exception ex)
            {
                return new Result<GetProjectDto>().WithValue(null).Failure(ex.Message);
            }

        }

        public async Task<Result<GetProjectDto>> UpdateAsync(GetProjectDto projectDto, CancellationToken cancellationToken)
        {
            try
            {
                var project = await _context.Projects
                    .Where(x => x.Id == projectDto.Id && x.IsDeleted == false)
                    .Include(x => x.ContractorBid)
                    .Include(x => x.ProjectStatuses)
                    .FirstOrDefaultAsync(cancellationToken);
                if (project is null)
                {
                    return new Result<GetProjectDto>().WithValue(null).Failure(ErrorMessages.ProjectNotFound);
                }
                else
                {
                    project.CompletedAt = projectDto.CompletedAt;
                    project.StartedAt = project.StartedAt;
                    project.IsDeleted = projectDto.IsDeleted;
                    project.DeletedBy = projectDto.DeletedBy;
                    project.DeletedAt = DateTime.Now;
                    project.UpdatedAt = DateTime.Now;
                    project.UpdatedBy = projectDto.UpdatedBy;
                    _context.Projects.Update(project);
                    await _context.SaveChangesAsync();
                    return new Result<GetProjectDto>().WithValue(projectDto).Success("پروژه آپدیت شد.");
                }

            }
            catch (Exception ex)
            {
                return new Result<GetProjectDto>().WithValue(null).Failure(ex.Message);
            }
        }



    }
}
