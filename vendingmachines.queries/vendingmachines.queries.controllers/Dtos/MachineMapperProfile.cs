using AutoMapper;
using vendingmachines.queries.entities;

namespace vendingmachines.queries.controllers.Dtos;

public class MachineMapperProfile : Profile
{
    public MachineMapperProfile()
    {
        CreateMap<Machine, MachineDto>().ReverseMap();
        CreateMap<Product, ProductDto>().ReverseMap();
    }
}
