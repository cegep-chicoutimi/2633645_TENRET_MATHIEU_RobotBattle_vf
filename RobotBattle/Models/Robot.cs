using static System.Collections.Specialized.BitVector32;

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

        private readonly Randomize randomize;

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
            randomize = new Randomize(config);
        }

        private Stats GetStat(StatsType type)
        {
            return stats.First(s => s.Type == type);
        }

        // Gestion de l'action d'attaque
        public int Attack()
        {
            IsRecharging = false;

            // Si l'action est ratée, on n'envoie aucun dégât
            if (!randomize.HasCompleteRandom(GetBaseStatValue(StatsType.HP), GetStatValue(StatsType.HP), config.MinSuccessPercent, config.MaxSuccessPercent))
            {
                return 0;
            }

            return GetStat(StatsType.DAMAGE).CurrentValue;
        }

        // Gestion de l'action d'attaque puissante
        public int AttackWithPower()
        {
            Stats energy = GetStat(StatsType.ENERGY);

            // Vérification de l'énergie nécessaire
            if (energy.CurrentValue < config.PowerDamageEnergyCost)
            {
                throw new InvalidOperationException("Énergie insuffisante pour une attaque puissante !");
            }

            energy.CurrentValue -= config.PowerDamageEnergyCost;
            IsRecharging = false;

            // Si l'acrion est ratée, on n'envoie aucun dégât
            if (!randomize.HasCompleteRandom(GetBaseStatValue(StatsType.HP), GetStatValue(StatsType.HP), config.MinSuccessPercent, config.MaxSuccessPercent))
            {
                return 0;
            }

            return GetStat(StatsType.DAMAGE).CurrentValue * config.PowerDamageMultiplier;
        }

        // Gestion de l'action de recharge (100% de chance de récupérer 1 énergie)
        public void Recharge()
        {
            // Vérification du niveua d'énergie pour ne pas dépasser la limite max
            Stats energy = GetStat(StatsType.ENERGY);
            energy.CurrentValue = Math.Min(config.MaxEnergy, energy.CurrentValue + config.RechargeEnergyGain);
            IsRecharging = true;
        }

        // Gestion de l'action de défense
        public void Defend()
        {
            // Si l'action est ratée, on ne se défend d'aucun dégât
            if (!randomize.HasCompleteRandom(GetBaseStatValue(StatsType.HP), GetStatValue(StatsType.HP), config.MinSuccessPercent, config.MaxSuccessPercent))
            {
                return;
            }

            IsDefending = true;

            // Tirage pour une gagner ou non une énergie supplémentaire
            if (randomize.RandomDefenseRecharge(config.DefenseBonusPercent))
            {
                Stats energy = GetStat(StatsType.ENERGY);

                energy.CurrentValue = Math.Min(config.MaxEnergy, energy.CurrentValue + 1);
            }
        }

        // Gestion de l'action de réparation
        public int Repair()
        {
            Stats energy = GetStat(StatsType.ENERGY);

            // Vérification de l'énergie nécessaire
            if (energy.CurrentValue < config.RepairEnergyCost)
            {
                throw new InvalidOperationException("Énergie insuffisante pour une réparation !");
            }

            energy.CurrentValue -= config.RepairEnergyCost;

            // Si l'action est ratée, on ne récupère aucun pv
            if (!randomize.HasCompleteRandom(GetBaseStatValue(StatsType.HP), GetStatValue(StatsType.HP), config.MinSuccessPercent, config.MaxSuccessPercent))
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

        // Gestion de l'action d'esquive
        public bool Dodge()
        {
            Stats energy = GetStat(StatsType.ENERGY);

            // Vérifie l'énergie nécessaire
            if (energy.CurrentValue < config.DodgeEnergyCost)
            {
                throw new InvalidOperationException("Énergie insuffisante pour esquiver !");
            }

            energy.CurrentValue -= config.DodgeEnergyCost;

            int currentHp = GetStatValue(StatsType.HP);
            int baseHp = GetBaseStatValue(StatsType.HP);

            // Après plusieurs échecs consécutifs, la prochaine esquive réussit automatiquement
            if (failStreak >= config.MaxFailStreak)
            {
                failStreak = 0;
                isDodging = true;
                return true;
            }

            isDodging = randomize.HasCompleteRandom(baseHp, currentHp, config.MinSuccessPercent, config.MaxSuccessPercent);

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

        // Gestion de fuite
        public bool Escape()
        {
            // Tirage pour savoir si la fuite est réussie
            return randomize.RandomEscape(config.LuckToEscapePercent);
        }

        // Calcul des dégâts reçus
        public int ReceiveDamage(int attack)
        {
            if (attack <= 0)
            {
                return 0;
            }

            // L'esquive réussie annule tous les dégâts
            if (isDodging)
            {
                isDodging = false;
                isDefending = false;
                return 0;
            }

            int armor = GetStatValue(StatsType.ARMOR);

            if (isDefending)
            {
                int bonus = Math.Max(config.DefenseMinBonus, armor * config.DefenseBonusPercent / 100);

                armor += bonus;
                isDefending = false;
            }

            int damage = Math.Max(1, attack - armor);

            int hp = GetStatValue(StatsType.HP);

            SetStatValue(StatsType.HP, Math.Max(0, hp - damage));

            return damage;
        }

        // Lecture de la valeur actuelle de la stat demandée
        public int GetStatValue(StatsType type)
        {
            return GetStat(type).CurrentValue;
        }

        // Lecture de la valeur de base de la stat demandée
        public int GetBaseStatValue(StatsType type)
        {
            return GetStat(type).BaseValue;
        }

        // Récupération de la valeur actuelle de la stat demandée sous forme de string
        public string GetStats(StatsType type)
        {
            return GetStat(type).CurrentValue.ToString();
        }

        // Modification de la valeur actuelle d'une stat
        public void SetStatValue(StatsType type, int value)
        {
            GetStat(type).CurrentValue = value;
        }
    }
}