using CityBuilder.Application.DTOs;
using CityBuilder.Application.Events;
using CityBuilder.Application.Services;
using CityBuilder.Application.UseCases;
using CityBuilder.Domain.Models;
using CityBuilder.Infrastructure.Services;
using CityBuilder.Presentation.Presenters;
using MessagePipe;
using MessagePipe.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CityBuilder.CompositionRoot 
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GamePresenter _gamePresenter;
        [SerializeField] private BuildingConfigProvider _buildingConfigProvider;
        [SerializeField] private int _gridWidth = 32;
        [SerializeField] private int _gridHeight = 32;
        [SerializeField] private int _initialGold = 1000;
        [SerializeField] private float _incomeTickInterval = 2f;
        [SerializeField] private float _autoSaveInterval = 30f;
        
        
        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("GameEntryPoint Configure");
            // MessagePipe
            var options = builder.RegisterMessagePipe();
            builder.RegisterBuildCallback(c => GlobalMessagePipe.SetProvider(c.AsServiceProvider()));

            // Domain Models
            builder.Register<BuildingGrid>(Lifetime.Singleton)
                .WithParameter(_gridWidth)
                .WithParameter(_gridHeight);
            
            builder.Register<GameResources>(Lifetime.Singleton)
                .WithParameter(_initialGold);

            // Application Services
            builder.Register<IIdGenerator, GuidIdGenerator>(Lifetime.Singleton);
            builder.RegisterInstance<IBuildingConfigProvider>(_buildingConfigProvider);

            // Use Cases
            builder.Register<PlaceBuildingUseCase>(Lifetime.Singleton);
            builder.Register<RemoveBuildingUseCase>(Lifetime.Singleton);
            builder.Register<MoveBuildingUseCase>(Lifetime.Singleton);
            builder.Register<UpgradeBuildingUseCase>(Lifetime.Singleton);

            // Infrastructure Services
            builder.Register<EconomyService>(Lifetime.Singleton)
                .WithParameter(_incomeTickInterval)
                .AsSelf()
                .AsImplementedInterfaces();
            
            builder.Register<SaveLoadService>(Lifetime.Singleton)
                .WithParameter(_autoSaveInterval)
                .AsSelf()
                .AsImplementedInterfaces();

            // MessagePipe Publishers & Subscribers
            RegisterMessagePipe<PlaceBuildingDto>(builder);
            RegisterMessagePipe<RemoveBuildingDto>(builder);
            RegisterMessagePipe<MoveBuildingDto>(builder);
            RegisterMessagePipe<UpgradeBuildingDto>(builder);
            RegisterMessagePipe<SaveGameDto>(builder);
            RegisterMessagePipe<LoadGameDto>(builder);
            
            RegisterMessagePipe<BuildingPlacedEvent>(builder);
            RegisterMessagePipe<BuildingRemovedEvent>(builder);
            RegisterMessagePipe<BuildingMovedEvent>(builder);
            RegisterMessagePipe<BuildingUpgradedEvent>(builder);
            RegisterMessagePipe<ResourcesChangedEvent>(builder);
            RegisterMessagePipe<InsufficientResourcesEvent>(builder);
            RegisterMessagePipe<GameSavedEvent>(builder);
            RegisterMessagePipe<GameLoadedEvent>(builder);
            RegisterMessagePipe<BuildingSelectedEvent>(builder);
            RegisterMessagePipe<BuildingDeselectedEvent>(builder);

            // Presentation
            builder.RegisterComponent(_gamePresenter);
            
            // Entry Point
            builder.RegisterEntryPoint<GameEntryPoint>();
        }

        private void RegisterMessagePipe<T>(IContainerBuilder builder)
        {
            builder.RegisterMessageBroker<T>(new MessagePipeOptions());
        }
    }
}