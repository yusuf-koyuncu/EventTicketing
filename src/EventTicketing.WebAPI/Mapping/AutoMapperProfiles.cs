using AutoMapper;
using EventTicketing.Domain.Dtos;
using EventTicketing.Domain.Entities;

namespace EventTicketing.WebAPI.Mapping;

public class AutoMapperProfiles : Profile
{
    public AutoMapperProfiles()
    {
        CreateMap<Ticket, TicketDto>()
            .ForMember(d => d.TicketId, o => o.MapFrom(s => s.Id));
    }
}
