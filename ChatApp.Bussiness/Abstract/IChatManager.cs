using ChatApp.Data.DTOs;
using ChatApp.Data.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Bussiness.Abstract
{
    public interface IChatManager
    {
        Task<List<ConversationDto>> GetConversationsByUserName(string userName);
        Task<List<string>> GetAllChatRooms();
        Task<bool> CreateChatRoom(string userName, string chatRoomName);
        Task<Conversation> JoinChatRoom(string userName, string chatRoomName);
        Task<bool> CreateConversation(string username, string receipent, bool isPrivate);
        Task SendMessage(MessageDto messageDto);
        Task AddToRoom(string userName);
        Task RemoveFromRoom(string userName);
    }
}
