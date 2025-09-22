using AutoMapper;
using BusinessObject.Model;
using Service.RequestAndResponse.Response.Accounts;

namespace Service.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile() 
        {
            CreateMap<Account, GetCustomerUser>().ReverseMap();
            CreateMap<Account, GetStaffUser>().ReverseMap();
            CreateMap<Account, GetAccount>().ReverseMap();

        }
    }
}
