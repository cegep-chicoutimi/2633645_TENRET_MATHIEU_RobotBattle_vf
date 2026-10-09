using System;
using System.Collections.Generic;
using System.Text;

namespace RobotBattle.Models
{
    internal class Randomize
    {
        private readonly Random random = new Random();
        private readonly Config config;

        public Randomize(Config config)
        {
            this.config = config;
        }

        public bool HasCompleted(int successPercent)
        {
            return random.Next(0, 100) < successPercent;
        }

        public bool HasCompleteRandom(int hpBase, int hpNow, int minSuccessPercent, int maxSuccessPercent)
        {
            if (hpBase <= 0)
            {
                return false;
            }

            int lostHp = Math.Clamp(hpBase - hpNow, 0, hpBase);

            int chancePercent = config.MinSuccessPercent + (config.MaxSuccessPercent - config.MinSuccessPercent) * lostHp / hpBase;

            return random.Next(0, 100) < chancePercent;
        }

        public bool RandomEscape(int luckToEscapePercent)
        {
            return HasCompleted(luckToEscapePercent);
        }

        public bool RandomDefenseRecharge(int defenseBonusPercent)
        {
            return HasCompleted(defenseBonusPercent);
        }

        //public bool RandomAttack()
        //{
        //    return HasCompleted(100);
        //}

        //public bool RandomPowerfulAttack()
        //{
        //    return HasCompleted(100);
        //}

        //public bool RandomDefense()
        //{
        //    return HasCompleted(100);
        //}

        //public bool RandomRepair()
        //{
        //    return HasCompleted(100);
        //}
    }
}
