using Finexa.Application.Common.DTOs;
using Finexa.Application.Common.Helpers;
using Finexa.Application.Common.Models;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Goals.DTOs;
using Finexa.Application.Modules.Goals.Interfaces;
using Finexa.Application.Modules.Transactions.DTOs;
using Finexa.Application.Modules.Transactions.Interfaces;
using Finexa.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Application.Modules.Goals.Services
{
    public class GoalService : IGoalService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly ITransactionService _transactionService;

        public GoalService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ITransactionService transactionService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _transactionService = transactionService;
        }

        public async Task CreateGoalAsync(CreateGoalDto dto)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            ValidateCreateGoalInput(dto);

            var months = CalculateMonths(dto);

            var targetDate = DateTime.UtcNow.AddMonths(months);

            var goal = new Goal
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                TargetAmount = dto.TargetAmount,
                TargetDate = targetDate,
                AppUserId = userId
            };

            var goalRepo = _unitOfWork.Repository<Goal, Guid>();

            await goalRepo.AddAsync(goal);

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<PagedResult<GoalDto>> GetGoalsAsync(GoalFilterDto filter)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException();

            if (filter.PageNumber <= 0)
                filter.PageNumber = 1;

            if (filter.PageSize <= 0 || filter.PageSize > 50)
                filter.PageSize = 10;

            var goalRepo = _unitOfWork.Repository<Goal, Guid>();

            var query = goalRepo.Query()
                .Where(g => g.AppUserId == userId && !g.IsHidden);

            if (filter.Status.HasValue)
                query = query.Where(x => x.Status == filter.Status.Value);

            var totalCount = await query.CountAsync();

            query = ApplyGoalSorting(query, filter);

            
            var skip = (filter.PageNumber - 1) * filter.PageSize;

            var goals = await query
                .Skip(skip)
                .Take(filter.PageSize)
                .ToListAsync();

            var items = goals.Select(g => MapGoalToDto(g)).ToList();

            return new PagedResult<GoalDto>
            {
                Items = items,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }
        public async Task ContributeToGoalAsync(Guid goalId, GoalContributionDto dto)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            if (dto.Amount <= 0)
                throw new Exception("Amount must be greater than zero");

            var goalRepo = _unitOfWork.Repository<Goal, Guid>();

            var goal = await goalRepo.GetByIdAsync(goalId);

            if (goal == null || goal.AppUserId != userId)
                throw new Exception("Goal not found");

            if (goal.IsCompleted())
                throw new Exception("Goal already completed");

            if (goal.IsCanceled())
                throw new Exception("Goal already canceled");

            var transactionDto = new CreateTransactionDto
            {
                Amount = dto.Amount,
                Type = TransactionType.Expense,
                CategoryId = await GetGoalCategoryId(userId),
                Notes = $"Contribution to goal: {goal.Title}",
                OccurredAt = DateTime.UtcNow,
                Item = goal.Title
            };

            await _transactionService.AddTransactionAsync(transactionDto, TransactionSource.Manual,goal.Id);

            goal.AddContribution(dto.Amount);

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<PagedResult<GoalHistoryDto>> GetGoalHistoryAsync(Guid goalId, BaseFilterDto filter)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException();

            if (filter.PageNumber <= 0)
                filter.PageNumber = 1;

            var goalRepo = _unitOfWork.Repository<Goal, Guid>();
            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();

            var goal = await goalRepo.GetByIdAsync(goalId);

            if (goal == null || goal.AppUserId != userId)
                throw new Exception("Goal not found");

            var query = transactionRepo.Query()
                .Where(t => t.GoalId == goalId);

            var totalCount = await query.CountAsync();

            if (filter.PageSize <= 0 || filter.PageSize > 20)
                filter.PageSize = 2;


            var skip = (filter.PageNumber - 1) * filter.PageSize;

            var transactions = await query
                .OrderByDescending(t => t.OccurredAt)
                .Skip(skip)
                .Take(filter.PageSize)
                .ToListAsync();

            var items = transactions.Select(t => new GoalHistoryDto
            {
                Amount = t.Amount,
                Date = DateTimeHelper.EnsureUtcKind(t.OccurredAt),
                Notes = t.Notes,
                Status = goal.Status

            }).ToList();

            return new PagedResult<GoalHistoryDto>
            {
                Items = items,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }
        public async Task<GoalDetailsDto> GetGoalDetailsAsync(Guid goalId)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException();

            var goalRepo = _unitOfWork.Repository<Goal, Guid>();
            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();

            var goal = await goalRepo.GetByIdAsync(goalId);

            if (goal == null || goal.AppUserId != userId)
                throw new Exception("Goal not found");

            var transactions = await transactionRepo.Query()
            .Where(t => t.GoalId == goalId)
            .OrderByDescending(t => t.OccurredAt)
            .Take(5) 
            .ToListAsync();

            var current = goal.CurrentAmount;
            var remaining = goal.GetRemainingAmount();

            var progress = goal.TargetAmount == 0
                ? 0
                : (double)(current / goal.TargetAmount) * 100;

            var months = CalculateRemainingMonths(goal.TargetDate);

            var monthly = months == 0
                ? remaining
                : remaining / months;

            return new GoalDetailsDto
            {
                GoalId = goal.Id,
                Title = goal.Title,
                Description = goal.Description,
                TargetAmount = goal.TargetAmount,
                CurrentAmount = current,
                RemainingAmount = remaining,
                ProgressPercentage = Math.Round(progress, 2),
                MonthlyAmount = Math.Round(monthly, 2),
                Status = goal.Status,
                TargetDate = DateTimeHelper.EnsureUtcKind(goal.TargetDate)
            };
        }
        public async Task CancelGoalAsync(Guid goalId)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            var goalRepo = _unitOfWork.Repository<Goal, Guid>();

            var goal = await goalRepo.GetByIdAsync(goalId);

            if (goal == null || goal.AppUserId != userId)
                throw new Exception("Goal not found");

            if ( goal.Status == GoalStatus.Canceled)
                throw new InvalidOperationException("Goal already canceled");

            goal.CancelGoal();

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task RefundGoalAsync(Guid goalId)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException();

            var goalRepo = _unitOfWork.Repository<Goal, Guid>();
            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();

            var goal = await goalRepo.GetByIdAsync(goalId);

            if (goal == null || goal.AppUserId != userId)
                throw new Exception("Goal not found");

            if (goal.IsRefunded)
                throw new Exception("Goal already refunded");

            if (goal.Status != GoalStatus.Canceled)
                throw new Exception("Only canceled goals can be refunded");

            var contributions = await transactionRepo
                .WhereAsync(t => t.GoalId == goalId);

            if (!contributions.Any())
            {
                goal.ResetAfterRefund();
                await _unitOfWork.SaveChangesAsync();
                return;
            }                    

            var categoryId = await GetGoalCategoryId(userId);
            var refundCategory = await GetBalanceAdjustmentCategoryAsync(userId);

            var totalRefund = contributions.Sum(t => t.Amount);


            var dto = new CreateTransactionDto
            {
                Amount = totalRefund,
                Item = $"{goal.Title} Refund",
                Type = TransactionType.Income,
                CategoryId = refundCategory.Id,
                Notes = $"Refund for goal: {goal.Title}"
            };

            await _transactionService.AddTransactionAsync(dto, TransactionSource.Manual, null);

            goal.ResetAfterRefund();

            await _unitOfWork.SaveChangesAsync();
        }

        private void ValidateCreateGoalInput(CreateGoalDto dto)
        {
            var hasDuration = dto.DurationValue.HasValue && dto.DurationUnit.HasValue;
            var hasMonthly = dto.MonthlyAmount.HasValue;

            if (hasDuration == hasMonthly)
                throw new Exception("You must choose either duration or monthly amount");
        }


        private int CalculateMonths(CreateGoalDto dto)
        {
            if (dto.DurationValue.HasValue && dto.DurationUnit.HasValue)
            {
                return dto.DurationUnit == GoalDurationUnit.Years
                    ? dto.DurationValue.Value * 12
                    : dto.DurationValue.Value;
            }

            return (int)Math.Ceiling(dto.TargetAmount / dto.MonthlyAmount!.Value);
        }

        private GoalDto MapGoalToDto(Goal goal)
        {
            var currentAmount = goal.CurrentAmount;

            var remaining = goal.TargetAmount - currentAmount;

            var progress = goal.TargetAmount == 0
                ? 0
                : (double)(currentAmount / goal.TargetAmount) * 100;

            var months = CalculateRemainingMonths(goal.TargetDate);

            var monthly = months == 0
                ? remaining
                : remaining / months;

            return new GoalDto
            {
                GoalId = goal.Id,
                Title = goal.Title,
                Description = goal.Description,
                TargetAmount = goal.TargetAmount,
                CurrentAmount = currentAmount,
                RemainingAmount = remaining,
                ProgressPercentage = Math.Round(progress, 2),
                MonthlyAmount = Math.Round(monthly, 2),
                TargetDate = goal.TargetDate,
                Status = goal.Status
            };
        }

        private int CalculateRemainingMonths(DateTime targetDate)
        {
            var now = DateTime.UtcNow;

            var months = (targetDate.Year - now.Year) * 12 + targetDate.Month - now.Month;

            return months <= 0 ? 1 : months;
        }


        private async Task<Guid> GetGoalCategoryId(Guid userId)
        {
            var categoryRepo = _unitOfWork.Repository<Category, Guid>();

            var category = await categoryRepo.FirstOrDefaultAsync(c =>
                c.Name == "Goals" &&
                (c.AppUserId == null || c.AppUserId == userId));

            if (category == null)
                throw new Exception("Goal category not found");

            return category.Id;
        }

        private async Task<Category> GetBalanceAdjustmentCategoryAsync(Guid userId)
        {
            var category = await _unitOfWork.Repository<Category, Guid>()
                .Query()
                .FirstOrDefaultAsync(c =>
                    c.IsActive &&
                    c.Name == "Balance Adjustment" &&
                    c.Type == TransactionType.Income &&
                    (c.AppUserId == null || c.AppUserId == userId));

            if (category == null)
                throw new KeyNotFoundException("Balance Adjustment category not found");

            return category;
        }
        private IQueryable<Goal> ApplyGoalSorting(IQueryable<Goal> query, GoalFilterDto filter)
        {
            var desc = filter.SortDirection != SortDirection.Asc;

            return filter.SortBy switch
            {
                GoalSortBy.TargetAmount => desc
                    ? query.OrderByDescending(x => x.TargetAmount)
                    : query.OrderBy(x => x.TargetAmount),

                GoalSortBy.Progress => desc
                    ? query.OrderByDescending(x =>
                        x.TargetAmount == 0 ? 0 :
                        (x.CurrentAmount / x.TargetAmount) * 100)
                    : query.OrderBy(x =>
                        x.TargetAmount == 0 ? 0 :
                        (x.CurrentAmount / x.TargetAmount) * 100),

                GoalSortBy.Status =>
                     query.OrderBy(x =>
                         x.Status == GoalStatus.InProgress ? 0 :
                         x.Status == GoalStatus.Completed ? 1 :
                         x.Status == GoalStatus.Refunded ? 2 :
                         x.Status == GoalStatus.Canceled ? 3 : 4),

                _ => desc
                    ? query.OrderByDescending(x => x.CreatedAt)
                    : query.OrderBy(x => x.CreatedAt)
            };
        }
    }
}