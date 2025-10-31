using System;
using CityBuilder.Application.DTOs;
using CityBuilder.Application.Events;
using CityBuilder.Application.Services;
using CityBuilder.Application.UseCases;
using CityBuilder.Domain.Models;
using CityBuilder.Infrastructure.Services; 
using CityBuilder.Presentation.Presenters;
using MessagePipe;
using UnityEngine;
using VContainer.Unity;

namespace CityBuilder.CompositionRoot
{
    public class GameEntryPoint : IStartable, IDisposable
    {
        private readonly GamePresenter _gamePresenter;
        private readonly EconomyService _economyService;
        private readonly SaveLoadService _saveLoadService;
        private readonly GameResources _resources;
        private readonly BuildingGrid _grid;
        
        private readonly PlaceBuildingUseCase _placeBuildingUseCase;
        private readonly RemoveBuildingUseCase _removeBuildingUseCase;
        private readonly MoveBuildingUseCase _moveBuildingUseCase;
        private readonly UpgradeBuildingUseCase _upgradeBuildingUseCase;
        
        private readonly ISubscriber<PlaceBuildingDto> _placeBuildingSubscriber;
        private readonly ISubscriber<RemoveBuildingDto> _removeBuildingSubscriber;
        private readonly ISubscriber<MoveBuildingDto> _moveBuildingSubscriber;
        private readonly ISubscriber<UpgradeBuildingDto> _upgradeBuildingSubscriber;
        private readonly ISubscriber<SaveGameDto> _saveGameSubscriber;
        private readonly ISubscriber<LoadGameDto> _loadGameSubscriber;
        
        private readonly IPublisher<PlaceBuildingDto> _placeBuildingPublisher;
        private readonly IPublisher<RemoveBuildingDto> _removeBuildingPublisher;
        private readonly IPublisher<MoveBuildingDto> _moveBuildingPublisher;
        private readonly IPublisher<UpgradeBuildingDto> _upgradeBuildingPublisher;
        private readonly IPublisher<SaveGameDto> _saveGamePublisher;
        private readonly IPublisher<LoadGameDto> _loadGamePublisher;
        
        private readonly ISubscriber<BuildingPlacedEvent> _buildingPlacedSubscriber;
        private readonly ISubscriber<BuildingRemovedEvent> _buildingRemovedSubscriber;
        private readonly ISubscriber<BuildingMovedEvent> _buildingMovedSubscriber;
        private readonly ISubscriber<BuildingUpgradedEvent> _buildingUpgradedSubscriber;
        private readonly ISubscriber<ResourcesChangedEvent> _resourcesChangedSubscriber;
        private readonly ISubscriber<InsufficientResourcesEvent> _insufficientResourcesSubscriber;
        private readonly ISubscriber<GameSavedEvent> _gameSavedSubscriber;
        private readonly ISubscriber<GameLoadedEvent> _gameLoadedSubscriber;
        
        private readonly IPublisher<ResourcesChangedEvent> _resourcesPublisher;

        private IDisposable _subscriptions;

        public GameEntryPoint(
            GamePresenter gamePresenter,
            EconomyService economyService,
            SaveLoadService saveLoadService,
            GameResources resources,
            BuildingGrid grid,
            PlaceBuildingUseCase placeBuildingUseCase,
            RemoveBuildingUseCase removeBuildingUseCase,
            MoveBuildingUseCase moveBuildingUseCase,
            UpgradeBuildingUseCase upgradeBuildingUseCase,
            ISubscriber<PlaceBuildingDto> placeBuildingSubscriber,
            ISubscriber<RemoveBuildingDto> removeBuildingSubscriber,
            ISubscriber<MoveBuildingDto> moveBuildingSubscriber,
            ISubscriber<UpgradeBuildingDto> upgradeBuildingSubscriber,
            ISubscriber<SaveGameDto> saveGameSubscriber,
            ISubscriber<LoadGameDto> loadGameSubscriber,
            IPublisher<PlaceBuildingDto> placeBuildingPublisher,
            IPublisher<RemoveBuildingDto> removeBuildingPublisher,
            IPublisher<MoveBuildingDto> moveBuildingPublisher,
            IPublisher<UpgradeBuildingDto> upgradeBuildingPublisher,
            IPublisher<SaveGameDto> saveGamePublisher,
            IPublisher<LoadGameDto> loadGamePublisher,
            ISubscriber<BuildingPlacedEvent> buildingPlacedSubscriber,
            ISubscriber<BuildingRemovedEvent> buildingRemovedSubscriber,
            ISubscriber<BuildingMovedEvent> buildingMovedSubscriber,
            ISubscriber<BuildingUpgradedEvent> buildingUpgradedSubscriber,
            ISubscriber<ResourcesChangedEvent> resourcesChangedSubscriber,
            ISubscriber<InsufficientResourcesEvent> insufficientResourcesSubscriber,
            ISubscriber<GameSavedEvent> gameSavedSubscriber,
            ISubscriber<GameLoadedEvent> gameLoadedSubscriber,
            IPublisher<ResourcesChangedEvent> resourcesPublisher)
        {
            
            Debug.Log("Game Entry Point Constructor");
            _gamePresenter = gamePresenter;
            _economyService = economyService;
            _saveLoadService = saveLoadService;
            _resources = resources;
            _grid = grid;
            
            _placeBuildingUseCase = placeBuildingUseCase;
            _removeBuildingUseCase = removeBuildingUseCase;
            _moveBuildingUseCase = moveBuildingUseCase;
            _upgradeBuildingUseCase = upgradeBuildingUseCase;
            
            _placeBuildingSubscriber = placeBuildingSubscriber;
            _removeBuildingSubscriber = removeBuildingSubscriber;
            _moveBuildingSubscriber = moveBuildingSubscriber;
            _upgradeBuildingSubscriber = upgradeBuildingSubscriber;
            _saveGameSubscriber = saveGameSubscriber;
            _loadGameSubscriber = loadGameSubscriber;
            
            _placeBuildingPublisher = placeBuildingPublisher;
            _removeBuildingPublisher = removeBuildingPublisher;
            _moveBuildingPublisher = moveBuildingPublisher;
            _upgradeBuildingPublisher = upgradeBuildingPublisher;
            _saveGamePublisher = saveGamePublisher;
            _loadGamePublisher = loadGamePublisher;
            
            _buildingPlacedSubscriber = buildingPlacedSubscriber;
            _buildingRemovedSubscriber = buildingRemovedSubscriber;
            _buildingMovedSubscriber = buildingMovedSubscriber;
            _buildingUpgradedSubscriber = buildingUpgradedSubscriber;
            _resourcesChangedSubscriber = resourcesChangedSubscriber;
            _insufficientResourcesSubscriber = insufficientResourcesSubscriber;
            _gameSavedSubscriber = gameSavedSubscriber;
            _gameLoadedSubscriber = gameLoadedSubscriber;
            
            _resourcesPublisher = resourcesPublisher;
        }
        
        public void Start()
        {
            // Initialize Presenter
            _gamePresenter.Initialize(
                _grid,
                _placeBuildingPublisher,
                _removeBuildingPublisher,
                _moveBuildingPublisher,
                _upgradeBuildingPublisher,
                _saveGamePublisher,
                _loadGamePublisher,
                _buildingPlacedSubscriber,
                _buildingRemovedSubscriber,
                _buildingMovedSubscriber,
                _buildingUpgradedSubscriber,
                _resourcesChangedSubscriber,
                _insufficientResourcesSubscriber,
                _gameSavedSubscriber,
                _gameLoadedSubscriber);

            // Subscribe to DTOs
            var bag = DisposableBag.CreateBuilder();
            
            _placeBuildingSubscriber.Subscribe(dto => _placeBuildingUseCase.Execute(dto)).AddTo(bag);
            _removeBuildingSubscriber.Subscribe(dto => _removeBuildingUseCase.Execute(dto)).AddTo(bag);
            _moveBuildingSubscriber.Subscribe(dto => _moveBuildingUseCase.Execute(dto)).AddTo(bag);
            _upgradeBuildingSubscriber.Subscribe(dto => _upgradeBuildingUseCase.Execute(dto)).AddTo(bag);
            _saveGameSubscriber.Subscribe(_ => _saveLoadService.Save()).AddTo(bag);
            _loadGameSubscriber.Subscribe(_ => _saveLoadService.Load()).AddTo(bag);

            _subscriptions = bag.Build();

            // Start Services
            _economyService.Start();
            _saveLoadService.StartAutoSave();

            // Publish initial resources
            _resourcesPublisher.Publish(new ResourcesChangedEvent(_resources.Gold));
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
            _economyService?.Dispose();
            _saveLoadService?.Dispose();
        }
    }
}