using CityBuilder.Application.DTOs;
using CityBuilder.Application.Events;
using CityBuilder.Application.Services;
using CityBuilder.Domain.Models;
using MessagePipe;

namespace CityBuilder.Application.UseCases
{
    public class UpgradeBuildingUseCase
    {
        private readonly BuildingGrid _grid;
        private readonly GameResources _resources;
        private readonly IBuildingConfigProvider _configProvider;
        private readonly IPublisher<BuildingUpgradedEvent> _upgradedPublisher;
        private readonly IPublisher<ResourcesChangedEvent> _resourcesPublisher;
        private readonly IPublisher<InsufficientResourcesEvent> _insufficientPublisher;

        public UpgradeBuildingUseCase(
            BuildingGrid grid,
            GameResources resources,
            IBuildingConfigProvider configProvider,
            IPublisher<BuildingUpgradedEvent> upgradedPublisher,
            IPublisher<ResourcesChangedEvent> resourcesPublisher,
            IPublisher<InsufficientResourcesEvent> insufficientPublisher)
        {
            _grid = grid;
            _resources = resources;
            _configProvider = configProvider;
            _upgradedPublisher = upgradedPublisher;
            _resourcesPublisher = resourcesPublisher;
            _insufficientPublisher = insufficientPublisher;
        }

        public void Execute(UpgradeBuildingDto dto)
        {
            var building = FindBuildingById(dto.BuildingId);
            if (building == null) return;

            var config = _configProvider.GetConfig(building.Type);
            
            if (!building.CanUpgrade(config.MaxLevel)) return;

            var upgradeCost = config.GetCostForLevel(building.Level + 1);

            if (!_resources.CanAfford(upgradeCost))
            {
                _insufficientPublisher.Publish(new InsufficientResourcesEvent(upgradeCost, _resources.Gold));
                return;
            }

            _resources.Spend(upgradeCost);
            building.Upgrade();

            _upgradedPublisher.Publish(new BuildingUpgradedEvent(building));
            _resourcesPublisher.Publish(new ResourcesChangedEvent(_resources.Gold));
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