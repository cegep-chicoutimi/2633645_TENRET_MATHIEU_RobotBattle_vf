namespace RobotBattle.Models
{
    internal class RobotConfig
    {
        private int hpPoints;

        public int HpPoints
        {
            get { return hpPoints; }
            set { hpPoints = value; }
        }

        private int armorPoints;

        public int ArmorPoints
        {
            get { return armorPoints; }
            set { armorPoints = value; }
        }

        private int damagePoints;

        public int DamagePoints
        {
            get { return damagePoints; }
            set { damagePoints = value; }
        }

        public RobotConfig(int hpPoints, int armorPoints, int damagePoints)
        {
            HpPoints = hpPoints;
            ArmorPoints = armorPoints;
            DamagePoints = damagePoints;
        }

        public bool IsValid(int pointsToGive)
        {
            return HpPoints >= 0
                && ArmorPoints >= 0
                && DamagePoints >= 0
                && HpPoints + ArmorPoints + DamagePoints == pointsToGive;
        }

        public List<Stats> ToStats(Config config)
        {
            if (!IsValid(config.PointsToGive))
            {
                throw new ArgumentException("Configuration de robot invalide !");
            }

            return new List<Stats>
            {
                new Stats(StatsType.HP, config.BaseHp + HpPoints * config.HpPerPoint),
                new Stats(StatsType.ARMOR, config.BaseArmor + ArmorPoints * config.ArmorPerPoint),
                new Stats(StatsType.DAMAGE, config.BaseDamage + DamagePoints * config.DamagePerPoint),
                new Stats(StatsType.ENERGY, config.BaseEnergy)
            };
        }
    }
}
