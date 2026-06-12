using Finexa.Application.Common.Models;
using Finexa.Application.Modules.Admin.DTOs;

namespace Finexa.Application.Modules.Admin.Interfaces
{
    public interface IAdminAuditLogService
    {
        Task AddAsync(CreateAdminAuditLogDto dto);

        Task<PagedResult<AdminAuditLogDto>> GetLogsAsync(AdminAuditLogFilterDto filter);
    }
}