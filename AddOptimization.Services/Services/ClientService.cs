using AddOptimization.Contracts.Dto;
using AddOptimization.Contracts.Services;
using AddOptimization.Data.Contracts;
using AddOptimization.Data.Entities;
using AddOptimization.Data.Repositories;
using AddOptimization.Utilities.Common;
using AddOptimization.Utilities.Constants;
using AddOptimization.Utilities.Extensions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace AddOptimization.Services.Services
{
    public class ClientService : IClientService
    {
        private readonly IGenericRepository<Client> _clientRepository;
        private readonly ILogger<ClientService> _logger;
        private readonly IMapper _mapper;


        public ClientService(IGenericRepository<Client> clientRepository, ILogger<ClientService> logger, IMapper mapper)
        {
            _clientRepository = clientRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResult<ClientResponseDto>> Create(ClientRequestDto model)
        {
            try
            {
                var isExists = await _clientRepository.IsExist(t => t.Email.ToLower() == model.ClientEmail.ToLower(), ignoreGlobalFilter: true);
                if (isExists)
                {
                    var errorMessage = isExists? "User already exists with some other role in the system." : "Client already exists with same email.";
                    return ApiResult<ClientResponseDto>.Failure(ValidationCodes.EmailUserNameAlreadyExists, errorMessage);
                }

                Client entity = new Client();
                entity.FirstName = model.FirstName;
                entity.LastName = model.LastName;
                entity.Organization = model.Company;
                entity.ManagerName = model.ManagerName;
                entity.Email = model.ClientEmail;
                entity.IsActive = true;
                entity.IsDeleted = false;
                entity.CountryId =model.CountryId;
                entity.IsApprovalRequired = model.IsApprovalRequired;
                await _clientRepository.InsertAsync(entity);

                var mappedEntity = _mapper.Map<ClientResponseDto>(entity);

                return ApiResult<ClientResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<List<ClientResponseDto>>> Search()
        {
            try
            {
                var entities = await _clientRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser));
                var mappedEntities = _mapper.Map<List<ClientResponseDto>>(entities.ToList());
                return ApiResult<List<ClientResponseDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<ClientResponseDto>> Get(Guid id)
        {
            try
            {
                var entity = await _clientRepository.FirstOrDefaultAsync(t => t.Id == id, ignoreGlobalFilter: true);
                if (entity == null)
                {
                    return ApiResult<ClientResponseDto>.NotFound("Client");
                }
                var mappedEntity = _mapper.Map<ClientResponseDto>(entity);
               
                return ApiResult<ClientResponseDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<List<ClientResponseDto>>> GetAllClients()
        {
            try
            {
                var entities = await _clientRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.UpdatedByUser), orderBy: x => x.OrderBy(x => x.Id));
                var mappedEntities = _mapper.Map<List<ClientResponseDto>>(entities);
                return ApiResult<List<ClientResponseDto>>.Success(mappedEntities);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<bool>> Delete(Guid id)
        {
            try
            {
                var entity = await _clientRepository.FirstOrDefaultAsync(t => t.Id == id);
                if (entity == null)
                {
                    return ApiResult<bool>.NotFound("Client");
                }

                await _clientRepository.DeleteAsync(entity);
                return ApiResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }


    }
}

