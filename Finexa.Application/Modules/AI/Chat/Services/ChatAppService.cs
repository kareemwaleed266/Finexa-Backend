using Finexa.Application.Interfaces.Persistence;
using Finexa.Application.Modules.AI.Chat.DTOs;
using Finexa.Application.Modules.AI.Chat.Interfaces;
using Finexa.Domain.Entities.Ai.Chat;
using Finexa.Domain.Enums.Ai;

namespace Finexa.Application.Modules.AI.Chat.Services
{
    public class ChatAppService : IChatAppService
    {
        private const int MaxMessageLength = 1000;
        private const int SummaryTriggerCount = 10;
        private const int KeepLastMessages = 4;


        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;
        private readonly IChatService _chatService;

        public ChatAppService(
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUser,
            IChatService chatService)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
            _chatService = chatService;
        }

        public async Task<ChatSessionDto> CreateSessionAsync(string? title = null)
        {
            var userId = GetCurrentUserId();

            var sessionRepo = _unitOfWork.Repository<ChatSession, Guid>();

            var session = new ChatSession
            {
                AppUserId = userId,
                Title = string.IsNullOrWhiteSpace(title) ? null : title.Trim(),
                Summary = null,
                LastActivityAt = DateTime.UtcNow
            };

            await sessionRepo.AddAsync(session);
            await _unitOfWork.SaveChangesAsync();

            return new ChatSessionDto
            {
                SessionId = session.Id,
                Title = session.Title,
                LastMessage = null,
                LastActivityAt = session.LastActivityAt
            };
        }

        public async Task<IReadOnlyList<ChatSessionDto>> GetMySessionsAsync()
        {
            var userId = GetCurrentUserId();

            var sessionRepo = _unitOfWork.Repository<ChatSession, Guid>();

            var sessions = await sessionRepo.WhereAsync(x => x.AppUserId == userId);

            var result = sessions
                .OrderByDescending(x => x.LastActivityAt)
                .Select(s =>
                {
                    var lastMessage = s.Messages
                        .OrderByDescending(m => m.CreatedAt)
                        .FirstOrDefault();

                    return new ChatSessionDto
                    {
                        SessionId = s.Id,
                        Title = s.Title,
                        LastMessage = lastMessage?.Content,
                        LastActivityAt = s.LastActivityAt
                    };
                })
                .ToList();

            return result;
        }

        public async Task<ChatDetailsDto> GetSessionDetailsAsync(Guid sessionId)
        {
            var userId = GetCurrentUserId();

            ValidateSessionId(sessionId);

            var session = await GetOwnedSessionAsync(userId, sessionId);

            var messageRepo = _unitOfWork.Repository<ChatMessage, Guid>();

            var messages = await messageRepo.WhereAsync(x => x.SessionId == sessionId);

            var orderedMessages = messages
                .OrderBy(x => x.CreatedAt)
                .TakeLast(20)
                .Select(m => new ChatMessageDto
                {
                    Role = MapRole(m.Role),
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                })
                .ToList();

            return new ChatDetailsDto
            {
                SessionId = session.Id,
                Title = session.Title,
                Messages = orderedMessages
            };
        }

        public async Task<SendMessageResponseDto> SendMessageAsync(SendMessageDto dto)
        {
            var userId = GetCurrentUserId();

            ValidateSendMessage(dto);

            var session = await GetOwnedSessionAsync(userId, dto.SessionId);

            var messageRepo = _unitOfWork.Repository<ChatMessage, Guid>();

            var allMessages = await messageRepo.WhereAsync(x => x.SessionId == session.Id);

            var recentMessages = allMessages
                .OrderByDescending(x => x.CreatedAt)
                .Take(SummaryTriggerCount)
                .OrderBy(x => x.CreatedAt)
                .ToList();

            var history = recentMessages
                .Select(m => new ChatMessageDto
                {
                    Role = MapRole(m.Role),
                    Content = m.Content,
                    CreatedAt = m.CreatedAt
                })
                .ToList();

            bool generateSummary = recentMessages.Count >= SummaryTriggerCount;

            var request = new ChatRequestDto
            {
                Message = dto.Message.Trim(),
                Summary = session.Summary,
                History = history,
                GenerateSummary = generateSummary
            };

            var userMessage = new ChatMessage
            {
                SessionId = session.Id,
                Role = ChatRole.User,
                Content = dto.Message.Trim()
            };

            await messageRepo.AddAsync(userMessage);

            session.LastActivityAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            var aiResponse = await _chatService.SendMessageAsync(request);

            if (string.IsNullOrWhiteSpace(aiResponse.Reply))
                throw new Exception("AI returned empty response");

            var assistantMessage = new ChatMessage
            {
                SessionId = session.Id,
                Role = ChatRole.Assistant,
                Content = aiResponse.Reply.Trim()
            };

            await messageRepo.AddAsync(assistantMessage);

            if (aiResponse.SummaryUpdated && !string.IsNullOrWhiteSpace(aiResponse.Summary))
            {
                session.Summary = aiResponse.Summary.Trim();

                var updatedMessages = await messageRepo
                    .WhereAsync(x => x.SessionId == session.Id);

                var messagesToDelete = updatedMessages
                    .OrderByDescending(x => x.CreatedAt)
                    .Skip(KeepLastMessages)
                    .ToList();

                foreach (var msg in messagesToDelete)
                {
                    messageRepo.Delete(msg);
                }
            }

            session.LastActivityAt = DateTime.UtcNow;

            await _unitOfWork.SaveChangesAsync();

            return new SendMessageResponseDto
            {
                SessionId = session.Id,
                Reply = aiResponse.Reply
            };
        }


        private Guid GetCurrentUserId()
        {
            var userId = _currentUser.UserId;

            if (userId == Guid.Empty)
                throw new UnauthorizedAccessException("User not authenticated");

            return userId;
        }

        private static void ValidateSessionId(Guid sessionId)
        {
            if (sessionId == Guid.Empty)
                throw new Exception("Invalid session id");
        }

        private static void ValidateSendMessage(SendMessageDto dto)
        {
            if (dto == null)
                throw new Exception("Request is null");

            if (dto.SessionId == Guid.Empty)
                throw new Exception("Invalid session");

            if (string.IsNullOrWhiteSpace(dto.Message))
                throw new Exception("Message is required");

            if (dto.Message.Length > MaxMessageLength)
                throw new Exception("Message too long");
        }

        private async Task<ChatSession> GetOwnedSessionAsync(Guid userId, Guid sessionId)
        {
            var sessionRepo = _unitOfWork.Repository<ChatSession, Guid>();

            var session = await sessionRepo.FirstOrDefaultAsync(
                x => x.Id == sessionId && x.AppUserId == userId);

            if (session == null)
                throw new Exception("Session not found");

            return session;
        }

        private static string MapRole(ChatRole role)
        {
            return role switch
            {
                ChatRole.User => "user",
                ChatRole.Assistant => "assistant",
                ChatRole.System => "system",
                _ => "user"
            };
        }
    }
}