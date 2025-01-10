    using AutoMapper;
    using Models.DBModel;
    using Models.DTOModel.Users;
namespace LibraryManagementSystem.API.Mapping
{

    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
      
            CreateMap<User, GetUserDTO>();
            CreateMap<GetUserDTO, User>();
            CreateMap<UpdateUserDTO, User>();
            CreateMap<User, UpdateUserDTO>();
        }
    }

}
