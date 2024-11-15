﻿using Microsoft.Extensions.Logging;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Contracts.Services;
using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Extensions;
using AddOptimization.Contracts.Constants;
using AddOptimization.Utilities.Models;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using AddOptimization.Utilities.Interface;
using Microsoft.Extensions.Configuration;
using System;
using AddOptimization.Utilities.Constants;
using iText.StyledXmlParser.Jsoup.Nodes;
using AddOptimization.Utilities.Helpers;
using AddOptimization.Utilities.Enums;
using System.Linq;

namespace AddOptimization.Services.Services;
public class EmployeeService : IEmployeeService
{
    private readonly IGenericRepository<ApplicationUser> _applicationUserRepository;
    private readonly ILogger<EmployeeService> _logger;
    private readonly IMapper _mapper;
    private readonly List<string> _currentUserRoles;
    private readonly IAddressService _addressService;
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly IConfiguration _configuration;
    private readonly IGenericRepository<PasswordResetToken> _passwordResetTokenRepository;
    private readonly IGenericRepository<Role> _roleRepository;
    private readonly IGenericRepository<UserRole> _userRoleRepository;
    private readonly IGenericRepository<Employee> _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IRoleService _roleService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IGenericRepository<Country> _countryRepository;
    private readonly IGenericRepository<EmployeeContract> _employeeContract;
    public EmployeeService(IGenericRepository<Employee> employeeRepository, ILogger<EmployeeService> logger, IMapper mapper,
        IAddressService addressService, IUnitOfWork unitOfWork, IEmailService emailService, ITemplateService templateService,
        IGenericRepository<PasswordResetToken> passwordResetTokenRepository,
        IGenericRepository<UserRole> userRoleRepository, IRoleService roleService,
        IGenericRepository<Role> roleRepository, IApplicationUserService applicationUserService,
    IGenericRepository<ApplicationUser> applicationUserRepository, IConfiguration configuration, IGenericRepository<EmployeeContract> employeeContract, IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration;
        _applicationUserRepository = applicationUserRepository;
        _logger = logger;
        _mapper = mapper;
        _addressService = addressService;
        _unitOfWork = unitOfWork;
        _currentUserRoles = httpContextAccessor.HttpContext.GetCurrentUserRoles();
        _emailService = emailService;
        _templateService = templateService;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
        _employeeRepository = employeeRepository;
        _applicationUserRepository = applicationUserRepository;
        _applicationUserService = applicationUserService;
        _roleService = roleService;
        _httpContextAccessor = httpContextAccessor;
        _employeeContract = employeeContract;
    }

    public async Task<ApiResult<bool>> Save(EmployeeDto model)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();
            UserCreateDto user = new UserCreateDto
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                Password = model.Password,
                UserName = model.UserName,
            };
            var userResult = await _applicationUserService.Create(user);
            if (!userResult.Result)
            {
                return userResult;
            }
            var savedEmployee = await _applicationUserRepository.FirstOrDefaultAsync(x => x.Email == model.Email);
            Employee entity = new Employee
            {
                Salary = model.Salary,
                BankName = model.BankName,
                BankAccountName = model.BankAccountName,
                BankAccountNumber = model.BankAccountNumber,
                SwiftCode = model.SwiftCode,
                BankAddress = model.BankAddress,
                BankState = model.BankState,
                BankCity = model.BankCity,
                BankCountry = model.BankCountry,
                BankPostalCode = model.BankPostalCode,
                IsExternal = model.IsExternal,
                BillingAddress = model.BillingAddress,
                UserId = savedEmployee.Id,
                VATNumber = model.VATNumber,
                ZipCode = model.ZipCode,
                State = model.State,
                JobTitle = model.JobTitle,
                City = model.City,
                CompanyName = model.CompanyName,
                CountryId = model.CountryId,
                Address = model.Address,
                ExternalZipCode = model.ExternalZipCode,
                ExternalCity = model.ExternalCity,
                ExternalState = model.ExternalState,
                ExternalCountryId = model.ExternalCountryId,
                ExternalAddress = model.ExternalAddress,
                IdentityNumber = model.IdentityNumber,
                IdentityId = model.IdentityId,
            };

            await _employeeRepository.InsertAsync(entity);

            var roles = (await _roleService.Search(null)).Result;

            await AddRole(roles.FirstOrDefault(x => x.Name.Equals("Employee", StringComparison.InvariantCultureIgnoreCase)).Id, savedEmployee.Id);

            if (model.IsExternal)
            {
                await AddRole(roles.FirstOrDefault(x => x.Name.Equals("External Employee", StringComparison.InvariantCultureIgnoreCase)).Id, savedEmployee.Id);
            }

            await _unitOfWork.CommitTransactionAsync();
            await SendEmployeeCreatedEmail(savedEmployee.FullName, savedEmployee.Email);
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogException(ex);
            throw;
        }
    }

    private async Task AddRole(Guid roleId, int employeeId)
    {
        UserRole role = new UserRole()
        {
            RoleId = roleId,
            UserId = employeeId,
        };
        await _userRoleRepository.InsertAsync(role);
    }

    public async Task<ApiResult<EmployeeDto>> Update(Guid id, EmployeeDto model)
    {
        try
        {
            // await _unitOfWork.BeginTransactionAsync();

            var roles = (await _roleService.Search(null)).Result;

            var userRoles = new List<Guid>();
            userRoles.Add(roles.FirstOrDefault(x => x.Name.Equals("Employee", StringComparison.InvariantCultureIgnoreCase)).Id);
            if (model.IsExternal)
            {
                userRoles.Add(roles.FirstOrDefault(x => x.Name.Equals("External Employee", StringComparison.InvariantCultureIgnoreCase)).Id);
            }
            var user = new UserUpdateDto()
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Roles = userRoles,
            };
            var userResult = await _applicationUserService.Update(model.UserId, user);

            Employee entity = new Employee { };
            _mapper.Map(model, entity);
            await _employeeRepository.UpdateAsync(entity);

            //await _unitOfWork.CommitTransactionAsync();
            return ApiResult<EmployeeDto>.Success(_mapper.Map<EmployeeDto>(entity));
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<bool>> SignNDA(bool isNDASigned)
    {
        try
        {
            var userId = _httpContextAccessor.HttpContext.GetCurrentUserId().Value;
            var entity = await _employeeRepository.FirstOrDefaultAsync(o => o.UserId == userId, disableTracking: false, ignoreGlobalFilter: true);
            if (entity == null)
            {
                return ApiResult<bool>.Failure("Employee");
            }
            entity.IsNDASigned = isNDASigned;
            entity.NdaSignDate = DateTime.Now;
            await _employeeRepository.UpdateAsync(entity);
            return ApiResult<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }


    public async Task<PagedApiResult<EmployeeDto>> Search(PageQueryFiterBase filter)
    {
        try
        {
            var entities = await _employeeRepository.QueryAsync(
                include: entities => entities
                    .Include(e => e.CreatedByUser)
                    .Include(e => e.UpdatedByUser)
                    .Include(e => e.Country)
                    .Include(e => e.ExternalCountry)
                    .Include(e => e.ApplicationUser),
                orderBy: entities => entities.OrderByDescending(x => x.CreatedAt)
            );

            entities = ApplySorting(entities, filter?.Sorted?.FirstOrDefault());

            entities = ApplyFilters(entities, filter);

            var employeeIds = entities
                .Select(e => (int?)e.UserId)
                .Where(id => id.HasValue)
                .ToList();

            var contracts = await _employeeContract.QueryAsync(
                x => employeeIds.Contains(x.EmployeeId.GetValueOrDefault())
            );

            var contractEmployeeIds = new HashSet<int>(contracts.Select(c => c.EmployeeId.GetValueOrDefault()));

            var mappedEntities = _mapper.Map<List<EmployeeDto>>(entities).Select(e =>
            {
                e.HasContract = contractEmployeeIds.Contains(e.UserId);
                return e;
            }).ToList();

            var pagedResult = PageHelper<Employee, EmployeeDto>.ApplyPaging(
                entities,
                filter,
                pagedEntities => pagedEntities.Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    UserId = e.UserId,
                    FirstName = e.ApplicationUser.FirstName,
                    LastName = e.ApplicationUser.LastName,
                    Email = e.ApplicationUser.Email,
                    UserName = e.ApplicationUser.FullName,
                    JobTitle = e.JobTitle,
                    Address = e.Address,
                    City = e.City,
                    Salary = e.Salary,
                    CountryId = e.CountryId,
                    CountryName = e.Country.CountryName,
                    IsExternal = e.IsExternal,
                    IsNDASigned = e.IsNDASigned,
                    isActive = e.ApplicationUser.IsActive,
                    CreatedAt = e.CreatedAt,
                    UpdatedAt = e.UpdatedAt,
                    CreatedBy = e.CreatedByUser.FullName,
                    UpdatedBy = e.UpdatedByUser.FullName,
                    NdaSignDate = e.NdaSignDate,
                    ZipCode = e.ZipCode,
                    IdentityId = e.IdentityId,
                    IdentityNumber = e.IdentityNumber,

                    HasContract = contractEmployeeIds.Contains(e.UserId)
                }).ToList()
            );

            return PagedApiResult<EmployeeDto>.Success(pagedResult);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<ApiResult<EmployeeDto>> GetEmployeeById(Guid id)
    {
        try
        {
            var entity = await _employeeRepository.FirstOrDefaultAsync(t => t.Id == id, include: entity => entity.Include(e => e.ApplicationUser), ignoreGlobalFilter: true);
            if (entity == null)
            {
                return ApiResult<EmployeeDto>.NotFound("Employee");
            }
            var mappedEntity = _mapper.Map<EmployeeDto>(entity);

            return ApiResult<EmployeeDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    private async Task<bool> SendEmployeeCreatedEmail(string employeeFullName, string email)
    {
        try
        {
            var subject = "Account created";
            var emailTemplate = _templateService.ReadTemplate(EmailTemplates.EmployeeAccountCreated);
            var loginLink = $"{_configuration.ReadSection<AppUrls>(AppSettingsSections.AppUrls).BaseUrl}/login";
            emailTemplate = emailTemplate.Replace("[UserFullName]", employeeFullName)
                                         .Replace("[LoginLink]", loginLink);
            return await _emailService.SendEmail(email, subject, emailTemplate);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            return false;
        }
    }

    public async Task<ApiResult<EmployeeDto>> GetEmployeeByUserId(int id)
    {
        try
        {
            var entity = await _employeeRepository.FirstOrDefaultAsync(t => t.UserId == id, include: entity => entity.Include(e => e.ApplicationUser).Include(e => e.Country).Include(e => e.EmployeeIdentity), ignoreGlobalFilter: true);
            var mappedEntity = _mapper.Map<EmployeeDto>(entity);

            return ApiResult<EmployeeDto>.Success(mappedEntity);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    public async Task<PagedApiResult<EmployeeDto>> SearchEmployeesNda(PageQueryFiterBase filters)
    {
        try
        {
            var entities = await _employeeRepository.QueryAsync((e => e.ApplicationUser.IsActive), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.ApplicationUser), orderBy: x => x.OrderByDescending(x => x.CreatedAt));
            entities = ApplySorting(entities, filters?.Sorted?.FirstOrDefault());
            entities = ApplyFilters(entities, filters);

            var pagedResult = PageHelper<Employee, EmployeeDto>.ApplyPaging(entities, filters, entities => entities.Select(e => new EmployeeDto
            {
                Id = e.Id,
                UserId = e.ApplicationUser.Id,
                NdaSignDate = e.NdaSignDate,
                IsNDASigned = e.IsNDASigned,
                FullName = e.ApplicationUser.FullName,
                CreatedAt = e.CreatedAt,
                IsExternal = e.IsExternal,

            }).ToList());


            var result = pagedResult;
            return PagedApiResult<EmployeeDto>.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            throw;
        }
    }

    private IQueryable<Employee> ApplyFilters(IQueryable<Employee> entities, PageQueryFiterBase filter)
    {

        filter.GetValue<string>("firstName", (v) =>
        {
            entities = entities.Where(e => e.ApplicationUser.FullName.ToLower().Contains(v.ToLower()));
        });
        filter.GetValue<string>("email", (v) =>
        {
            entities = entities.Where(e => e.ApplicationUser.Email.ToLower().Contains(v.ToLower()));
        });

        filter.GetValue<string>("fullName", (v) =>
        {
            entities = entities.Where(e => e.ApplicationUser.FullName != null && (e.ApplicationUser.FullName.ToLower().Contains(v.ToLower())));
        });

        filter.GetValue<DateTime>("createdAt", (v) =>
        {
            entities = entities.Where(e => e.CreatedAt != null && e.CreatedAt < v);
        }, OperatorType.lessthan, true);

        filter.GetValue<DateTime>("createdAt", (v) =>
        {
            entities = entities.Where(e => e.CreatedAt != null && e.CreatedAt > v);
        }, OperatorType.greaterthan, true);

        filter.GetValue<DateTime>("ndaSignDate", (v) =>
        {
            entities = entities.Where(e => e.NdaSignDate != null && e.NdaSignDate < v);
        }, OperatorType.lessthan, true);

        filter.GetValue<DateTime>("ndaSignDate", (v) =>
        {
            entities = entities.Where(e => e.NdaSignDate != null && e.NdaSignDate > v);
        }, OperatorType.greaterthan, true);


        return entities;
    }


    private IQueryable<Employee> ApplySorting(IQueryable<Employee> entities, SortModel sort)
    {
        try
        {
            if (sort?.Name == null)
            {
                entities = entities.OrderByDescending(o => o.CreatedAt);
                return entities;
            }
            var columnName = sort.Name.ToUpper();
            if (sort.Direction == SortDirection.ascending.ToString())
            {
                if (columnName.ToUpper() == nameof(EmployeeDto.FullName).ToUpper())
                {
                    entities = entities.OrderBy(o => o.ApplicationUser.FullName);
                }
                if (columnName.ToUpper() == nameof(EmployeeDto.FirstName).ToUpper())
                {
                    entities = entities.OrderBy(o => o.ApplicationUser.FullName);
                }
                if (columnName.ToUpper() == nameof(EmployeeDto.Email).ToUpper())
                {
                    entities = entities.OrderBy(o => o.ApplicationUser.Email);
                }

                if (columnName.ToUpper() == nameof(EmployeeDto.CreatedAt).ToUpper())
                {
                    entities = entities.OrderBy(o => o.CreatedAt);
                }
                if (columnName.ToUpper() == nameof(EmployeeDto.NdaSignDate).ToUpper())
                {
                    entities = entities.OrderBy(o => o.NdaSignDate);
                }


            }

            else
            {
                if (columnName.ToUpper() == nameof(EmployeeDto.FullName).ToUpper())
                {
                    entities = entities.OrderByDescending(o => o.ApplicationUser.FullName);
                }
                if (columnName.ToUpper() == nameof(EmployeeDto.FirstName).ToUpper())
                {
                    entities = entities.OrderByDescending(o => o.ApplicationUser.FullName);
                }
                if (columnName.ToUpper() == nameof(EmployeeDto.Email).ToUpper())
                {
                    entities = entities.OrderByDescending(o => o.ApplicationUser.Email);
                }

                if (columnName.ToUpper() == nameof(EmployeeDto.CreatedAt).ToUpper())
                {
                    entities = entities.OrderByDescending(o => o.CreatedAt);
                }

                if (columnName.ToUpper() == nameof(EmployeeDto.NdaSignDate).ToUpper())
                {
                    entities = entities.OrderByDescending(o => o.NdaSignDate);
                }

            }
            return entities;

        }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            return entities;
        }

    }

}
