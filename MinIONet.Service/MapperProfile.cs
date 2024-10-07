using AutoMapper;
using System.Diagnostics.Metrics;

namespace MinIONet.Service
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            //CreateMap<ClassObj,ClassObjDto>();
            //CreateMap<ClassObj, ClassObjDto>()
            //  .ForMember(des => des.Property, opt => opt.NullSubstitute(string.Empty));
        }
    }
}
