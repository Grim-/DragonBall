﻿using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DragonBall
{

    public class TournamentTracker : GameComponent
    {
        private Dictionary<string, TournamentHistoryEntry> fighterHistory = new Dictionary<string, TournamentHistoryEntry>();
        private Dictionary<int, Dictionary<string, Tournament>> tournaments = new Dictionary<int, Dictionary<string, Tournament>>();
        private int totalTournaments = 0;
        private int totalDragonBallsWon = 0;
        private int totalGoldWon = 0;

        public TournamentTracker(Game game)
        {
        }

        public void RecordTournamentResults(Pawn fighter, List<TournamentMatchResult> results, int dragonBallsWon, int goldWon, string winnerID)
        {
            if (results == null || !results.Any()) return;

            int tournamentId = results.First().TournamentID;


            if (!tournaments.TryGetValue(tournamentId, out var tournamentEntries))
            {
                tournamentEntries = new Dictionary<string, Tournament>();
                tournaments[tournamentId] = tournamentEntries;
                totalTournaments++;
            }


            if (!tournamentEntries.TryGetValue(fighter.ThingID, out var tournament))
            {
                tournament = new Tournament
                {
                    TournamentID = tournamentId,
                    FighterID = fighter.ThingID,
                    WinnerID = winnerID
                };
                tournamentEntries.Add(fighter.ThingID, tournament);
            }

            if (!fighterHistory.TryGetValue(fighter.ThingID, out var entry))
            {
                entry = new TournamentHistoryEntry
                {
                    fighterName = fighter.Label,
                    fighterID = fighter.ThingID, 
                    tournaments = new List<Tournament>()
                };
                fighterHistory.Add(fighter.ThingID, entry);
            }

            if (!entry.tournaments.Any(t => t.TournamentID == tournamentId))
            {
                entry.tournaments.Add(tournament);
            }

            tournament.Matches.Clear(); 
            tournament.Matches.AddRange(results);

            entry.totalFights += results.Count;
            entry.victories += results.Count(r => r.Victory);
            entry.totalExperience += results.Sum(r => r.ExperienceGained);
            entry.dragonBallsWon += dragonBallsWon;
            entry.goldWon += goldWon;

            // Update global stats
            totalDragonBallsWon += dragonBallsWon;
            totalGoldWon += goldWon;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref fighterHistory, "fighterHistory", LookMode.Value, LookMode.Deep);
            if (Scribe.mode == LoadSaveMode.Saving)
            {
                var flattenedTournaments = new List<SaveableTournament>();
                foreach (var kvp in tournaments)
                {
                    foreach (var innerKvp in kvp.Value)
                    {
                        flattenedTournaments.Add(new SaveableTournament(kvp.Key, innerKvp.Key, innerKvp.Value));
                    }
                }
                Scribe_Collections.Look(ref flattenedTournaments, "tournaments", LookMode.Deep);
            }
            //else
            else if(Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                tournaments.Clear();
                List<SaveableTournament> flattenedTournaments = new List<SaveableTournament>();
                Scribe_Collections.Look(ref flattenedTournaments, "tournaments", LookMode.Deep);
                if (flattenedTournaments != null)
                {
                    foreach (var st in flattenedTournaments)
                    {
                        if (!tournaments.TryGetValue(st.TournamentId, out var innerDict))
                        {
                            innerDict = new Dictionary<string, Tournament>();
                            tournaments[st.TournamentId] = innerDict;
                        }
                        innerDict[st.FighterId] = st.Tournament;
                    }
                }
            }
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

    public class SaveableTournament : IExposable
    {
        public int TournamentId;
        public string FighterId;
        public Tournament Tournament;

        public SaveableTournament() { } // Required empty constructor for loading

        public SaveableTournament(int tournamentId, string fighterId, Tournament tournament)
        {
            TournamentId = tournamentId;
            FighterId = fighterId;
            Tournament = tournament;
        }

        public void ExposeData()
        {
            Scribe_Values.Look(ref TournamentId, "tournamentId");
            Scribe_Values.Look(ref FighterId, "fighterId");
            Scribe_Deep.Look(ref Tournament, "tournament");
        }
    }
}
