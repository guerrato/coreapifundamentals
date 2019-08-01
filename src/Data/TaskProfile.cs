using System;
using AutoMapper;
using CoreCodeCamp.Models;

namespace CoreCodeCamp.Data
{
    public class TaskProfile : Profile
    {
        public TaskProfile()
        {
            this.CreateMap<Talk, TalkModel>();
        }
    }
}
