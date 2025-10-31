namespace CityBuilder.Domain.Models
{
    public class BuildingConfig
    {
        public BuildingType Type { get; }
        public Cost BaseCost { get; }
        public Income BaseIncome { get; }
        public int MaxLevel { get; }
        public float UpgradeCostMultiplier { get; }
        public float UpgradeIncomeMultiplier { get; }

        public BuildingConfig(
            BuildingType type,
            Cost baseCost,
            Income baseIncome,
            int maxLevel = 3,
            float upgradeCostMultiplier = 1.5f,
            float upgradeIncomeMultiplier = 2f)
        {
            Type = type;
            BaseCost = baseCost;
            BaseIncome = baseIncome;
            MaxLevel = maxLevel;
            UpgradeCostMultiplier = upgradeCostMultiplier;
            UpgradeIncomeMultiplier = upgradeIncomeMultiplier;
        }

        public Cost GetCostForLevel(int level)
        {
            if (level <= 1) return BaseCost;
            
            var multiplier = 1f;
            for (var i = 1; i < level; i++)
            {
                multiplier *= UpgradeCostMultiplier;
            }
            
            return new Cost((int)(BaseCost.Gold * multiplier));
        }

        public Income GetIncomeForLevel(int level)
        {
            if (level <= 1) return BaseIncome;
            
            var multiplier = 1f;
            for (var i = 1; i < level; i++)
            {
                multiplier *= UpgradeIncomeMultiplier;
            }
            
            return new Income((int)(BaseIncome.GoldPerTick * multiplier));
        }
    }
}