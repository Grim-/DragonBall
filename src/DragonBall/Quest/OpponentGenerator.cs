using RimWorld;
using System.Collections.Generic;
using Verse;
using static DragonBall.FighterData;

namespace DragonBall
{
    public static class OpponentGenerator
    {
        public static FighterData GenerateOpponent(float targetScore, float difficultyVariance = 0.2f)
        {
            List<TournamentFighterDef> fighters = DefDatabase<TournamentFighterDef>.AllDefsListForReading;

            if (Rand.Value < 0.3f && fighters.Count > 0)
            {
                TournamentFighterDef fighter = fighters.RandomElement();
                return new FighterData
                {
                    Name = fighter.Name,
                    MeleeSkill = fighter.MeleeSkill,
                    ShootingSkill = fighter.ShootingSkill,
                    MeleePassion = fighter.MeleePassion,
                    MovementCapacity = fighter.MovementCapacity,
                    ManipulationCapacity = fighter.ManipulationCapacity,
                    KiLevel = fighter.KiLevel * Rand.Range(1f - difficultyVariance, 1f + difficultyVariance),
                    HasSuperSaiyan = fighter.HasSuperSaiyan,
                    SuperSaiyanLevel = fighter.SuperSaiyanLevel,
                    HasSuperSaiyan2 = fighter.HasSuperSaiyan2,
                    SuperSaiyan2Level = fighter.SuperSaiyan2Level,
                    HasKaioKen = fighter.HasKaioKen,
                    KaioKenLevel = fighter.KaioKenLevel,
                    IsPawn = false
                };
            }

            float targetKiLevel = targetScore / 15f;
            return new FighterData
            {
                Name = GenerateRandomName(),
                MeleeSkill = Rand.Range(2f, 20f),
                ShootingSkill = Rand.Range(2f, 16f),
                MeleePassion = (Passion)Rand.Range(0, 3),
                MovementCapacity = Rand.Range(0.8f, 1f),
                ManipulationCapacity = Rand.Range(0.8f, 1f),
                KiLevel = targetKiLevel * Rand.Range(0.2f, 0.5f),
                HasSuperSaiyan = Rand.Value < 0.15f,
                SuperSaiyanLevel = Rand.Range(1f, 3f),
                HasSuperSaiyan2 = Rand.Value < 0.005f,
                SuperSaiyan2Level = Rand.Range(1f, 3f),
                HasKaioKen = Rand.Value < 0.2f,
                KaioKenLevel = Rand.Range(1f, 3f),
                IsPawn = false
            };
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
