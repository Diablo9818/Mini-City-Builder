using CityBuilder.Application.DTOs;
using CityBuilder.Application.Events;
using CityBuilder.Application.Services;
using CityBuilder.Domain.Models;
using MessagePipe;

namespace CityBuilder.Application.UseCases
{
    public class PlaceBuildingUseCase
    {
        private readonly BuildingGrid _grid;
        private readonly GameResources _resources;
        private readonly IBuildingConfigProvider _configProvider;
        private readonly IIdGenerator _idGenerator;
        private readonly IPublisher<BuildingPlacedEvent> _placedPublisher;
        private readonly IPublisher<ResourcesChangedEvent> _resourcesPublisher;
        private readonly IPublisher<InsufficientResourcesEvent> _insufficientPublisher;

        public PlaceBuildingUseCase(
            BuildingGrid grid,
            GameResources resources,
            IBuildingConfigProvider configProvider,
            IIdGenerator idGenerator,
            IPublisher<BuildingPlacedEvent> placedPublisher,
            IPublisher<ResourcesChangedEvent> resourcesPublisher,
            IPublisher<InsufficientResourcesEvent> insufficientPublisher)
        {
            _grid = grid;
            _resources = resources;
            _configProvider = configProvider;
            _idGenerator = idGenerator;
            _placedPublisher = placedPublisher;
            _resourcesPublisher = resourcesPublisher;
            _insufficientPublisher = insufficientPublisher;
        }

        public void Execute(PlaceBuildingDto dto)
        {
            if (!_grid.CanPlaceBuilding(dto.Position))
            {
                return;
            }

            var config = _configProvider.GetConfig(dto.Type);
            var cost = config.BaseCost;

            if (!_resources.CanAfford(cost))
            {
                _insufficientPublisher.Publish(new InsufficientResourcesEvent(cost, _resources.Gold));
                return;
            }

            _resources.Spend(cost);
            
            var building = new Building(_idGenerator.Generate(), dto.Type, dto.Position);
            _grid.PlaceBuilding(building);

            _placedPublisher.Publish(new BuildingPlacedEvent(building));
            _resourcesPublisher.Publish(new ResourcesChangedEvent(_resources.Gold));
        }
    }
}