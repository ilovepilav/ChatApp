using ChatApp.Bussiness.Abstract;
using ChatApp.Bussiness.Helpers;
using ChatApp.Data.Context;
using ChatApp.Data.DTOs;
using ChatApp.Data.Entities.Concrete;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Bussiness.Concrete
{
    public class ChatManager : IChatManager
    {
        private readonly ChatAppWebContext _context;

        public ChatManager(ChatAppWebContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateChatRoom(string userName, string chatRoomName)
        {
            if (await _context.Set<Conversation>().AnyAsync(c => c.Name == chatRoomName))
            {
                return false;
            }
            ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(u => u.UserName == userName);
            Conversation conversation = new Conversation
            {
                Name = chatRoomName,
                PrivateChat = false,
            };
            conversation.Users.Add(user);
            await _context.Set<Conversation>().AddAsync(conversation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CreateConversation(string username, string receipent, bool isPrivate)
        {
            Conversation conversation = new Conversation();

            if (isPrivate)
            {
                ApplicationUser receipentUser = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(u => u.UserName == receipent);
                string conversationName = username + receipent; //refactor
                List<Conversation> privateConversations = await _context.Set<Conversation>().Include(u => u.Users).Where(c => c.PrivateChat == true).ToListAsync();
                Conversation privateConversation = privateConversations.FirstOrDefault(c => c.Users.Contains(receipentUser));
                if (privateConversation is not null)
                {
                    return false;
                }
                conversation.Users.Add(receipentUser);
                receipent = conversationName;
            }
            if (!isPrivate && await _context.Set<Conversation>().AnyAsync(c => c.Name == receipent))
            {
                return false;
            }

            ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(u => u.UserName == username);
            conversation.Name = receipent;
            conversation.PrivateChat = isPrivate;
            conversation.Users.Add(user);
            await _context.Set<Conversation>().AddAsync(conversation);
            await _context.SaveChangesAsync();
            ChatRooms.chatRooms.Add(receipent, new HashSet<string>() { user.UserName });
            return true;
        }

        public async Task<List<string>> GetAllChatRooms()
        {
            var list = await _context.Set<Conversation>().Where(c => c.PrivateChat == false).ToListAsync();
            return list.Select(x => x.Name).ToList();
        }

        public async Task<List<ConversationDto>> GetConversationsByUserName(string userName)
        {
            ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(u => u.UserName == userName);
            List<Conversation> conversations = await _context.Set<Conversation>().Include(c => c.Messages).Include(c => c.Users).AsSplitQuery().Where(c => c.Users.Contains(user)).OrderByDescending(c => c.UpdateDate).ToListAsync();
            return MapConversations(userName, conversations); //refactor order by modifided date desc
        }

        public async Task SendMessage(MessageDto messageDto)
        {
            Message msg = new Message { Sender = messageDto.Sender, Content = messageDto.Content };
            if (messageDto.IsPrivate)
            {
                ApplicationUser sender = await _context.Set<ApplicationUser>().Include(u => u.Conversations).SingleOrDefaultAsync(u => u.UserName == messageDto.Sender);
                ApplicationUser receiver = await _context.Set<ApplicationUser>().Include(u => u.Conversations).SingleOrDefaultAsync(u => u.UserName == messageDto.Receiver);

                Conversation convSenderAndReceiver = sender.Conversations.Where(c => c.PrivateChat == true).FirstOrDefault(u => u.Users.Contains(receiver));
                convSenderAndReceiver.Messages.Add(msg);
                convSenderAndReceiver.UpdateDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return;

            }
            Conversation conversation = await _context.Set<Conversation>().Include(c => c.Messages).Include(c => c.Users).AsSplitQuery().SingleOrDefaultAsync(c => c.Name == messageDto.Receiver);

            conversation.Messages.Add(msg);
            conversation.UpdateDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public List<ConversationDto> MapConversations(string sender, List<Conversation> conversations) //refactor automapper
        {
            List<ConversationDto> mappedConversationDtos = new List<ConversationDto>();
            foreach (Conversation item in conversations)
            {
                ConversationDto conversationDto = new ConversationDto();
                foreach (Message msg in item.Messages)
                {
                    MessageDto messageDto = new MessageDto();
                    messageDto.Content = msg.Content;
                    messageDto.Sender = msg.Sender;
                    messageDto.Date = msg.CreateDate;
                    conversationDto.Messages.Add(messageDto);
                }
                if (item.PrivateChat)
                {
                    conversationDto.PrivateChat = true;
                    conversationDto.Receipent = item.Users.SkipWhile(u => u.UserName == sender).FirstOrDefault().UserName;
                }
                else
                {
                    conversationDto.PrivateChat = false;
                    conversationDto.Receipent = item.Name;
                }

                mappedConversationDtos.Add(conversationDto);
            }
            return mappedConversationDtos;
        }

        public async Task<Conversation> JoinChatRoom(string userName, string chatRoomName)
        {
            ApplicationUser user = await _context.Set<ApplicationUser>().SingleOrDefaultAsync(u => u.UserName == userName);
            Conversation chatRoom = await _context.Set<Conversation>().Include(c => c.Messages).Include(c => c.Users).AsSplitQuery().SingleOrDefaultAsync(c => c.Name == chatRoomName);
            if (chatRoom.Users.Contains(user))
            {
                return null;
            }
            chatRoom.Users.Add(user);
            await _context.SaveChangesAsync();
            return chatRoom;
        }

        public async Task AddToRoom(string userName)
        {
            ApplicationUser user = await _context.Set<ApplicationUser>().Include(u => u.Conversations).SingleOrDefaultAsync(u => u.UserName == userName);

            List<Conversation> userConversations = user.Conversations.Where(c => c.PrivateChat == false).ToList();

            if (userConversations.Count == 0)
            {
                return;
            }

            List<string> usersChatRooms = userConversations.Select(c => c.Name).ToList();

            foreach (string item in usersChatRooms)
            {
                if (ChatRooms.chatRooms.ContainsKey(item) && !ChatRooms.chatRooms[item].Contains(userName))
                {
                    ChatRooms.chatRooms[item].Add(userName);
                }
                if (!ChatRooms.chatRooms.ContainsKey(item))
                {
                    ChatRooms.chatRooms.Add(item, new HashSet<string> { userName });
                }
            }
        }

        public async Task RemoveFromRoom(string userName)
        {
            ApplicationUser user = await _context.Set<ApplicationUser>().Include(u => u.Conversations).SingleOrDefaultAsync(u=>u.UserName==userName);

            List<Conversation> userConversations = user.Conversations.Where(c => c.PrivateChat == false).ToList();

            if (userConversations.Count == 0)
            {
                return;
            }

            List<string> usersChatRooms = userConversations.Select(c => c.Name).ToList();

            foreach (string item in usersChatRooms)
            {
                if (ChatRooms.chatRooms.ContainsKey(item))
                {
                    ChatRooms.chatRooms[item].Remove(userName);
                }
            }
        }
    }
}
