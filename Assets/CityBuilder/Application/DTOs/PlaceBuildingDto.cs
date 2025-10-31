using CityBuilder.Domain.Models;

namespace CityBuilder.Application.DTOs
{
    public class PlaceBuildingDto
    {
        public BuildingType Type { get; set; }
        public GridPosition Position { get; set; }
    }

    public class MoveBuildingDto
    {
        public string BuildingId { get; set; }
        public GridPosition NewPosition { get; set; }
    }

    public class UpgradeBuildingDto
    {
        public string BuildingId { get; set; }
    }

    public class RemoveBuildingDto
    {
        public string BuildingId { get; set; }
    }

    public class SaveGameDto { }
    
    public class LoadGameDto { }
}