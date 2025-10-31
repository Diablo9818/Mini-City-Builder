using CityBuilder.Application.DTOs;
using CityBuilder.Application.Events;
using CityBuilder.Domain.Models;
using MessagePipe;

namespace CityBuilder.Application.UseCases
{
    public class MoveBuildingUseCase
    {
        private readonly BuildingGrid _grid;
        private readonly IPublisher<BuildingMovedEvent> _movedPublisher;

        public MoveBuildingUseCase(
            BuildingGrid grid,
            IPublisher<BuildingMovedEvent> movedPublisher)
        {
            _grid = grid;
            _movedPublisher = movedPublisher;
        }

        public void Execute(MoveBuildingDto dto)
        {
            var building = FindBuildingById(dto.BuildingId);
            if (building == null) return;

            if (!_grid.CanPlaceBuilding(dto.NewPosition)) return;

            var oldPosition = building.Position;
            _grid.RemoveBuilding(oldPosition);
            
            building.SetPosition(dto.NewPosition);
            _grid.PlaceBuilding(building);

            _movedPublisher.Publish(new BuildingMovedEvent(building, oldPosition));
        }

        private Building FindBuildingById(string id)
        {
            foreach (var building in _grid.GetAllBuildings())
            {
                if (building.Id == id) return building;
            }
            return null;
        }
    }
}