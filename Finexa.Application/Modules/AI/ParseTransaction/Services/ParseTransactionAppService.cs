using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.AI.ParseTransaction.DTOs;
using Finexa.Application.Modules.AI.ParseTransaction.Interfaces;

namespace Finexa.Application.Modules.AI.ParseTransaction.Services
{
    public class ParseTransactionAppService : IParseTransactionAppService
    {
        private const int MaxTextLength = 1000;

        private readonly IParseTransactionService _parseTransactionService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public ParseTransactionAppService(IParseTransactionService parseTransactionService,IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _parseTransactionService = parseTransactionService;
            _unitOfWork = unitOfWork;
            _currentUser = currentUserService;
        }

        public async Task<ParseTransactionResponseDto> ParseAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new Exception("Text is required");

            var userId = _currentUser.UserId;

            var categoryRepo = _unitOfWork.Repository<Category, Guid>();

            var categories = await categoryRepo
                .WhereAsync(c => c.AppUserId == null || c.AppUserId == userId);

            var categoryNames = categories
                .Select(c => c.Name)
                .ToList();

            var request = new ParseTransactionRequestDto
            {
                Text = text.Trim(),
                AvailableCategories = categoryNames
            };

            var result = await _parseTransactionService.ParseAsync(request);

            return result;
        }
    }
}