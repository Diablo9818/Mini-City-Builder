using System.Collections.Generic;
using CityBuilder.Application.Services;
using CityBuilder.Domain.Models;
using UnityEngine;

namespace CityBuilder.Infrastructure.Services
{
    [CreateAssetMenu(fileName = "BuildingConfigProvider", menuName = "CityBuilder/BuildingConfigProvider")]
    public class BuildingConfigProvider : ScriptableObject, IBuildingConfigProvider
    {
        [SerializeField] private BuildingConfigData[] _configs;

        private Dictionary<BuildingType, BuildingConfig> _configMap;

        private void OnEnable()
        {
            InitializeConfigs();
        }

        private void InitializeConfigs()
        {
            _configMap = new Dictionary<BuildingType, BuildingConfig>();
            
            foreach (var data in _configs)
            {
                var config = new BuildingConfig(
                    data.Type,
                    new Cost(data.BaseCost),
                    new Income(data.BaseIncome),
                    data.MaxLevel,
                    data.UpgradeCostMultiplier,
                    data.UpgradeIncomeMultiplier
                );
                _configMap[data.Type] = config;
            }
        }

        public BuildingConfig GetConfig(BuildingType type)
        {
            if (_configMap == null)
            {
                InitializeConfigs();
            }
            
            return _configMap.TryGetValue(type, out var config) ? config : null;
        }

        [System.Serializable]
        public class BuildingConfigData
        {
            public BuildingType Type;
            public int BaseCost = 100;
            public int BaseIncome = 10;
            public int MaxLevel = 3;
            public float UpgradeCostMultiplier = 1.5f;
            public float UpgradeIncomeMultiplier = 2f;
        }
    }
}