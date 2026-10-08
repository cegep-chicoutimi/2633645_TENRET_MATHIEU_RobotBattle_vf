namespace RobotBattle.Models
{
    internal class Robot
    {
        private const int PowerfulAttackEnergyCost = 2;
        private const int RechargeAmount = 2;
        private const int RepairEnergyCost = 1;
        private int failStreak = 0;

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

        private bool isDefending = false;

        public bool IsDefending
        {
            get { return isDefending; }
            set { isDefending = value; }
        }

        private bool isDodging = false;

        public bool IsDodging
        {
            get { return isDodging; }
            set { isDodging = value; }
        }

        private readonly Randomize randomize = new Randomize();

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

            if (!randomize.RandomAttack())
            {
                return 0;
            }

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

            if (!randomize.RandomPowerfulAttack())
            {
                return 0;
            }

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
            if (!randomize.RandomDefense())
            {
                return;
            }

            IsDefending = true;

            if (randomize.RandomDefenseRecharge(config.DefenseBonusPercent))
            {
                Stats energy = GetStat(StatsType.ENERGY);

                energy.CurrentValue = Math.Min(config.MaxEnergy, energy.CurrentValue + 1);
            }
        }

        public int Repair()
        {
            Stats energy = GetStat(StatsType.ENERGY);

            if (energy.CurrentValue < RepairEnergyCost)
            {
                throw new InvalidOperationException("INVALID_ACTION");
            }

            energy.CurrentValue -= RepairEnergyCost;

            if (!randomize.RandomRepair())
            {
                return 0;
            }

            int currentHp = GetStatValue(StatsType.HP);
            int baseHp = GetBaseStatValue(StatsType.HP);

            int repairedHp = Math.Max(config.RepairMinHp, baseHp * config.RepairPercent / 100);

            int newHp = Math.Min(currentHp + repairedHp, baseHp);

            SetStatValue(StatsType.HP, newHp);

            return newHp - currentHp;
        }

        public bool Dodge()
        {
            int currentHp = GetStatValue(StatsType.HP);
            int baseHp = GetBaseStatValue(StatsType.HP);

            if (failStreak >= config.MaxFailStreak)
            {
                failStreak = 0;
                isDodging = true;
                return true;
            }

            isDodging = randomize.HasCompleteRandom(baseHp, currentHp, config.MinSuccessPercent, config.MaxSuccessPercent
            );

            if (isDodging)
            {
                failStreak = 0;
            }
            else
            {
                failStreak++;
            }

            return isDodging;
        }

        public bool Escape()
        {
            return randomize.RandomEscape(config.LuckToEscapePercent);
        }

        //public int ReceiveDamage(int damage)
        //{
        //    int armor = GetStat(StatsType.ARMOR).CurrentValue;
        //    int totalProtection = armor + (IsDefending ? config.DefenseBonus : 0);
        //    int effectiveDamage = Math.Max(0, damage - totalProtection);

        //    Stats hp = GetStat(StatsType.HP);
        //    hp.CurrentValue = Math.Max(0, hp.CurrentValue - effectiveDamage);

        //    IsDefending = false;
        //    return effectiveDamage;
        //}

        public int ReceiveDamage(int attack)
        {
            if (isDodging)
            {
                isDodging = false;
                isDefending = false;
                return 0;
            }

            int armor = GetStatValue(StatsType.ARMOR);

            if (isDefending)
            {
                int bonus = Math.Max(
                    config.DefenseMinBonus,
                    armor * config.DefenseBonusPercent / 100
                );

                armor += bonus;
                isDefending = false;
            }

            int damage = Math.Max(1, attack - armor);

            int hp = GetStatValue(StatsType.HP);

            SetStatValue(StatsType.HP, Math.Max(0, hp - damage));

            return damage;
        }

        public int GetStatValue(StatsType type)
        {
            return GetStat(type).CurrentValue;
        }

        public int GetBaseStatValue(StatsType type)
        {
            return GetStat(type).BaseValue;
        }

        public string GetStats(StatsType type)
        {
            return GetStat(type).CurrentValue.ToString();
        }

        public void SetStatValue(StatsType type, int value)
        {
            GetStat(type).CurrentValue = value;
        }
    }
}
