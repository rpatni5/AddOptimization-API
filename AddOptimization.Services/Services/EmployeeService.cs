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
    public EmployeeService(IGenericRepository<Employee> employeeRepository, ILogger<EmployeeService> logger, IMapper mapper,
        IAddressService addressService, IUnitOfWork unitOfWork, IEmailService emailService, ITemplateService templateService,
        IGenericRepository<PasswordResetToken> passwordResetTokenRepository,
        IGenericRepository<UserRole> userRoleRepository, IRoleService roleService,
        IGenericRepository<Role> roleRepository, IApplicationUserService applicationUserService,
    IGenericRepository<ApplicationUser> applicationUserRepository, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
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
                BankName= model.BankName,
                BankAccountName= model.BankAccountName,
                BankAccountNumber= model.BankAccountNumber,
                IsExternal= model.IsExternal,
                BillingAddress= model.BillingAddress,
                UserId = savedEmployee.Id,
            };

            await _employeeRepository.InsertAsync(entity);
           
            var roles = (await _roleService.Search(null)).Result;

            await AddRole(roles.FirstOrDefault(x => x.Name.Equals("Employee", StringComparison.InvariantCultureIgnoreCase)).Id, savedEmployee.Id);

            if (model.IsExternal)
            {
                await AddRole(roles.FirstOrDefault(x => x.Name.Equals("External Employee", StringComparison.InvariantCultureIgnoreCase)).Id, savedEmployee.Id);
            }

            await _unitOfWork.CommitTransactionAsync();
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

            Employee entity = new Employee();
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

    public async Task<ApiResult<List<EmployeeDto>>> Search(PageQueryFiterBase filters)
    {
        try
        {

            var entities = await _employeeRepository.QueryAsync(include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser).Include(e => e.ApplicationUser), orderBy: x => x.OrderByDescending(x => x.CreatedAt));

            var mappedEntities = _mapper.Map<List<EmployeeDto>>(entities);

            return ApiResult<List<EmployeeDto>>.Success(mappedEntities);
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

}