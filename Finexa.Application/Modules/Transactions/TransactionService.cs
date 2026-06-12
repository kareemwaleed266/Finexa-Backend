using Finexa.Application.Common.DTOs;
using Finexa.Application.Common.Files;
using Finexa.Application.Common.Helpers;
using Finexa.Application.Common.Models;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.AI.ParseTransaction.DTOs;
using Finexa.Application.Modules.Dashboard.DTOs;
using Finexa.Application.Modules.Transactions.DTOs;
using Finexa.Application.Modules.Transactions.Interfaces;
using Finexa.Domain.Entities.Financial;
using Finexa.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Finexa.Application.Modules.Transactions.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public TransactionService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Guid> AddTransactionAsync(
            CreateTransactionDto dto,
            TransactionSource source,
            Guid? GoalId = null)
        {
            var userId = _currentUser.UserId;
            var userEmail = _currentUser.Email;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            if (dto == null)
                throw new ArgumentException("Transaction data is required");

            if (dto.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");

            var category = await GetValidCategoryAsync(
                dto.CategoryId,
                userId,
                dto.Type);

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                Amount = dto.Amount,
                Type = dto.Type,
                CategoryId = category.Id,
                Notes = dto.Notes,
                OccurredAt = dto.OccurredAt.HasValue
                    ? DateTimeHelper.ConvertClientLocalToUtc(dto.OccurredAt.Value)
                    : DateTime.UtcNow,
                AppUserId = userId,
                Source = source,
                GoalId = GoalId,
                Merchant = dto.Merchant,
                Item = dto.Item,
                CreatedBy = userEmail
            };

            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();

            await transactionRepo.AddAsync(transaction);

            await ApplyTransactionEffectToBalanceAsync(
                userId,
                dto.Amount,
                dto.Type);

            await _unitOfWork.SaveChangesAsync();

            return transaction.Id;
        }

        public async Task AddTransactionAttachmentAsync(
            Guid transactionId,
            FileUploadResultDto file,
            AttachmentType type)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            if (transactionId == Guid.Empty)
                throw new ArgumentException("Invalid transaction");

            if (file == null || string.IsNullOrWhiteSpace(file.Url))
                throw new ArgumentException("Invalid attachment file");

            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();

            var transaction = await transactionRepo
                .FirstOrDefaultAsync(x => x.Id == transactionId);

            if (transaction == null)
                throw new KeyNotFoundException("Transaction not found");

            if (transaction.AppUserId != userId)
                throw new UnauthorizedAccessException("Unauthorized transaction");

            var attachmentRepo = _unitOfWork.Repository<TransactionAttachment, Guid>();

            var alreadyExists = await attachmentRepo.ExistsAsync(x =>
                x.TransactionId == transactionId &&
                x.Type == type);

            if (alreadyExists)
                throw new InvalidOperationException("Attachment already exists for this transaction");

            var attachment = new TransactionAttachment
            {
                Id = Guid.NewGuid(),
                TransactionId = transactionId,
                Url = file.Url,
                PublicId = file.PublicId,
                FileName = file.FileName,
                ContentType = file.ContentType,
                CreatedBy = _currentUser.Email,
                SizeInBytes = file.SizeInBytes,
                Type = type
            };

            await attachmentRepo.AddAsync(attachment);

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateTransactionAsync(Guid transactionId, UpdateTransactionDto dto)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            if (dto == null)
                throw new ArgumentException("Transaction data is required");

            if (dto.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");

            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();
            var balanceRepo = _unitOfWork.Repository<UserBalance, Guid>();

            var transaction = await transactionRepo
                .Query(withTracking: true)
                .FirstOrDefaultAsync(t => t.Id == transactionId);

            if (transaction == null || transaction.AppUserId != userId)
                throw new KeyNotFoundException("Transaction not found");

            if (transaction.GoalId != null)
                throw new InvalidOperationException("Goal transactions cannot be updated");

            if (transaction.Source == TransactionSource.Bill)
                throw new InvalidOperationException("Bill transactions cannot be updated");

            if (dto.Type != transaction.Type)
                throw new InvalidOperationException("Transaction type cannot be changed");

            var category = await GetValidCategoryAsync(
                dto.CategoryId,
                userId,
                dto.Type);

            var balance = await balanceRepo
                .FirstOrDefaultAsync(x => x.AppUserId == userId);

            if (balance == null)
                throw new KeyNotFoundException("Balance not found");

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
            transaction.CategoryId = category.Id;
            transaction.Notes = dto.Notes;
            transaction.OccurredAt = dto.OccurredAt.HasValue
                ? DateTimeHelper.ConvertClientLocalToUtc(dto.OccurredAt.Value)
                : transaction.OccurredAt;
            transaction.Merchant = dto.Merchant;
            transaction.Item = dto.Item;
            transaction.LastModifiedBy = _currentUser.Email;

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
                throw new KeyNotFoundException("Transaction not found");

            if (transaction.GoalId != null)
                throw new InvalidOperationException("Goal transactions cannot be deleted");

            if (transaction.Source == TransactionSource.Bill)
                throw new InvalidOperationException("Bill transactions cannot be deleted");

            var balance = await balanceRepo
                .FirstOrDefaultAsync(x => x.AppUserId == userId);

            if (balance == null)
                throw new KeyNotFoundException("Balance not found");

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

            if (dto == null)
                throw new ArgumentException("Balance adjustment data is required");

            var balanceRepo = _unitOfWork.Repository<UserBalance, Guid>();
            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();

            var balance = await balanceRepo
                .FirstOrDefaultAsync(x => x.AppUserId == userId);

            if (balance == null)
                throw new KeyNotFoundException("Balance not found");

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
                Item = "Adjust",
                Merchant = "Adjust",
                CategoryId = await GetAdjustmentCategoryId(userId),
                Notes = "Balance Adjustment"
            };

            await AddTransactionAsync(transactionDto, TransactionSource.Manual);
        }

        public async Task<CreateTransactionDto> ConfirmTransactionsAsync(
            ConfirmParsedTransactionDto dto)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            if (dto == null)
                throw new ArgumentNullException(nameof(dto), "Transaction is required");

            if (dto.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero");

            if (string.IsNullOrWhiteSpace(dto.Type))
                throw new ArgumentException("Transaction type is required");

            var normalizedType = dto.Type.Trim().ToLowerInvariant();

            if (normalizedType != "income" && normalizedType != "expense")
                throw new ArgumentException("Invalid transaction type");

            var type = normalizedType == "income"
                ? TransactionType.Income
                : TransactionType.Expense;

            var category = await GetValidCategoryAsync(
                dto.CategoryId,
                userId,
                type);

            return new CreateTransactionDto
            {
                Amount = dto.Amount,
                CategoryId = category.Id,
                Type = type,
                Notes = dto.Notes ?? "AI Transaction",
                OccurredAt = dto.OccurredAt ?? DateTime.UtcNow,
                Merchant = dto.Merchant,
                Item = dto.Item
            };
        }

        public async Task<TransactionDto> GetTransactionByIdAsync(Guid id)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();

            var transaction = await transactionRepo
                .FirstOrDefaultAsync(x => x.Id == id);

            if (transaction == null)
                throw new KeyNotFoundException("Transaction not found");

            if (transaction.AppUserId != userId)
                throw new UnauthorizedAccessException("Unauthorized access to this transaction");

            var categoryRepo = _unitOfWork.Repository<Category, Guid>();

            var category = await categoryRepo.GetByIdAsync(transaction.CategoryId);

            var attachmentRepo = _unitOfWork.Repository<TransactionAttachment, Guid>();

            var receiptAttachment = await attachmentRepo.FirstOrDefaultAsync(x =>
                x.TransactionId == transaction.Id &&
                x.Type == AttachmentType.Receipt);

            return new TransactionDto
            {
                TransactionId = transaction.Id,
                Amount = transaction.Amount,
                Type = transaction.Type,
                CategoryName = category?.Name ?? "Unknown",
                Notes = transaction.Notes,
                OccurredAt = DateTimeHelper.EnsureUtcKind(transaction.OccurredAt),
                Merchant = transaction.Merchant ?? "Unknown",
                Item = transaction.Item ?? "Unknown",
                Source = transaction.Source,
                HasReceipt = receiptAttachment != null,
                ReceiptImageUrl = receiptAttachment?.Url
            };
        }

        public async Task<PagedResult<TransactionDto>> GetTransactionsAsync(
            TransactionFilterDto filter)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            if (filter.PageNumber <= 0)
                filter.PageNumber = 1;

            if (filter.PageSize <= 0 || filter.PageSize > 50)
                filter.PageSize = 10;

            var transactionRepo = _unitOfWork.Repository<Transaction, Guid>();
            var categoryRepo = _unitOfWork.Repository<Category, Guid>();
            var attachmentRepo = _unitOfWork.Repository<TransactionAttachment, Guid>();

            var query = transactionRepo.Query()
                .Where(x => x.AppUserId == userId);

            var (from, to) = DateRangeHelper.GetRange(
                filter.Period,
                filter.FromDate,
                filter.ToDate);

            if (filter.Type.HasValue)
                query = query.Where(x => x.Type == filter.Type.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(x => x.CategoryId == filter.CategoryId.Value);

            query = query.Where(x => x.OccurredAt >= from && x.OccurredAt <= to);

            query = ApplySorting(query, filter);

            var totalCount = await query.CountAsync();

            var skip = (filter.PageNumber - 1) * filter.PageSize;

            var transactions = await query
                .Skip(skip)
                .Take(filter.PageSize)
                .ToListAsync();

            var categoryIds = transactions
                .Select(t => t.CategoryId)
                .Distinct()
                .ToList();

            var categories = (await categoryRepo
                    .WhereAsync(c => categoryIds.Contains(c.Id)))
                .ToDictionary(c => c.Id, c => c.Name);

            var transactionIds = transactions
                .Select(t => t.Id)
                .ToList();

            var receiptAttachments = await attachmentRepo.WhereAsync(x =>
                transactionIds.Contains(x.TransactionId) &&
                x.Type == AttachmentType.Receipt);

            var receiptByTransactionId = receiptAttachments
                .GroupBy(x => x.TransactionId)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().Url);

            var items = transactions.Select(t => new TransactionDto
            {
                TransactionId = t.Id,
                Amount = t.Amount,
                Type = t.Type,
                Notes = t.Notes,
                OccurredAt = DateTimeHelper.EnsureUtcKind(t.OccurredAt),
                CategoryName = categories.TryGetValue(t.CategoryId, out var name)
                    ? name
                    : "Unknown",
                Source = t.Source,
                Merchant = t.Merchant ?? "Unknown",
                Item = t.Item ?? "Unknown",
                HasReceipt = receiptByTransactionId.ContainsKey(t.Id),
                ReceiptImageUrl = receiptByTransactionId.TryGetValue(t.Id, out var receiptUrl)
                    ? receiptUrl
                    : null
            }).ToList();

            return new PagedResult<TransactionDto>
            {
                Items = items,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }

        public async Task<int> AddParsedTransactionsAsync(
            List<ParsedTransactionItemDto> transactions,
            TransactionSource source)
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            if (transactions == null || !transactions.Any())
                throw new ArgumentException("No transactions found");

            var categoryRepo = _unitOfWork.Repository<Category, Guid>();

            var categories = await categoryRepo
                .WhereAsync(c =>
                    c.IsActive &&
                    (c.AppUserId == null || c.AppUserId == userId));

            var createdCount = 0;

            foreach (var parsedTransaction in transactions)
            {
                if (parsedTransaction.Amount <= 0)
                    continue;

                if (string.IsNullOrWhiteSpace(parsedTransaction.CategoryName))
                    continue;

                if (string.IsNullOrWhiteSpace(parsedTransaction.Type))
                    continue;

                var normalizedType = parsedTransaction.Type.Trim().ToLowerInvariant();

                if (normalizedType != "income" && normalizedType != "expense")
                    continue;

                var type = normalizedType == "income"
                    ? TransactionType.Income
                    : TransactionType.Expense;

                var category = categories.FirstOrDefault(c =>
                    c.Type == type &&
                    string.Equals(
                        c.Name.Trim(),
                        parsedTransaction.CategoryName.Trim(),
                        StringComparison.OrdinalIgnoreCase));

                if (category == null)
                    continue;

                var createDto = new CreateTransactionDto
                {
                    Amount = parsedTransaction.Amount,
                    CategoryId = category.Id,
                    Type = type,
                    Notes = parsedTransaction.Notes ?? parsedTransaction.CategoryName,
                    OccurredAt = parsedTransaction.OccurredAt,
                    Merchant = parsedTransaction.Merchant,
                    Item = parsedTransaction.Item
                };

                await AddTransactionAsync(createDto, source, null);

                createdCount++;
            }

            return createdCount;
        }

        private async Task<Category> GetValidCategoryAsync(
            Guid categoryId,
            Guid userId,
            TransactionType transactionType)
        {
            var category = await _unitOfWork.Repository<Category, Guid>()
                .Query()
                .FirstOrDefaultAsync(c =>
                    c.Id == categoryId &&
                    c.IsActive &&
                    (c.AppUserId == null || c.AppUserId == userId));

            if (category == null)
                throw new ArgumentException("Invalid category");

            if (category.AppUserId != null && category.AppUserId != userId)
                throw new UnauthorizedAccessException("Unauthorized category");

            if (category.Type != transactionType)
                throw new InvalidOperationException("Category type does not match transaction type");

            return category;
        }

        private async Task<Guid> GetAdjustmentCategoryId(Guid userId)
        {
            var category = await _unitOfWork.Repository<Category, Guid>()
                .Query()
                .FirstOrDefaultAsync(c =>
                    c.IsActive &&
                    c.Name == "Balance Adjustment" &&
                    c.Type == TransactionType.Income &&
                    (c.AppUserId == null || c.AppUserId == userId));

            if (category == null)
                throw new KeyNotFoundException("Adjustment category not found");

            return category.Id;
        }

        private async Task ApplyTransactionEffectToBalanceAsync(
            Guid userId,
            decimal amount,
            TransactionType type)
        {
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
                    TotalBalance = 0,
                    CreatedBy = _currentUser.Email
                };

                await balanceRepo.AddAsync(balance);
            }

            if (type == TransactionType.Income)
            {
                balance.TotalIncome += amount;
                balance.TotalBalance += amount;
            }
            else
            {
                balance.TotalExpense += amount;
                balance.TotalBalance -= amount;
            }
        }

        private IQueryable<Transaction> ApplySorting(
            IQueryable<Transaction> query,
            TransactionFilterDto filter)
        {
            var desc = filter.SortDirection != SortDirection.Asc;

            return filter.SortBy switch
            {
                TransactionSortBy.Amount => desc
                    ? query.OrderByDescending(x => x.Amount)
                    : query.OrderBy(x => x.Amount),

                TransactionSortBy.Date or null => desc
                    ? query.OrderByDescending(x => x.OccurredAt)
                    : query.OrderBy(x => x.OccurredAt),

                _ => query.OrderByDescending(x => x.OccurredAt)
            };
        }
    }
}