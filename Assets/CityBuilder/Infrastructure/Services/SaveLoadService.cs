using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using CityBuilder.Application.DTOs;
using CityBuilder.Application.Events;
using CityBuilder.Domain.Models;
using MessagePipe;
using UnityEngine;

namespace CityBuilder.Infrastructure.Services
{
    public class SaveLoadService : IDisposable
    {
        private readonly BuildingGrid _grid;
        private readonly GameResources _resources;
        private readonly IPublisher<GameSavedEvent> _savedPublisher;
        private readonly IPublisher<GameLoadedEvent> _loadedPublisher;
        private readonly float _autoSaveInterval;
        
        private CancellationTokenSource _cts;
        private const string SaveKey = "CityBuilderSave";

        public SaveLoadService(
            BuildingGrid grid,
            GameResources resources,
            IPublisher<GameSavedEvent> savedPublisher,
            IPublisher<GameLoadedEvent> loadedPublisher,
            float autoSaveInterval = 30f)
        {
            _grid = grid;
            _resources = resources;
            _savedPublisher = savedPublisher;
            _loadedPublisher = loadedPublisher;
            _autoSaveInterval = autoSaveInterval;
        }

        public void StartAutoSave()
        {
            _cts = new CancellationTokenSource();
            AutoSaveLoopAsync(_cts.Token).Forget();
        }

        public void StopAutoSave()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private async UniTaskVoid AutoSaveLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(_autoSaveInterval), cancellationToken: ct);
                
                if (ct.IsCancellationRequested) break;
                
                Save();
            }
        }

        public void Save()
        {
            var saveData = new SaveData
            {
                Gold = _resources.Gold,
                Buildings = new List<BuildingSaveData>()
            };

            foreach (var building in _grid.GetAllBuildings())
            {
                saveData.Buildings.Add(new BuildingSaveData
                {
                    Id = building.Id,
                    Type = building.Type,
                    X = building.Position.X,
                    Y = building.Position.Y,
                    Level = building.Level
                });
            }

            var json = JsonUtility.ToJson(saveData, true);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.Save();

            _savedPublisher.Publish(new GameSavedEvent());
        }

        public void Load()
        {
            if (!PlayerPrefs.HasKey(SaveKey)) return;

            var json = PlayerPrefs.GetString(SaveKey);
            var saveData = JsonUtility.FromJson<SaveData>(json);

            _grid.Clear();
            _resources.SetGold(saveData.Gold);

            foreach (var buildingData in saveData.Buildings)
            {
                var position = new GridPosition(buildingData.X, buildingData.Y);
                var building = new Building(buildingData.Id, buildingData.Type, position, buildingData.Level);
                _grid.PlaceBuilding(building);
            }

            _loadedPublisher.Publish(new GameLoadedEvent());
        }

        public void Dispose()
        {
            StopAutoSave();
        }

        [Serializable]
        private class SaveData
        {
            public int Gold;
            public List<BuildingSaveData> Buildings;
        }

        [Serializable]
        private class BuildingSaveData
        {
            public string Id;
            public BuildingType Type;
            public int X;
            public int Y;
            public int Level;
        }
    }
}