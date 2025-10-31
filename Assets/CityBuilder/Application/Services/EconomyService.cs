using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using CityBuilder.Application.Events;
using CityBuilder.Domain.Models;
using MessagePipe;

namespace CityBuilder.Application.Services
{
    public class EconomyService : IDisposable
    {
        private readonly BuildingGrid _grid;
        private readonly GameResources _resources;
        private readonly IBuildingConfigProvider _configProvider;
        private readonly IPublisher<ResourcesChangedEvent> _resourcesPublisher;
        private readonly float _tickInterval;
        
        private CancellationTokenSource _cts;

        public EconomyService(
            BuildingGrid grid,
            GameResources resources,
            IBuildingConfigProvider configProvider,
            IPublisher<ResourcesChangedEvent> resourcesPublisher,
            float tickInterval = 2f)
        {
            _grid = grid;
            _resources = resources;
            _configProvider = configProvider;
            _resourcesPublisher = resourcesPublisher;
            _tickInterval = tickInterval;
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            IncomeLoopAsync(_cts.Token).Forget();
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async UniTaskVoid IncomeLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_tickInterval), cancellationToken: ct);
                
                if (ct.IsCancellationRequested) break;

                var totalIncome = CalculateTotalIncome();
                _resources.Add(totalIncome);
                _resourcesPublisher.Publish(new ResourcesChangedEvent(_resources.Gold));
            }
        }

        private int CalculateTotalIncome()
        {
            var total = 0;
            foreach (var building in _grid.GetAllBuildings())
            {
                var config = _configProvider.GetConfig(building.Type);
                var income = config.GetIncomeForLevel(building.Level);
                total += income.GoldPerTick;
            }
            return total;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}