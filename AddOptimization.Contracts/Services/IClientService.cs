using AddOptimization.Contracts.Dto;
using AddOptimization.Utilities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddOptimization.Contracts.Services
{
    public interface IClientService
    {
        Task<ApiResult<ClientResponseDto>> Create(ClientRequestDto model);
        Task<ApiResult<List<ClientResponseDto>>> Search();

        Task<ApiResult<ClientResponseDto>> Get(Guid id);

        Task<ApiResult<List<ClientResponseDto>>> GetAllClients();
        Task<ApiResult<bool>> Delete(Guid id);
    }
}
