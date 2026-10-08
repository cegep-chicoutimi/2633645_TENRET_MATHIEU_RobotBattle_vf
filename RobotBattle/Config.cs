namespace RobotBattle
{
    internal class Config
    {
        public int Port { get; set; }
        public string IpAddress { get; set; }
        public int MaxPlayers { get; set; }
        public int PointsToGive { get; set; }
        public int BaseEnergy { get; set; }
        public int MaxEnergy { get; set; }
        public int BaseHp { get; set; }
        public int BaseArmor { get; set; }
        public int BaseDamage { get; set; }
        public int HpPerPoint { get; set; }
        public int ArmorPerPoint { get; set; }
        public int DamagePerPoint { get; set; }
        public int DefenseBonus { get; set; }
        public int RepairPercent { get; set; }
        public int RepairMinHp { get; set; }
        public int LuckToEscape { get; set; }

        public int DefenseBonusPercent { get; set; } = 40;
        public int DefenseMinBonus { get; set; } = 5;
        public int LuckToEscapePercent { get; set; } = 15;
        public int MinSuccessPercent { get; set; } = 50;
        public int MaxSuccessPercent { get; set; } = 85;
        public int MaxFailStreak { get; set; } = 2;

        public Config(
            int port = 5000,
            string ipAddress = "0.0.0.0",
            int maxPlayers = 2,
            int pointsToGive = 10,
            int baseEnergy = 2,
            int maxEnergy = 5,
            int baseHp = 100,
            int powerDamageEnergyCost = 2,
            int powerDamageMultiplier = 2,
            int rechargeEnergyGain = 2,
            int baseArmor = 0,
            int baseDamage = 10,
            int hpPerPoint = 10,
            int armorPerPoint = 2,
            int damagePerPoint = 2,
            int defenseBonusPercent = 40,
            int defenseMinBonus = 5,
            int repairPercent = 10,
            int repairMinHp = 5,
            int luckToEscape = 15,
            int MinSuccessPercent = 60,
            int MaxSuccessPercent = 95,
            int MaxFailStreak = 2,
            int RepairEnergyCost = 1)
        {
            Port = port;
            IpAddress = ipAddress;
            MaxPlayers = maxPlayers;
            PointsToGive = pointsToGive;
            BaseEnergy = baseEnergy;
            MaxEnergy = maxEnergy;
            BaseHp = baseHp;
            BaseArmor = baseArmor;
            BaseDamage = baseDamage;
            HpPerPoint = hpPerPoint;
            ArmorPerPoint = armorPerPoint;
            DamagePerPoint = damagePerPoint;
            DefenseBonus = defenseBonus;
            RepairPercent = repairPercent;
            RepairMinHp = repairMinHp;
            LuckToEscape = luckToEscape;
        }
    }
}
