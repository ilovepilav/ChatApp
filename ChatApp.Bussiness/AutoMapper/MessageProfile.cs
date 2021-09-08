using AutoMapper;
using ChatApp.Data.DTOs;
using ChatApp.Data.Entities.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChatApp.Bussiness.AutoMapper
{
    public class MessageProfile : Profile
    {
        public MessageProfile()
        {
            CreateMap<Message, MessageDto>().ForMember(d => d.Date, opt => opt.MapFrom(src => src.CreateDate))
                .ReverseMap();
        }
    }
}
