using System;
using System.Collections.Generic;
using System.Text;

namespace RobotBattle.Models
{
    internal class Randomize
    {
        private readonly Random random = new Random();

        public bool HasCompleted(int successPercent)
        {
            return random.Next(100) < successPercent;
        }

        public bool HasCompleteRandom(int hpBase, int hpNow, int minSuccessPercent, int maxSuccessPercent)
        {
            if (hpBase <= 0)
            {
                return false;
            }

            int chance = 100 - hpNow * 100 / hpBase;

            chance = Math.Clamp(chance, minSuccessPercent, maxSuccessPercent);

            return HasCompleted(chance);
        }

        public bool RandomEscape(int luckToEscape)
        {
            return HasCompleted(luckToEscape);
        }

        public bool RandomDefenseRecharge(int defenseBonusPercent)
        {
            return HasCompleted(defenseBonusPercent);
        }

        public bool RandomAttack()
        {
            return HasCompleted(100);
        }

        public bool RandomPowerfulAttack()
        {
            return HasCompleted(100);
        }

        public bool RandomDefense()
        {
            return HasCompleted(100);
        }

        public bool RandomRepair()
        {
            return HasCompleted(100);
        }
    }
}
