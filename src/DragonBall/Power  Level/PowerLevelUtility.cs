using SaiyanMod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public static class PowerLevelUtility
    {
        private static List<PowerLevelContributionDef> cachedContributionDefs;

        private static List<PowerLevelContributionDef> GetAllContributionDefs()
        {
            if (cachedContributionDefs == null)
            {
                cachedContributionDefs = DefDatabase<PowerLevelContributionDef>.AllDefs.ToList();
            }
            return cachedContributionDefs;
        }

        public static float GetCurrentPowerLevel(Pawn pawn)
        {
            if (!pawn.TryGetKiAbilityClass(out AbilityClassKI kiClass))
                kiClass = null;

            float basePower = 1f;
            float totalMultiplier = 1f;

            foreach (var contribution in GetAllContributionDefs())
            {
                float value = contribution.Calculate(pawn, kiClass);
                if (contribution.isMultiplier)
                    totalMultiplier *= value;
                else
                    basePower += value;
            }

            return Math.Max(1f, basePower * totalMultiplier);
        }

        public static Dictionary<string, float> GetDetailedPowerBreakdown(Pawn pawn)
        {
            if (!pawn.TryGetKiAbilityClass(out AbilityClassKI kiClass))
                return new Dictionary<string, float> { { "Error", 0f } };

            var breakdown = new Dictionary<string, float>();

            // Add each contribution to the breakdown
            foreach (var contribution in GetAllContributionDefs())
            {
                float value = contribution.Calculate(pawn, kiClass);
                if (contribution.isMultiplier)
                    breakdown[$"{contribution.label} Multiplier"] = value;
                else
                    breakdown[contribution.label] = value;
            }

            breakdown["Total Power Level"] = GetCurrentPowerLevel(pawn);
            return breakdown;
        }

        public static string GetPowerLevelDisplay(float powerLevel)
        {
            if (powerLevel < 1000)
                return powerLevel.ToString("F0");
            if (powerLevel < 1000000)
                return (powerLevel / 1000).ToString("F1") + "K";
            return (powerLevel / 1000000).ToString("F1") + "M";
        }
    }
}
