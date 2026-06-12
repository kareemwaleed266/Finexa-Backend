using Finexa.Application.Common.Models;
using Finexa.Application.Modules.Admin.DTOs;

namespace Finexa.Application.Modules.Admin.Interfaces
{
    public interface IAdminUserService
    {
        Task<PagedResult<AdminUserListItemDto>> GetUsersAsync(AdminUserFilterDto filter);

        Task<AdminUserDetailsDto> GetUserDetailsAsync(Guid userId);

        Task LockUserAsync(Guid userId, AdminUserActionDto dto);

        Task UnlockUserAsync(Guid userId, AdminUserActionDto dto);

        Task ActivateUserAsync(Guid userId, AdminUserActionDto dto);

        Task DeactivateUserAsync(Guid userId, AdminUserActionDto dto);

        Task MakeAdminAsync(Guid userId, AdminUserActionDto dto);

        Task RemoveAdminAsync(Guid userId, AdminUserActionDto dto);
    }
}