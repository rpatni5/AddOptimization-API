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
    public class ClientEmployeeAssociationService : IClientEmployeeAssociationService
    {
        private readonly IGenericRepository<ClientEmployeeAssociation> _clientEmployeeAssociationRepository;
        private readonly ILogger<ClientEmployeeAssociationService> _logger;
        private readonly IMapper _mapper;


        public ClientEmployeeAssociationService(IGenericRepository<ClientEmployeeAssociation> clientEmployeeAssociationRepository, ILogger<ClientEmployeeAssociationService> logger, IMapper mapper)
        {
            _clientEmployeeAssociationRepository = clientEmployeeAssociationRepository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task<ApiResult<ClientEmployeeAssociationDto>> Create(ClientEmployeeAssociationDto model)
        {
            try
            {

                var isExists = await _clientEmployeeAssociationRepository.IsExist(t => t.ClientId == model.ClientId && t.EmployeeId == model.EmployeeId && !t.IsDeleted, ignoreGlobalFilter: true);
                if (isExists)
                {
                    return ApiResult<ClientEmployeeAssociationDto>.Failure(ValidationCodes.ClientEmployeeAssociationAlreadyExists, ValidationErrorMessage.ClientEmployeeAssociationExist);
                }

                ClientEmployeeAssociation entity = new ClientEmployeeAssociation();
                _mapper.Map(model, entity);
                await _clientEmployeeAssociationRepository.InsertAsync(entity);
                var mappedEntity = _mapper.Map<ClientEmployeeAssociationDto>(entity);
                return ApiResult<ClientEmployeeAssociationDto>.Success(mappedEntity);
            }
            catch (Exception ex)
            {
                _logger.LogException(ex);
                throw;
            }
        }

        public async Task<ApiResult<List<ClientEmployeeAssociationDto>>> Search()
        {
            try
            {
                var entities = await _clientEmployeeAssociationRepository.QueryAsync((e => !e.IsDeleted), include: entities => entities.Include(e => e.CreatedByUser).Include(e => e.Approver).Include(e => e.Client).Include(e => e.ApplicationUser));

                var mappedEntities = _mapper.Map<List<ClientEmployeeAssociationDto>>(entities.ToList());
                return ApiResult<List<ClientEmployeeAssociationDto>>.Success(mappedEntities);
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
                var entity = await _clientEmployeeAssociationRepository.FirstOrDefaultAsync(t => t.Id == id);
                if (entity == null)
                {
                    return ApiResult<bool>.NotFound("Association");
                }
                entity.IsDeleted = true;
                await _clientEmployeeAssociationRepository.UpdateAsync(entity);
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

