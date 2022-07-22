using AutoMapper;

namespace VLO_BOARDS.Mappings;

public class BaseMap : Profile
{
    public BaseMap()
    {
        this.RegisterAuthMappings();
    }
}