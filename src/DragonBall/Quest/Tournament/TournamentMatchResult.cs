using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DragonBall
{
    public class TournamentMatchResult : IExposable
    {
        public string MatchID;
        public bool Victory;
        public float ScoreMargin;
        public string OpponentName;
        public float ExperienceGained;
        public int Round;
        public int TournamentID; 
        public Dictionary<string, float> ScoreContributions = new Dictionary<string, float>();
        public Dictionary<string, float> OpponentScoreContributions = new Dictionary<string, float>();
        public float TotalScore => ScoreContributions.Sum(x => x.Value);
        public float OpponentTotalScore => OpponentScoreContributions.Sum(x => x.Value);

        public void ExposeData()
        {
            Scribe_Values.Look(ref Victory, "isVictory", false);
            Scribe_Values.Look(ref MatchID, "MatchID");
            Scribe_Values.Look(ref ScoreMargin, "ScoreMargin");
            Scribe_Values.Look(ref OpponentName, "OpponentName", "Name");
            Scribe_Values.Look(ref ExperienceGained, "ExperienceGained");
            Scribe_Values.Look(ref Round, "Round");
            Scribe_Values.Look(ref TournamentID, "TournamentID");
            Scribe_Collections.Look(ref ScoreContributions, "ScoreContributions", LookMode.Value, LookMode.Value);
            Scribe_Collections.Look(ref OpponentScoreContributions, "OpponentScoreContributions", LookMode.Value, LookMode.Value);
        }
    }
}
