using RimWorld;
using SaiyanMod;
using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public partial class FighterData : IExposable
    {
        public string Name;
        public bool IsPawn;
        public string FighterKind;
        public float MeleeSkill;
        public float ShootingSkill;
        public Passion MeleePassion;
        public float MovementCapacity;
        public float ManipulationCapacity;
        public float KiLevel;
        public bool HasSuperSaiyan;
        public float SuperSaiyanLevel;
        public bool HasSuperSaiyan2;
        public float SuperSaiyan2Level;
        public bool HasTrueSuperSaiyan;
        public float TrueSuperSaiyanLevel;
        public bool IsLegendarySaiyan = false;
        public bool HasKaioKen;
        public float KaioKenLevel;
        public float DifficultyLevel = 100;

        public void ExposeData()
        {
            Scribe_Values.Look(ref Name, "Name");
            Scribe_Values.Look(ref FighterKind, "FighterKind");
            Scribe_Values.Look(ref MeleeSkill, "MeleeSkill");
            Scribe_Values.Look(ref ShootingSkill, "ShootingSkill");
            Scribe_Values.Look(ref MeleePassion, "MeleePassion");
            Scribe_Values.Look(ref MovementCapacity, "MovementCapacity");
            Scribe_Values.Look(ref ManipulationCapacity, "ManipulationCapacity");
            Scribe_Values.Look(ref KiLevel, "KiLevel");
            Scribe_Values.Look(ref IsPawn, "isPawn");
            Scribe_Values.Look(ref IsLegendarySaiyan, "IsLegendarySaiyan");
            Scribe_Values.Look(ref HasSuperSaiyan, "HasSuperSaiyan");
            Scribe_Values.Look(ref SuperSaiyanLevel, "SuperSaiyanLevel");
            Scribe_Values.Look(ref HasSuperSaiyan2, "HasSuperSaiyan2");
            Scribe_Values.Look(ref SuperSaiyan2Level, "SuperSaiyan2Level");
            Scribe_Values.Look(ref TrueSuperSaiyanLevel, "TrueSuperSaiyanLevel");
            Scribe_Values.Look(ref HasTrueSuperSaiyan, "HasTrueSuperSaiyan");
            Scribe_Values.Look(ref HasKaioKen, "HasKaioKen");
            Scribe_Values.Look(ref KaioKenLevel, "KaioKenLevel");
        }

        public Dictionary<string, float> CalculateScore()
        {
            var scoreContributions = new Dictionary<string, float>();
            float totalScore = 0f;

            // Melee skill contribution
            float meleeScore = MeleeSkill * 2f;
            totalScore += meleeScore;
            scoreContributions["Melee Skill"] = meleeScore;
            float meleePassionScore = 0f;

            switch (MeleePassion)
            {
                case Passion.None :
                    meleePassionScore = 0f;
                    break;
                case Passion.Minor:
                    meleePassionScore = 25f;
                    break;
                case Passion.Major:
                    meleePassionScore = 50f;
                    break;
            }

            totalScore += meleePassionScore;
            scoreContributions["Melee Passion"] = meleePassionScore;

            // Shooting contribution
            float shootingScore = ShootingSkill * 1.5f;
            totalScore += shootingScore;
            scoreContributions["Shooting Skill"] = shootingScore;

            // Movement contribution
            float movementScore = MovementCapacity * 10f;
            totalScore += movementScore;
            scoreContributions["Movement"] = movementScore;

            // Manipulation contribution
            float manipulationScore = ManipulationCapacity * 8f;
            totalScore += manipulationScore;
            scoreContributions["Manipulation"] = manipulationScore;

            // Ki contribution
            float kiScore = KiLevel * 15f;
            totalScore += kiScore;
            scoreContributions["Ki Level"] = kiScore;

            // Super Saiyan contributions
            if (HasSuperSaiyan)
            {
                float ssScore = 130f + SuperSaiyanLevel;
                totalScore += ssScore;
                scoreContributions["Super Saiyan"] = ssScore;
            }

            if (HasSuperSaiyan2)
            {
                float ss2Score = 160f + SuperSaiyan2Level;
                totalScore += ss2Score;
                scoreContributions["Super Saiyan 2"] = ss2Score;
            }

            if (HasTrueSuperSaiyan)
            {
                float ss2Score = 360f + TrueSuperSaiyanLevel;
                totalScore += ss2Score;
                scoreContributions["True Super Saiyan"] = ss2Score;
            }

            if (HasKaioKen)
            {
                float kaiokenScore = 60f + KaioKenLevel;
                totalScore += kaiokenScore;
                scoreContributions["Kaio-Ken"] = kaiokenScore;
            }

            if (IsLegendarySaiyan)
            {
                float LegendarySaiyaScore = 150f;
                totalScore += LegendarySaiyaScore;
                scoreContributions["Legendary-Saiyan"] = LegendarySaiyaScore;
            }

            scoreContributions["Total Score"] = totalScore;
            return scoreContributions;
        }

        public static FighterData FromPawn(Pawn pawn)
        {
            if (!pawn.TryGetKiAbilityClass(out AbilityClassKI abilityClassKI))
                return null;

            return new FighterData
            {
                Name = pawn.LabelShort,
                FighterKind = pawn.kindDef.defName,
                MeleeSkill = pawn.skills.GetSkill(SkillDefOf.Melee).Level,
                ShootingSkill = pawn.skills.GetSkill(SkillDefOf.Shooting).Level,
                MeleePassion = pawn.skills.GetSkill(SkillDefOf.Melee).passion,
                MovementCapacity = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving),
                ManipulationCapacity = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation),
                KiLevel = abilityClassKI.abilityResource.MaxEnergy,
                HasSuperSaiyan = abilityClassKI.Learned(SR_DefOf.SR_SuperSaiyan),
                SuperSaiyanLevel = abilityClassKI.Learned(SR_DefOf.SR_SuperSaiyan) ?
                    abilityClassKI.GetLearnedAbility(SR_DefOf.SR_SuperSaiyan).LevelHumanReadable : 0,
                HasSuperSaiyan2 = abilityClassKI.Learned(DBDefOf.SR_SuperSaiyan2),
                SuperSaiyan2Level = abilityClassKI.Learned(DBDefOf.SR_SuperSaiyan2) ?
                    abilityClassKI.GetLearnedAbility(DBDefOf.SR_SuperSaiyan2).LevelHumanReadable : 0,
                HasTrueSuperSaiyan = abilityClassKI.Learned(SR_DefOf.SR_TrueSuperSaiyan),
                TrueSuperSaiyanLevel = abilityClassKI.Learned(SR_DefOf.SR_TrueSuperSaiyan) ?
                    abilityClassKI.GetLearnedAbility(SR_DefOf.SR_TrueSuperSaiyan).LevelHumanReadable : 0,
                HasKaioKen = abilityClassKI.Learned(DBDefOf.SR_KaioKen),
                KaioKenLevel = abilityClassKI.Learned(DBDefOf.SR_KaioKen) ?
                    abilityClassKI.GetLearnedAbility(DBDefOf.SR_KaioKen).LevelHumanReadable : 0
            };
        }
    }
}
