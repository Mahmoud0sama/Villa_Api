using AutoMapper;
using Villa_API.Models;
using Villa_API.Models.Dto;

namespace Villa_API
{
	public class MappingConfig:Profile
	{
        public MappingConfig()
        {
            CreateMap<Villa,VillaDTO>().ReverseMap();
			CreateMap<Villa,VillaCreateDTO>().ReverseMap();
            CreateMap<Villa,VillaUpdateDTO>().ReverseMap();

			CreateMap<VillaNumber, VillaNumberDTO>().ReverseMap();
			CreateMap<VillaNumber, VillaNumberCreateDTO>().ReverseMap();
			CreateMap<VillaNumber, VillaNumberUpdateDTO>().ReverseMap();
		}
    }
}
