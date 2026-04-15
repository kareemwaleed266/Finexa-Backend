using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.Goals.DTOs;
using Finexa.Application.Modules.Goals.Interfaces;
using Finexa.Application.Modules.Transactions.DTOs;
using Finexa.Application.Modules.Transactions.Interfaces;
using Finexa.Domain.Enums;

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
        public async Task<List<GoalDto>> GetGoalsAsync()
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException();

            var goalRepo = _unitOfWork.Repository<Goal, Guid>();

            var goals = await goalRepo
                .WhereAsync(g => g.AppUserId == userId);

            return goals.Select(g => MapGoalToDto(g)).ToList();
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
                GoalId = goal.Id
            };

            await _transactionService.AddTransactionAsync(transactionDto, TransactionSource.Manual);

            goal.AddContribution(dto.Amount);

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<List<GoalHistoryDto>> GetGoalHistoryAsync(Guid goalId)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException();

            var goalRepo = _unitOfWork.Repository<Goal, Guid>();
            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();

            var goal = await goalRepo.GetByIdAsync(goalId);

            if (goal == null || goal.AppUserId != userId)
                throw new Exception("Goal not found");

            var transactions = await transactionRepo
                .WhereAsync(t => t.GoalId == goalId);

            return transactions
                .OrderByDescending(t => t.OccurredAt)
                .Select(t => new GoalHistoryDto
                {
                    Amount = t.Amount,
                    Date = t.OccurredAt,
                    Notes = t.Notes
                })
                .ToList();
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

            // Get History
            var transactions = await transactionRepo
                .WhereAsync(t => t.GoalId == goalId);

            var history = transactions
                .OrderByDescending(t => t.OccurredAt)
                .Select(t => new GoalHistoryDto
                {
                    Amount = t.Amount,
                    Date = t.OccurredAt,
                    Notes = t.Notes
                })
                .ToList();

            // Calculations
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
                TargetDate = goal.TargetDate,
                History = history
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
                throw new InvalidOperationException("Goal already canceled.");
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

            var contributions = await transactionRepo
                .WhereAsync(t => t.GoalId == goalId);

            if (!contributions.Any())
                throw new Exception("No contributions to refund");

            var categoryId = await GetGoalCategoryId(userId);

            var totalRefund = contributions.Sum(t => t.Amount);

            foreach (var t in contributions)
            {
                var dto = new CreateTransactionDto
                {
                    Amount = t.Amount,
                    Type = TransactionType.Income,
                    CategoryId = categoryId,
                    Notes = $"Refund for goal: {goal.Title}"
                };

                await _transactionService.AddTransactionAsync(dto, TransactionSource.Manual);
            }

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
                TargetDate = goal.TargetDate
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
    }
}