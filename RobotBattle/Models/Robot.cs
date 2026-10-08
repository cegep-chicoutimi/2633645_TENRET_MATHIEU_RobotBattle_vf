namespace RobotBattle.Models
{
    internal class Robot
    {
        private const int PowerfulAttackEnergyCost = 2;
        private const int RechargeAmount = 1;

        private bool isHost;

        public bool IsHost
        {
            get { return isHost; }
            set { isHost = value; }
        }

        private bool isRecharging;

        public bool IsRecharging
        {
            get { return isRecharging; }
            set { isRecharging = value; }
        }

        private bool isDefending;

        public bool IsDefending
        {
            get { return isDefending; }
            set { isDefending = value; }
        }

        private List<Stats> stats;

        public IReadOnlyList<Stats> Stats
        {
            get { return stats; }
        }

        private Config config;

        public Config Config
        {
            get { return config; }
        }

        public Robot(bool isHost, List<Stats> stats, Config config)
        {
            IsHost = isHost;
            this.stats = stats;
            this.config = config;
        }

        private Stats GetStat(StatsType type)
        {
            return stats.First(s => s.Type == type);
        }

        public int Attack()
        {
            IsRecharging = false;
            return GetStat(StatsType.DAMAGE).CurrentValue;
        }

        public int AttackWithPower()
        {
            Stats energy = GetStat(StatsType.ENERGY);

            if (energy.CurrentValue < PowerfulAttackEnergyCost)
            {
                throw new InvalidOperationException("Énergie insuffisante pour une attaque puissante !");
            }

            energy.CurrentValue -= PowerfulAttackEnergyCost;
            IsRecharging = false;

            return GetStat(StatsType.DAMAGE).CurrentValue * 2;
        }

        public void Recharge()
        {
            Stats energy = GetStat(StatsType.ENERGY);
            energy.CurrentValue = Math.Min(config.MaxEnergy, energy.CurrentValue + RechargeAmount);
            IsRecharging = true;
        }

        public void Defend()
        {
            IsDefending = true;
            IsRecharging = false;
        }

        public int ReceiveDamage(int damage)
        {
            int armor = GetStat(StatsType.ARMOR).CurrentValue;
            int totalProtection = armor + (IsDefending ? config.DefenseBonus : 0);
            int effectiveDamage = Math.Max(0, damage - totalProtection);

            Stats hp = GetStat(StatsType.HP);
            hp.CurrentValue = Math.Max(0, hp.CurrentValue - effectiveDamage);

            IsDefending = false;
            return effectiveDamage;
        }

        public int GetStatValue(StatsType type)
        {
            return GetStat(type).CurrentValue;
        }

        public string GetStats(StatsType type)
        {
            return GetStat(type).CurrentValue.ToString();
        }
    }
}
