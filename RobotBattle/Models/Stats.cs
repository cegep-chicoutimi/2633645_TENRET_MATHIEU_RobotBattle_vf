namespace RobotBattle.Models
{
    internal class Stats
    {
        private StatsType type;

        public StatsType Type
        {
            get { return type; }
        }

        private int baseValue;

        public int BaseValue
        {
            get { return baseValue; }
            set { baseValue = value; }
        }

        private int currentValue;

        public int CurrentValue
        {
            get { return currentValue; }
            set { currentValue = value; }
        }

        public Stats(StatsType type, int baseValue)
        {
            this.type = type;
            BaseValue = baseValue;
            CurrentValue = baseValue;
        }
    }
}
