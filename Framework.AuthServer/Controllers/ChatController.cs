using AutoMapper;
using Framework.Application;
using Framework.AuthServer.Dtos.ChatService.Input;
using Framework.AuthServer.Dtos.ChatService.Output;
using Framework.AuthServer.Hubs;
using Framework.AuthServer.Interfaces.Repositories;
using Framework.AuthServer.Models;
using Framework.Domain.Interfaces.Repositories;
using Framework.Shared.Dtos;
using Framework.Shared.Entities;
using Framework.Shared.Entities.Configurations;
using Framework.Shared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Framework.AuthServer.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class ChatController : BaseController
    {
        private readonly Configuration Configuration;
        private readonly ILogger<ChatController> Logger;
        private readonly IMapper Mapper;
        private readonly IHubContext<ChatHub> HubContext;
        private readonly IChatMessageRepository ChatMessageRepository;
        private readonly IGenericRepository<User, Guid> UserRepository;

        public ChatController(
            Configuration configuration,
            ILogger<ChatController> logger,
            IMapper mapper,
            IHubContext<ChatHub> hubContext,
            IChatMessageRepository chatMessageRepository,
            IGenericRepository<User, Guid> userRepository
            )
        {
            Configuration = configuration;
            Logger = logger;
            Mapper = mapper;
            HubContext = hubContext;
            ChatMessageRepository = chatMessageRepository;
            UserRepository = userRepository;
        }

        [HttpGet]
        [Authorize]
        public async Task<GeneralResponse<GetChatMessagesOutput>> GetChatMessagesAsync([FromQuery] int count, [FromQuery] Guid receiverId, [FromQuery] int? lastMessageId = 0)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var userId = GetUserIdGuid();
                count = Math.Min(count, 200);

                var sortList = new List<Sort>
                {
                    new() { Name = nameof(ChatMessage.SentAt), Type = SortTypes.ASC }
                };

                var chatMessages = await ChatMessageRepository.WhereAsync(x => 
                    ((x.SenderId == userId && x.ReceiverId == receiverId) || (x.SenderId == receiverId && x.ReceiverId == userId)) 
                    && (lastMessageId == 0 || x.Id < lastMessageId)
                    , readOnly: true, sorts: sortList, pagination: new Pagination { Page = 0, Count = count });

                var res = new GetChatMessagesOutput
                {
                    ChatMessages = chatMessages
                };

                return res;
            });
        }

        [HttpGet("overview")]
        [Authorize]
        public async Task<GeneralResponse<GetChatOverviewOutput>> GetChatOverviewAsync([FromQuery] int page, [FromQuery] int count)
        {
            return await WithLoggingGeneralResponseAsync(async () =>
            {
                var userId = GetUserIdGuid();
                count = Math.Min(count, 200);

                var res = await ChatMessageRepository.GetChatOverviewAsync(userId, page, count);

                var chatUserIds = res.Chats.Select(x => x.UserId);

                var users = await UserRepository.WhereAsync(x => chatUserIds.Contains(x.Id));
                var userDictionary = users.ToDictionary(x => x.Id, x => x);

                foreach (var chat in res.Chats)
                {
                    if (userDictionary.TryGetValue(chat.UserId, out var user))
                    {
                        chat.Name = $"{user.FirstName} {user.LastName}";
                        chat.Image = user.Image;
                    }
                    else
                    {
                        chat.Name = "Deleted User"; 
                    }
                }

                return res;
            });
        }

        [HttpPost("message")]
        public async Task<GeneralResponse<int>> SendChatMessageAsync(SendChatMessageInput input)
        {
            return await WithLoggingGeneralResponseAsync<int>(async () =>
            {
                var userId = GetUserIdGuid();
                var userIdStr = GetUserId();

                var chatMessage = new ChatMessage
                {
                    Content = input.Content,
                    SenderId = userId,
                    SentAt = DateTime.UtcNow,
                    IsRead = false,
                    ReceiverId = input.ReceiverId
                };

                await ChatMessageRepository.InsertOneAsync(chatMessage);

                await HubContext.Clients.User(input.ReceiverId.ToString()).SendAsync("ReceiveMessage", chatMessage);

                return chatMessage.Id;
            });
        }

        [HttpPut("read")]
        public async Task<GeneralResponse<object>> ReadAllChatMessagesAsync([FromQuery] Guid senderId)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var userId = GetUserIdGuid();

                var chatMessages = await ChatMessageRepository.WhereAsync(x => x.ReceiverId == userId && x.SenderId == senderId && !x.IsRead);

                if (chatMessages.Count != 0)
                {
                    foreach (var chatMessage in chatMessages)
                    {
                        chatMessage.IsRead = true;
                        chatMessage.ReadAt = DateTime.UtcNow;
                    }

                    await ChatMessageRepository.UpdateManyAsync(chatMessages);

                    await HubContext.Clients.User(chatMessages.First().SenderId.ToString()).SendAsync("ReadMessage", chatMessages.Select(x => x.Id));
                }

                return true;
            });
        }

        [HttpPut("message/read")]
        public async Task<GeneralResponse<object>> ReadChatMessageAsync([FromQuery] int messageId)
        {
            return await WithLoggingGeneralResponseAsync<object>(async () =>
            {
                var userId = GetUserIdGuid();

                var chatMessage = await ChatMessageRepository.FirstOrDefaultAsync(x => x.Id == messageId && x.ReceiverId == userId && !x.IsRead);

                if(chatMessage is not null)
                {
                    chatMessage.IsRead = true;
                    chatMessage.ReadAt = DateTime.UtcNow;

                    await ChatMessageRepository.UpdateOneAsync(chatMessage);

                    await HubContext.Clients.User(chatMessage.SenderId.ToString()).SendAsync("ReadMessage", messageId);
                }

                return true;
            });
        }
    }
}