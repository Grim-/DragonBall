using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public class TournamentMatch : IExposable
    {
        public string MatchId;
        public FighterData Fighter1;
        public FighterData Fighter2;
        public FighterData Winner;
        public FighterData Loser;
        public int Round;
        public float Fighter1Score;
        public float Fighter2Score;
        public Dictionary<string, float> Fighter1ScoreData;
        public Dictionary<string, float> Fighter2ScoreData;
        public bool IsComplete;
        public int MatchIndex;

        public void ExposeData()
        {
            Scribe_Values.Look(ref MatchId, "matchId");
            Scribe_Deep.Look(ref Fighter1, "fighter1");
            Scribe_Deep.Look(ref Fighter2, "fighter2");
            Scribe_Deep.Look(ref Winner, "winner");
            Scribe_Deep.Look(ref Loser, "loser");
            Scribe_Values.Look(ref Round, "round");
            Scribe_Values.Look(ref Fighter1Score, "fighter1Score");
            Scribe_Values.Look(ref Fighter2Score, "fighter2Score");
            Scribe_Values.Look(ref IsComplete, "isComplete");
            Scribe_Values.Look(ref MatchIndex, "matchIndex");
            Scribe_Collections.Look(ref Fighter1ScoreData, "fighter1ScoreData", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref Fighter2ScoreData, "fighter2ScoreData", LookMode.Value, LookMode.Value);
        }
    }
}
