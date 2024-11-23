using RimWorld;
using Verse;

namespace DragonBall
{
    public class TournamentFighterDef : Def
    {
        public string Name;
        public string FighterKind = "Human";
        public int MeleeSkill = 5;
        public int ShootingSkill = 5;
        public Passion MeleePassion = Passion.None;
        public float MovementCapacity = 1f;
        public float ManipulationCapacity = 1f;
        public float KiLevel = 100f;

        public bool HasKaioKen = false;
        public int KaioKenLevel = 0;

        public bool HasSuperSaiyan = false;
        public int SuperSaiyanLevel = 0;

        public bool HasSuperSaiyan2 = false;
        public int SuperSaiyan2Level = 0;

        public bool HasTrueSuperSaiyan = false;
        public int TrueSuperSaiyanLevel = 0;

        public bool IsLegendary = false;

        public float ScoreBonus = 0;
    }
}
