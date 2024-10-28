
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProjectService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<Result<AddProjectDto>> AddAsync(AddProjectDto addProjectDto, CancellationToken cancellationToken)
        {
            if (addProjectDto == null)
            {
                return new Result<AddProjectDto>().WithValue(null).Failure(ErrorMessages.EntityIsNull);
            }
            try
            {
                var project = new Entites.Project
                {
                    ContractorBidId = addProjectDto.ContractorBidId,
                };
                await _context.Projects.AddAsync(project, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                return new Result<AddProjectDto>().WithValue(addProjectDto).Success(SuccessMessages.OperationSuccessful);
            }
            catch (Exception ex)
            {
                return new Result<AddProjectDto>().WithValue(null).Failure(ex.Message);
            }
        }
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
