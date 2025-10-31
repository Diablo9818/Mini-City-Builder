using CityBuilder.Application.DTOs;
using CityBuilder.Application.Events;
using CityBuilder.Domain.Models;
using MessagePipe;

namespace CityBuilder.Application.UseCases
{
    public class RemoveBuildingUseCase
    {
        private readonly BuildingGrid _grid;
        private readonly IPublisher<BuildingRemovedEvent> _removedPublisher;

        public RemoveBuildingUseCase(
            BuildingGrid grid,
            IPublisher<BuildingRemovedEvent> removedPublisher)
        {
            _grid = grid;
            _removedPublisher = removedPublisher;
        }

        public void Execute(RemoveBuildingDto dto)
        {
            var building = FindBuildingById(dto.BuildingId);
            if (building == null) return;

            _grid.RemoveBuilding(building.Position);
            _removedPublisher.Publish(new BuildingRemovedEvent(building.Id, building.Position));
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