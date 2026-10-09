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
        public int PowerDamageEnergyCost { get; set; }
        public int PowerDamageMultiplier { get; set; }
        public int RechargeEnergyGain { get; set; }
        public int HpPerPoint { get; set; }
        public int ArmorPerPoint { get; set; }
        public int DamagePerPoint { get; set; }
        public int DefenseBonusPercent { get; set; }
        public int DefenseMinBonus { get; set; }
        public int RepairPercent { get; set; }
        public int RepairMinHp { get; set; }
        public int LuckToEscapePercent { get; set; }
        public int MinSuccessPercent { get; set; }
        public int MaxSuccessPercent { get; set; }
        public int MaxFailStreak { get; set; }
        public int RepairEnergyCost { get; set; }
        public int DodgeEnergyCost { get; set; }

        public Config(
            int port = 5000,
            string ipAddress = "0.0.0.0",
            int maxPlayers = 2,
            int pointsToGive = 10,
            int baseEnergy = 2,
            int maxEnergy = 5,
            int baseHp = 100,
            int baseArmor = 0,
            int baseDamage = 10,
            int powerDamageEnergyCost = 2,
            int powerDamageMultiplier = 2,
            int rechargeEnergyGain = 2,
            int hpPerPoint = 10,
            int armorPerPoint = 2,
            int damagePerPoint = 2,
            int defenseBonusPercent = 40,
            int defenseMinBonus = 5,
            int repairPercent = 10,
            int repairMinHp = 5,
            int luckToEscapePercent = 15,
            int minSuccessPercent = 60,
            int maxSuccessPercent = 95,
            int maxFailStreak = 2,
            int repairEnergyCost = 1,
            int dodgeEnergyCost = 1)
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
            PowerDamageEnergyCost = powerDamageEnergyCost;
            PowerDamageMultiplier = powerDamageMultiplier;
            RechargeEnergyGain = rechargeEnergyGain;
            HpPerPoint = hpPerPoint;
            ArmorPerPoint = armorPerPoint;
            DamagePerPoint = damagePerPoint;
            DefenseBonusPercent = defenseBonusPercent;
            DefenseMinBonus = defenseMinBonus;
            RepairPercent = repairPercent;
            RepairMinHp = repairMinHp;
            LuckToEscapePercent = luckToEscapePercent;
            MinSuccessPercent = minSuccessPercent;
            MaxSuccessPercent = maxSuccessPercent;
            MaxFailStreak = maxFailStreak;
            RepairEnergyCost = repairEnergyCost;
            DodgeEnergyCost = dodgeEnergyCost;
        }
    }
}
