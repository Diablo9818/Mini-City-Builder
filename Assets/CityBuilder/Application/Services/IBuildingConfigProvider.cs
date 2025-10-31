using CityBuilder.Domain.Models;

namespace CityBuilder.Application.Services
{
    public interface IBuildingConfigProvider
    {
        BuildingConfig GetConfig(BuildingType type);
    }
}