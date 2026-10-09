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

        // Génération d'un tirage aléatoire à partir d'une chance de réussite
        public bool HasCompleted(int successPercent)
        {
            return random.Next(0, 100) < successPercent;
        }

        // Génération d'un tirage aléatoire à partir de la vie du joueur
        public bool HasCompleteRandom(int hpBase, int hpNow, int minSuccessPercent, int maxSuccessPercent)
        {
            if (hpBase <= 0)
            {
                return false;
            }

            // Plus la perte de PV est importante, plus la chance de réussite l'est aussi
            int lostHp = Math.Clamp(hpBase - hpNow, 0, hpBase);

            int chancePercent = config.MinSuccessPercent + (config.MaxSuccessPercent - config.MinSuccessPercent) * lostHp / hpBase;

            return random.Next(0, 100) < chancePercent;
        }

        // Tirage aléatoire pour la fuite
        public bool RandomEscape(int luckToEscapePercent)
        {
            return HasCompleted(luckToEscapePercent);
        }

        // Tirage aléatoire pour déterminer si le joueur reçoit un point d'énergie après une défense
        public bool RandomDefenseRecharge(int defenseBonusPercent)
        {
            return HasCompleted(defenseBonusPercent);
        }
    }
}
