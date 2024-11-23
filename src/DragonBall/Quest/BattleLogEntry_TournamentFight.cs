using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public class BattleLogEntry_TournamentFight : LogEntry
    {
        public string winnerName;
        public string loserName;
        public bool hasKaioKen;
        public bool hasSuperSaiyan;
        public bool hasSuperSaiyan2;
        public bool hasTrueSuperSaiyan;
        public bool isLegendarySaiyan;
        public bool closeMatch;
        public bool onesided;
        public bool highKi;
        public bool multiTransform;

        public BattleLogEntry_TournamentFight() { } // For saving/loading

        public BattleLogEntry_TournamentFight(
            string winner,
            string loser,
            FighterData winnerData,
            FighterData loserData,
            LogEntryDef def = null) : base(def ?? DefDatabase<LogEntryDef>.GetNamed("TournamentFight"))
        {
            winnerName = winner;
            loserName = loser;

            // Set transformation flags
            hasKaioKen = winnerData.HasKaioKen;
            hasSuperSaiyan = winnerData.HasSuperSaiyan;
            hasSuperSaiyan2 = winnerData.HasSuperSaiyan2;
            hasTrueSuperSaiyan = winnerData.HasTrueSuperSaiyan;
            isLegendarySaiyan = winnerData.IsLegendarySaiyan;

            // Calculate match characteristics
            var winnerScore = winnerData.CalculateScore();
            var loserScore = loserData.CalculateScore();
            float winnerTotal = winnerScore["Total Score"];
            float loserTotal = loserScore["Total Score"];

            closeMatch = loserTotal / winnerTotal > 0.9f;
            onesided = loserTotal / winnerTotal < 0.6f;
            highKi = winnerData.KiLevel > 1000 || loserData.KiLevel > 1000;
            multiTransform = (winnerData.HasKaioKen ? 1 : 0) +
                            (winnerData.HasSuperSaiyan ? 1 : 0) +
                            (winnerData.HasSuperSaiyan2 ? 1 : 0) +
                            (winnerData.HasTrueSuperSaiyan ? 1 : 0) > 1;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref winnerName, "winnerName");
            Scribe_Values.Look(ref loserName, "loserName");
            Scribe_Values.Look(ref hasKaioKen, "hasKaioKen");
            Scribe_Values.Look(ref hasSuperSaiyan, "hasSuperSaiyan");
            Scribe_Values.Look(ref hasSuperSaiyan2, "hasSuperSaiyan2");
            Scribe_Values.Look(ref hasTrueSuperSaiyan, "hasTrueSuperSaiyan");
            Scribe_Values.Look(ref isLegendarySaiyan, "isLegendarySaiyan");
            Scribe_Values.Look(ref closeMatch, "closeMatch");
            Scribe_Values.Look(ref onesided, "onesided");
            Scribe_Values.Look(ref highKi, "highKi");
            Scribe_Values.Look(ref multiTransform, "multiTransform");
        }

        public override bool Concerns(Thing t)
        {
            if (t is Pawn p)
            {
                return p.Name.ToStringShort == winnerName || p.Name.ToStringShort == loserName;
            }
            return false;
        }

        public override IEnumerable<Thing> GetConcerns()
        {
            List<Thing> concerns = new List<Thing>();
            foreach (Pawn p in Find.CurrentMap?.mapPawns?.AllPawnsSpawned ?? new List<Pawn>())
            {
                if (p.Name.ToStringShort == winnerName || p.Name.ToStringShort == loserName)
                {
                    concerns.Add(p);
                }
            }
            return concerns;
        }
    }
}
