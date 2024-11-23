using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using static DragonBall.FighterData;

namespace DragonBall
{
    public static class OpponentGenerator
    {
        // Constants for better balance control
        private const float BASE_SCORE_MULTIPLIER = 0.8f;
        private const float MAX_SCORE_MULTIPLIER = 1.2f;
        private const float LEGENDARY_CHANCE_PER_DIFFICULTY = 0.01f;

        public static FighterData GenerateOpponent(float targetScore, float difficultyLevel = 100f, float difficultyVariance = 0.2f)
        {
            // Scale target score based on difficulty level
            float adjustedTargetScore = ScaleScoreByDifficulty(targetScore, difficultyLevel);

            // Try to use predefined fighter first
            if (ShouldUsePredefinedFighter(difficultyLevel))
            {
                return GeneratePredefinedFighter(adjustedTargetScore, difficultyVariance);
            }

            // Generate custom fighter based on target score
            return GenerateCustomFighter(adjustedTargetScore, difficultyLevel);
        }

        private static float ScaleScoreByDifficulty(float baseScore, float difficultyLevel)
        {
            // Scale between 80% and 120% of base score based on difficulty
            float difficultyFactor = BASE_SCORE_MULTIPLIER +
                ((difficultyLevel / 100f) * (MAX_SCORE_MULTIPLIER - BASE_SCORE_MULTIPLIER));

            return baseScore * difficultyFactor;
        }

        private static bool ShouldUsePredefinedFighter(float difficultyLevel)
        {
            List<TournamentFighterDef> fighters = DefDatabase<TournamentFighterDef>.AllDefsListForReading;
            return fighters.Count > 0 && Rand.Value < (0.3f * (difficultyLevel / 100f));
        }

        private static FighterData GeneratePredefinedFighter(float targetScore, float difficultyVariance)
        {
            TournamentFighterDef fighter = DefDatabase<TournamentFighterDef>.AllDefsListForReading.RandomElement();
            return new FighterData
            {
                Name = fighter.Name,
                MeleeSkill = fighter.MeleeSkill,
                ShootingSkill = fighter.ShootingSkill,
                MeleePassion = fighter.MeleePassion,
                MovementCapacity = fighter.MovementCapacity,
                ManipulationCapacity = fighter.ManipulationCapacity,
                KiLevel = ScaleKiLevelToTarget(fighter.KiLevel, targetScore, difficultyVariance),
                HasSuperSaiyan = fighter.HasSuperSaiyan,
                SuperSaiyanLevel = fighter.SuperSaiyanLevel,
                HasSuperSaiyan2 = fighter.HasSuperSaiyan2,
                SuperSaiyan2Level = fighter.SuperSaiyan2Level,
                HasKaioKen = fighter.HasKaioKen,
                KaioKenLevel = fighter.KaioKenLevel,
                IsPawn = false
            };
        }

        private static FighterData GenerateCustomFighter(float targetScore, float difficultyLevel)
        {
            // Calculate base stats that would result in roughly the target score
            float baseKiLevel = CalculateBaseKiLevel(targetScore);

            FighterData fighter = new FighterData
            {
                Name = GenerateRandomName(),
                MeleeSkill = GenerateSkillLevel(difficultyLevel),
                ShootingSkill = GenerateSkillLevel(difficultyLevel * 0.8f), // Slightly lower for shooting
                MeleePassion = GeneratePassion(difficultyLevel),
                MovementCapacity = 0.8f + (difficultyLevel / 500f), // Scales from 0.8 to 1.0
                ManipulationCapacity = 0.8f + (difficultyLevel / 500f),
                KiLevel = baseKiLevel,
                IsPawn = false
            };

            // Add transformations based on target score and difficulty
            AssignTransformations(fighter, targetScore, difficultyLevel);

            return fighter;
        }

        private static float CalculateBaseKiLevel(float targetScore)
        {
            // Base Ki level should account for potential transformation boosts
            return targetScore / (15f * 2f); // Divided by 2 to account for potential power-ups
        }

        private static float GenerateSkillLevel(float difficultyLevel)
        {
            float baseSkill = 2f + (difficultyLevel / 10f);
            return Mathf.Clamp(baseSkill + Rand.Range(-2f, 2f), 2f, 20f);
        }

        private static Passion GeneratePassion(float difficultyLevel)
        {
            float chance = difficultyLevel / 200f; // 50% chance at difficulty 100
            if (Rand.Value < chance)
                return Rand.Value < 0.3f ? Passion.Major : Passion.Minor;
            return Passion.None;
        }

        private static void AssignTransformations(FighterData fighter, float targetScore, float difficultyLevel)
        {
            // Legendary status (rare but more common at high difficulties)
            fighter.IsLegendarySaiyan = Rand.Value < (LEGENDARY_CHANCE_PER_DIFFICULTY * difficultyLevel);

            // Scale transformation chances with difficulty
            float transformChance = difficultyLevel / 200f; // 50% at difficulty 100

            // Kaio-Ken (most common)
            if (Rand.Value < transformChance * 1.5f)
            {
                fighter.HasKaioKen = true;
                fighter.KaioKenLevel = Rand.Range(1f, 1f + (difficultyLevel / 50f));
            }

            // Super Saiyan (moderate rarity)
            if (Rand.Value < transformChance)
            {
                fighter.HasSuperSaiyan = true;
                fighter.SuperSaiyanLevel = Rand.Range(1f, 1f + (difficultyLevel / 75f));
            }

            // Super Saiyan 2 (rare)
            if (fighter.HasSuperSaiyan && Rand.Value < (transformChance * 0.3f))
            {
                fighter.HasSuperSaiyan2 = true;
                fighter.SuperSaiyan2Level = Rand.Range(1f, 1f + (difficultyLevel / 100f));
            }

            // True Super Saiyan (very rare)
            if (fighter.HasSuperSaiyan2 && Rand.Value < (transformChance * 0.1f))
            {
                fighter.HasTrueSuperSaiyan = true;
                fighter.TrueSuperSaiyanLevel = Rand.Range(1f, 1f + (difficultyLevel / 150f));
            }
        }

        private static float ScaleKiLevelToTarget(float baseKiLevel, float targetScore, float variance)
        {
            float scaleFactor = targetScore / (baseKiLevel * 15f);
            return baseKiLevel * scaleFactor * Rand.Range(1f - variance, 1f + variance);
        }

        private static string GenerateRandomName()
        {
            return NameGenerator.GenerateName(
                DBDefOf.NamerFighterDragonBall,
                new List<string>(),
                false
            );
        }
    }

}
