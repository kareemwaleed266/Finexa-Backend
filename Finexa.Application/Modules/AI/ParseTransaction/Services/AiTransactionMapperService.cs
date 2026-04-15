using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.AI.ParseTransaction.DTOs;
using Finexa.Application.Modules.AI.ParseTransaction.Interfaces;
using Finexa.Application.Modules.Transactions.DTOs;
using Finexa.Domain.Enums;

namespace Finexa.Application.Modules.AI.ParseTransaction.Services
{
    public class AiTransactionMapperService : IAiTransactionMapperService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public AiTransactionMapperService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<CreateTransactionDto> MapAsync(ParsedTransactionItemDto parsed)
        {
            var userId = _currentUser.UserId;

            // 🟡 1. Resolve Type
            var type = parsed.Type?.ToLower() == "income"
                ? TransactionType.Income
                : TransactionType.Expense;

            // 🟡 2. Get Categories
            var categoryRepo = _unitOfWork.Repository<Category, Guid>();

            var categories = await categoryRepo
                .WhereAsync(c => c.AppUserId == null || c.AppUserId == userId);

            // 🟡 3. Resolve Category
            var categoryName = parsed.CategoryName?.ToLower();

            var category = categories.FirstOrDefault(c =>
                c.Name.ToLower() == categoryName
            );

            // 🟡 fallback
            if (category == null)
            {
                category = categories.FirstOrDefault(c =>
                    c.Name.ToLower().Contains( "other")
                );

                if (category == null)
                    throw new Exception("Default category 'Other' not found");
            }

            // 🟡 4. Build DTO
            return new CreateTransactionDto
            {
                Amount = parsed.Amount,
                Type = type,
                CategoryId = category.Id,
                Notes = parsed.CategoryName ?? "AI Transaction",
                OccurredAt = parsed.OccurredAt ?? DateTime.UtcNow
                
            };
        }
    }
}
