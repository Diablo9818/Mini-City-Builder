using CityBuilder.Domain.Models;

namespace CityBuilder.Application.Events
{
    public class BuildingPlacedEvent
    {
        public Building Building { get; }
        public BuildingPlacedEvent(Building building) => Building = building;
    }

    public class BuildingRemovedEvent
    {
        public string BuildingId { get; }
        public GridPosition Position { get; }
        public BuildingRemovedEvent(string id, GridPosition pos)
        {
            BuildingId = id;
            Position = pos;
        }
    }

    public class BuildingMovedEvent
    {
        public Building Building { get; }
        public GridPosition OldPosition { get; }
        public BuildingMovedEvent(Building building, GridPosition oldPos)
        {
            Building = building;
            OldPosition = oldPos;
        }
    }

    public class BuildingUpgradedEvent
    {
        public Building Building { get; }
        public BuildingUpgradedEvent(Building building) => Building = building;
    }

    public class ResourcesChangedEvent
    {
        public int Gold { get; }
        public ResourcesChangedEvent(int gold) => Gold = gold;
    }

    public class InsufficientResourcesEvent
    {
        public Cost Required { get; }
        public int Available { get; }
        public InsufficientResourcesEvent(Cost required, int available)
        {
            Required = required;
            Available = available;
        }
    }

    public class GameSavedEvent { }
    
    public class GameLoadedEvent { }

    public class BuildingSelectedEvent
    {
        public Building Building { get; }
        public BuildingSelectedEvent(Building building) => Building = building;
    }

    public class BuildingDeselectedEvent { }
}