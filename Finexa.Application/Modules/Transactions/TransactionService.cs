using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.AI.ParseTransaction.DTOs;
using Finexa.Application.Modules.Dashboard.DTOs;
using Finexa.Application.Modules.Transactions.DTOs;
using Finexa.Application.Modules.Transactions.Interfaces;
using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.Transactions.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public TransactionService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task AddTransactionAsync(CreateTransactionDto dto, TransactionSource source)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            var categoryRepo = _unitOfWork.Repository<Category, Guid>();

            var category = await categoryRepo
                .FirstOrDefaultAsync(c => c.Id == dto.CategoryId);

            if (category == null)
                throw new Exception("Invalid category");

            if (category.AppUserId != null && category.AppUserId != userId)
                throw new Exception("Unauthorized category");

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = dto.Amount,
                Type = dto.Type,
                CategoryId = dto.CategoryId,
                Notes = dto.Notes,
                OccurredAt = dto.OccurredAt ?? DateTime.UtcNow,
                AppUserId = userId,
                Source = source,
                GoalId = dto.GoalId 
            };

            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();
            await transactionRepo.AddAsync(transaction);

            var balanceRepo = _unitOfWork.Repository<UserBalance, Guid>();

            var balance = await balanceRepo
                .FirstOrDefaultAsync(x => x.AppUserId == userId);

            if (balance == null)
            {
                balance = new UserBalance
                {
                    Id = Guid.NewGuid(),
                    AppUserId = userId,
                    TotalIncome = 0,
                    TotalExpense = 0,
                    TotalBalance = 0
                };

                await balanceRepo.AddAsync(balance);
            }

            if (dto.Type == TransactionType.Income)
            {
                balance.TotalIncome += dto.Amount;
                balance.TotalBalance += dto.Amount;
            }
            else
            {
                balance.TotalExpense += dto.Amount;
                balance.TotalBalance -= dto.Amount;
            }

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<List<TransactionDto>> GetMyTransactionsAsync()
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();
            var categoryRepo = _unitOfWork.Repository<Category, Guid>();

            var transactions = (await transactionRepo
                .WhereAsync(x => x.AppUserId == userId))
                .OrderByDescending(x => x.OccurredAt)
                .ToList();

            var categoryIds = transactions
                .Select(t => t.CategoryId)
                .Distinct()
                .ToList();

            var categories = (await categoryRepo
                .WhereAsync(c => categoryIds.Contains(c.Id)))
                .ToDictionary(c => c.Id, c => c.Name);

            return transactions.Select(t => new TransactionDto
            {
                TransactionId = t.Id,
                Amount = t.Amount,
                Type = t.Type,
                Notes = t.Notes,
                OccurredAt = t.OccurredAt,
                CategoryName = categories.TryGetValue(t.CategoryId, out var name)
                    ? name
                    : "Unknown",
                Source = t.Source
            }).ToList();
        }
        public async Task UpdateTransactionAsync(Guid transactionId, UpdateTransactionDto dto)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();
            var balanceRepo = _unitOfWork.Repository<UserBalance, Guid>();
            var categoryRepo = _unitOfWork.Repository<Category, Guid>();

            var transaction = await transactionRepo.GetByIdAsync(transactionId);

            if (transaction == null || transaction.AppUserId != userId)
                throw new Exception("Transaction not found");

            if (transaction.GoalId != null)
                throw new Exception("Goal transactions cannot be updated");

            var category = await categoryRepo.FirstOrDefaultAsync(c => c.Id == dto.CategoryId);

            if (category == null)
                throw new Exception("Invalid category");

            if (category.AppUserId != null && category.AppUserId != userId)
                throw new Exception("Unauthorized category");

            var balance = await balanceRepo.FirstOrDefaultAsync(x => x.AppUserId == userId);

            if (balance == null)
                throw new Exception("Balance not found");

            if (dto.Type != transaction.Type)
                throw new Exception("Transaction type cannot be changed");


            if (transaction.Type == TransactionType.Income)
            {
                balance.TotalIncome -= transaction.Amount;
                balance.TotalBalance -= transaction.Amount;
            }
            else
            {
                balance.TotalExpense -= transaction.Amount;
                balance.TotalBalance += transaction.Amount;
            }

            transaction.Amount = dto.Amount;
            transaction.Type = dto.Type;
            transaction.CategoryId = dto.CategoryId;
            transaction.Notes = dto.Notes;
            transaction.OccurredAt = dto.OccurredAt ?? transaction.OccurredAt;


            if (dto.Type == TransactionType.Income)
            {
                balance.TotalIncome += dto.Amount;
                balance.TotalBalance += dto.Amount;
            }
            else
            {
                balance.TotalExpense += dto.Amount;
                balance.TotalBalance -= dto.Amount;
            }

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task DeleteTransactionAsync(Guid transactionId)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();
            var balanceRepo = _unitOfWork.Repository<UserBalance, Guid>();

            var transaction = await transactionRepo.GetByIdAsync(transactionId);

            if (transaction == null || transaction.AppUserId != userId)
                throw new Exception("Transaction not found");

            if (transaction.GoalId != null)
                throw new Exception("Goal transactions cannot be deleted");

            var balance = await balanceRepo
                .FirstOrDefaultAsync(x => x.AppUserId == userId);

            if (balance == null)
                throw new Exception("Balance not found");

            if (transaction.Type == TransactionType.Income)
            {
                balance.TotalIncome -= transaction.Amount;
                balance.TotalBalance -= transaction.Amount;
            }
            else
            {
                balance.TotalExpense -= transaction.Amount;
                balance.TotalBalance += transaction.Amount;
            }

            transactionRepo.Delete(transaction);

            await _unitOfWork.SaveChangesAsync();
        }
        public async Task AdjustBalanceAsync(AdjustBalanceDto dto)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            var balanceRepo = _unitOfWork.Repository<UserBalance, Guid>();
            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();

            var balance = await balanceRepo
                .FirstOrDefaultAsync(x => x.AppUserId == userId);

            if (balance == null)
                throw new Exception("Balance not found");

            decimal currentBalance;

            if (balance.TotalIncome != 0 || balance.TotalExpense != 0)
            {
                currentBalance = balance.TotalBalance;
            }
            else
            {
                var transactions = await transactionRepo
                    .WhereAsync(x => x.AppUserId == userId);

                var income = transactions
                    .Where(x => x.Type == TransactionType.Income)
                    .Sum(x => x.Amount);

                var expense = transactions
                    .Where(x => x.Type == TransactionType.Expense)
                    .Sum(x => x.Amount);

                currentBalance = income - expense;
            }

            var difference = dto.TargetBalance - currentBalance;

            if (difference == 0)
                return;

            var type = difference > 0
                ? TransactionType.Income
                : TransactionType.Expense;

            var amount = Math.Abs(difference);

            var transactionDto = new CreateTransactionDto
            {
                Amount = amount,
                Type = type,
                CategoryId = await GetAdjustmentCategoryId(userId),
                Notes = "Balance Adjustment"
            };

            await AddTransactionAsync(transactionDto, TransactionSource.Manual);
        }
        public async Task<List<CreateTransactionDto>> ConfirmTransactionsAsync(
                     List<ConfirmParsedTransactionDto> dtos)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            if (dtos == null || !dtos.Any())
                throw new Exception("No transactions to confirm");

            var categoryRepo = _unitOfWork.Repository<Category, Guid>();

            var categories = await categoryRepo
                .WhereAsync(c => c.AppUserId == null || c.AppUserId == userId);

            var result = new List<CreateTransactionDto>();

            foreach (var dto in dtos)
            {
                // 🔥 Validation
                if (dto.Amount <= 0)
                    throw new Exception("Invalid amount");

                var category = categories.FirstOrDefault(c => c.Id == dto.CategoryId);

                if (category == null)
                    throw new Exception("Invalid category");

                if (category.AppUserId != null && category.AppUserId != userId)
                    throw new Exception("Unauthorized category");

                var type = dto.Type.ToLower() == "income"
                    ? TransactionType.Income
                    : TransactionType.Expense;

                // 🔁 Mapping فقط
                var createDto = new CreateTransactionDto
                {
                    Amount = dto.Amount,
                    CategoryId = dto.CategoryId,
                    Type = type,
                    Notes = dto.Notes,
                    OccurredAt = dto.OccurredAt ?? DateTime.UtcNow
                };

                result.Add(createDto);
            }

            return result;
        }

        public async Task<TransactionDto> GetTransactionByIdAsync(Guid id)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            var repo = _unitOfWork.Repository<Transaction, Guid>();

            var transaction = await repo.FirstOrDefaultAsync(x => x.Id == id);

            if (transaction == null)
                throw new Exception("Transaction not found");

            if (transaction.AppUserId != userId)
                throw new Exception("Unauthorized access to this transaction");

            var categoryRepo = _unitOfWork.Repository<Category, Guid>();

            var category = await categoryRepo.GetByIdAsync(transaction.CategoryId);

            return new TransactionDto
            {
                TransactionId = transaction.Id,
                Amount = transaction.Amount,
                Type = transaction.Type,
                CategoryName = category.Name,
                Notes = transaction.Notes,
                OccurredAt = transaction.OccurredAt
            };
        }
        private async Task<Guid> GetAdjustmentCategoryId(Guid userId)
        {
            var categoryRepo = _unitOfWork.Repository<Category, Guid>();

            var category = await categoryRepo
                .FirstOrDefaultAsync(c =>
                    c.Name == "Balance Adjustment" &&
                    (c.AppUserId == null || c.AppUserId == userId));

            if (category == null)
                throw new Exception("Adjustment category not found");

            return category.Id;
        }

        public async Task<List<TransactionDto>> GetTransactionsAsync(TransactionFilterDto filter)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException();

            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();
            var categoryRepo = _unitOfWork.Repository<Category, Guid>();

            var query = (await transactionRepo
                .WhereAsync(x => x.AppUserId == userId))
                .AsQueryable();

            // 🔹 Filters

            if (filter.Type.HasValue)
                query = query.Where(x => x.Type == filter.Type.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(x => x.CategoryId == filter.CategoryId.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(x => x.OccurredAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(x => x.OccurredAt <= filter.ToDate.Value);

            // 🔹 Sorting
            query = query.OrderByDescending(x => x.OccurredAt);

            // 🔹 Pagination
            var skip = (filter.PageNumber - 1) * filter.PageSize;

            var transactions = query
                .Skip(skip)
                .Take(filter.PageSize)
                .ToList();

            var categoryIds = transactions.Select(t => t.CategoryId).Distinct().ToList();

            var categories = (await categoryRepo
                .WhereAsync(c => categoryIds.Contains(c.Id)))
                .ToDictionary(c => c.Id, c => c.Name);

            return transactions.Select(t => new TransactionDto
            {
                TransactionId = t.Id,
                Amount = t.Amount,
                Type = t.Type,
                Notes = t.Notes,
                OccurredAt = t.OccurredAt,
                CategoryName = categories.TryGetValue(t.CategoryId, out var name)
                    ? name
                    : "Unknown",
                Source = t.Source
            }).ToList();
        }
    }
}