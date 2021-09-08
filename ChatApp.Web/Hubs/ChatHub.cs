using AutoMapper;
using ChatApp.Bussiness.Abstract;
using ChatApp.Bussiness.Concrete;
using ChatApp.Bussiness.Helpers;
using ChatApp.Data.DTOs;
using ChatApp.Data.Entities.Concrete;
using ChatApp.Web.RabbitMQ;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatApp.Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatManager _chatManager;
        private readonly IMapper _mapper;
        private readonly AddMessageToQeueue _messageQueue;

        public ChatHub(IChatManager chatManager, IMapper mapper, AddMessageToQeueue messageQueue)
        {
            _chatManager = chatManager;
            _mapper = mapper;
            _messageQueue = messageQueue;
        }

        public async Task SendMessage(string receiver, string message, bool isPrivateChat)
        {
            if (string.IsNullOrEmpty(message))
            {
                await SendErrorToUser(Context.User.Identity.Name,"Please don't send empty messages, it breaks my heart :(");
            }

            DateTime messageDate = DateTime.Now;
            MessageDto messageDto = new MessageDto() { Content = message, Date = messageDate, Sender = Context.User.Identity.Name, Receiver = receiver, IsPrivate = isPrivateChat };
            _messageQueue.SendMessageToQueue(messageDto);
            await NewMessage(messageDto);
        }


        public async override Task OnConnectedAsync()
        {
            string userIdentityName = Context.User.Identity.Name;
            if (String.IsNullOrEmpty(userIdentityName))
            {
                return;
            }
            if (!ConnectedUsers.UserList.ContainsKey(userIdentityName))
            {
                ConnectedUsers.UserList.Add(userIdentityName, new HashSet<string>() { Context.ConnectionId });
                await _chatManager.AddToRoom(Context.User.Identity.Name);
                await RefreshOnlineClients();
                await GetConversations(userIdentityName);
                await RefreshGroupChatList();
                await base.OnConnectedAsync();
                return;
            }
            ConnectedUsers.UserList[userIdentityName].Add(Context.ConnectionId);
            await _chatManager.AddToRoom(Context.User.Identity.Name);
            await RefreshOnlineClients();
            await GetConversations(userIdentityName);
            await RefreshGroupChatList();
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception exception)
        {
            string userIdentityName = Context.User.Identity.Name;
            ConnectedUsers.UserList[userIdentityName].Remove(Context.ConnectionId);
            if (ConnectedUsers.UserList[userIdentityName].Count() == 0)
            {
                ConnectedUsers.UserList.Remove(userIdentityName);
                await _chatManager.RemoveFromRoom(userIdentityName);
            }
            await RefreshOnlineClients();
            await base.OnDisconnectedAsync(exception);
        }

        public async Task RefreshOnlineClients()
        {
            string[] data = ConnectedUsers.UserList.Keys.ToArray();
            await Clients.All.SendAsync("ClientsRefreshed", data);
        }

        public async Task GetConversations(string userName = null, string[] userNames = null) //method overload'a signalR izin vermiyor.
        {
            if (userNames == null)
            {
                List<ConversationDto> conversations = await _chatManager.GetConversationsByUserName(userName);
                foreach (string item in ConnectedUsers.UserList[userName])
                {
                    await Clients.Client(item).SendAsync("Conversations", conversations);
                }
                return;
            }

            foreach (string user in userNames)
            {
                List<ConversationDto> conversations = await _chatManager.GetConversationsByUserName(user);
                foreach (string item in ConnectedUsers.UserList[user])
                {
                    await Clients.Client(item).SendAsync("Conversations", conversations);
                }
            }
        }

        public async Task CreateChatRoom(string roomName)
        {
            if (string.IsNullOrEmpty(roomName))
            {
                await SendErrorToUser(Context.User.Identity.Name, "Please create rooms that have names, it breaks my heart :(");
            }

            bool result = await _chatManager.CreateConversation(Context.User.Identity.Name, roomName, false);
            if (!result)
            {
                await SendErrorToUser(Context.User.Identity.Name, $"There is already a chatroom called {roomName}");
                return;
            }
            ConversationDto conversationDto = new ConversationDto() { PrivateChat = false, Receipent = roomName, Messages = new List<MessageDto>() };

            await _chatManager.CreateChatRoom(Context.User.Identity.Name, roomName);
            await RefreshGroupChatList();

            foreach (string item in ConnectedUsers.UserList[Context.User.Identity.Name])
            {
                await Clients.Client(item.ToString()).SendAsync("NewConversation", conversationDto);
            }
        }

        public async Task CreateChat(string receipent)
        {
            bool result = await _chatManager.CreateConversation(Context.User.Identity.Name, receipent, true);
            if (!result)
            {
                await SendErrorToUser(Context.User.Identity.Name, $"You have already a conversation with {receipent}");
                return;
            }
            string[] users = { Context.User.Identity.Name, receipent };
            //await GetConversations(null, users);

            foreach (string user in users)
            {
                ConversationDto conversationDto = new ConversationDto() {Receipent=users.SkipWhile(u=>u==user).FirstOrDefault(),Messages = new List<MessageDto>(), PrivateChat=true };
                foreach (string item in ConnectedUsers.UserList[user])
                {
                    await Clients.Client(item.ToString()).SendAsync("NewConversation", conversationDto);
                }
            }
        }

        public async Task RefreshGroupChatList()
        {
            List<string> chatrooms = await _chatManager.GetAllChatRooms();
            await Clients.All.SendAsync("RefreshChatRooms", chatrooms);
        }

        public async Task JoinChatRoom(string chatRoomName)
        {
            Conversation result = await _chatManager.JoinChatRoom(Context.User.Identity.Name, chatRoomName);
            if (result != null)
            {
                await _chatManager.AddToRoom(Context.User.Identity.Name);
                List<MessageDto> messageDtos = new List<MessageDto>();
                foreach (Message item in result.Messages)
                {
                    MessageDto msg = _mapper.Map<MessageDto>(item);
                    messageDtos.Add(msg);
                }
                ConversationDto conversationDto = new ConversationDto() { PrivateChat = false, Receipent = chatRoomName, Messages = messageDtos };
                foreach (string item in ConnectedUsers.UserList[Context.User.Identity.Name])
                {
                    await Clients.Client(item.ToString()).SendAsync("NewConversation", conversationDto);
                }
                return;
            }
            await SendErrorToUser(Context.User.Identity.Name, $"You already a member of {chatRoomName} chatroom.");
        }

        public async Task SendErrorToUser(string userName, string errorMessage)
        {
            foreach (string item in ConnectedUsers.UserList[userName])
            {
                await Clients.Client(item.ToString()).SendAsync("Error", errorMessage);
            }
        }

        public async Task NewMessage(MessageDto messageDto)
        {
            if (messageDto.IsPrivate)
            {
                foreach (string item in new List<string> {messageDto.Sender, messageDto.Receiver })
                {
                    if (ConnectedUsers.UserList.ContainsKey(item))
                    {
                        foreach (string userId in ConnectedUsers.UserList[item])
                        {
                            await Clients.Client(userId).SendAsync("NewMessage", messageDto.Receiver, messageDto);
                        }
                    }
                }
                return;
            }

            foreach (var item in ChatRooms.chatRooms[messageDto.Receiver])
            {
                if (ConnectedUsers.UserList.ContainsKey(item))
                {
                    foreach (string userId in ConnectedUsers.UserList[item])
                    {
                        await Clients.Client(userId).SendAsync("NewMessage", messageDto.Receiver, messageDto);
                    }
                }
            }
        }
    }
}
