using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DragonBall
{
    public class TournamentTracker : GameComponent
    {
        private Dictionary<string, TournamentHistoryEntry> fighterHistory = new Dictionary<string, TournamentHistoryEntry>();
        private int totalTournaments = 0;
        private int totalDragonBallsWon = 0;
        private int totalGoldWon = 0;

        public TournamentTracker(Game game)
        {
        }

        public void RecordTournamentResults(Pawn fighter, List<TournamentMatchResult> results, int dragonBallsWon, int goldWon)
        {
            TournamentHistoryEntry entry = null;
            if (!HasHistoricalEntry(fighter))
            {
                entry = CreateNewTournamentHistoryEntry(fighter);
            }
            else
            {
                entry = fighterHistory[fighter.ThingID];
            }

            // Create a tournament entry first
            var tournament = new Tournament
            {
                TournamentID = totalTournaments,
                Matches = results.ToList()
            };

            // Update fighter stats
            entry.totalFights += results.Count;
            entry.victories += results.Count(r => r.Victory);
            entry.totalExperience += results.Sum(r => r.ExperienceGained);
            entry.dragonBallsWon += dragonBallsWon;
            entry.goldWon += goldWon;
            entry.tournaments.Add(tournament);  // Add the whole tournament instead of individual matches

            // Update global stats
            totalTournaments++;
            totalDragonBallsWon += dragonBallsWon;
            totalGoldWon += goldWon;
        }

        private TournamentHistoryEntry CreateNewTournamentHistoryEntry(Pawn fighter)
        {
            if (!fighterHistory.ContainsKey(fighter.ThingID))
            {
                TournamentHistoryEntry entry = new TournamentHistoryEntry
                {
                    fighterName = fighter.Label,
                    tournaments = new List<Tournament>()
                };
                fighterHistory.Add(fighter.ThingID, entry);
                return entry;
            }
            return null;
        }

        private bool HasHistoricalEntry(Pawn fighter)
        {
            return fighterHistory.ContainsKey(fighter.ThingID);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref fighterHistory, "fighterHistory", LookMode.Value, LookMode.Deep);
            Scribe_Values.Look(ref totalTournaments, "totalTournaments");
            Scribe_Values.Look(ref totalDragonBallsWon, "totalDragonBallsWon");
            Scribe_Values.Look(ref totalGoldWon, "totalGoldWon");
        }

        public void OpenHistoryWindow()
        {
            Find.WindowStack.Add(new TournamentHistoryWindow(this));
        }

        public IEnumerable<TournamentHistoryEntry> GetAllHistory()
        {
            return fighterHistory.Values.OrderByDescending(h => h.winRate);
        }

        public int GetTotalTournaments() => totalTournaments;
        public int GetTotalDragonBalls() => totalDragonBallsWon;
        public int GetTotalGold() => totalGoldWon;
    }
}
