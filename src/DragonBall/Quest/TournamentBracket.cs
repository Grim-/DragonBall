using System;
using System.Collections.Generic;
using System.Linq;
using Verse;

namespace DragonBall
{
    public class TournamentBracket : IExposable
    {
        public List<TournamentMatch> Matches;
        public List<FighterData> Fighters;
        public int CurrentRound;
        public int TotalRounds;
        public Dictionary<int, List<TournamentMatch>> RoundMatches;
        public bool IsComplete;

        private int nextMatchIndex;


        public TournamentBracket()
        {
            Matches = new List<TournamentMatch>();
            RoundMatches = new Dictionary<int, List<TournamentMatch>>();
        }

        public TournamentBracket(List<FighterData> fighters) : this()
        {
            CurrentRound = 1;
            IsComplete = false;

            // Calculate total rounds needed
            TotalRounds = (int)Math.Ceiling(Math.Log(fighters.Count, 2));
            int requiredFighters = (int)Math.Pow(2, TotalRounds);

            // Create seeded bracket order
            Fighters = CreateSeededOrder(fighters, requiredFighters);

            // Generate first round matches
            GenerateFirstRoundMatches();
        }
        public bool ValidateState()
        {
            if (CurrentRound > TotalRounds)
            {
                IsComplete = true;
                return true;
            }

            // Verify all previous rounds are complete
            for (int round = 1; round < CurrentRound; round++)
            {
                if (!RoundMatches.TryGetValue(round, out var matches) ||
                    matches.Any(m => !m.IsComplete))
                {
                    Log.Error($"Tournament bracket validation failed: Round {round} has incomplete matches");
                    return false;
                }
            }

            return true;
        }

        private List<FighterData> CreateSeededOrder(List<FighterData> fighters, int totalSlots)
        {
            var seededOrder = new List<FighterData>();
            var playerFighters = fighters.Where(f => f.IsPawn).ToList();
            var npcFighters = fighters.Where(f => !f.IsPawn).ToList();


            int neededNpcs = totalSlots - fighters.Count;
            float avgScore = fighters.Average(f => f.CalculateScore()["Total Score"]);
            for (int i = 0; i < neededNpcs; i++)
            {
                npcFighters.Add(OpponentGenerator.GenerateOpponent(avgScore));
            }


            for (int i = 0; i < totalSlots; i++)
            {
                if (i % 2 == 0 && playerFighters.Any())
                {
                    seededOrder.Add(playerFighters.Pop());
                }
                else if (npcFighters.Any())
                {
                    seededOrder.Add(npcFighters.Pop());
                }
                else if (playerFighters.Any())
                {
                    seededOrder.Add(playerFighters.Pop());
                }
            }

            return seededOrder;
        }

        private void GenerateFirstRoundMatches()
        {
            var firstRoundMatches = new List<TournamentMatch>();
            for (int i = 0; i < Fighters.Count; i += 2)
            {
                var match = new TournamentMatch
                {
                    MatchId = $"R1_M{nextMatchIndex}",
                    Fighter1 = Fighters[i],
                    Fighter2 = Fighters[i + 1],
                    Round = 1,
                    MatchIndex = nextMatchIndex++,
                    IsComplete = false
                };
                firstRoundMatches.Add(match);
                Matches.Add(match);
            }
            RoundMatches[1] = firstRoundMatches;
        }

        public List<TournamentMatch> GetCurrentRoundMatches()
        {
            return RoundMatches.TryGetValue(CurrentRound, out var matches) ? matches : new List<TournamentMatch>();
        }

        public void SimulateMatch(TournamentMatch match, float luckFactor)
        {
            if (match.IsComplete) return;

            match.Fighter1ScoreData = match.Fighter1.CalculateScore();
            match.Fighter2ScoreData = match.Fighter2.CalculateScore();

            float fighter1Score = match.Fighter1ScoreData["Total Score"];
            float fighter2Score = match.Fighter2ScoreData["Total Score"];

            // Apply luck factor
            float luck1 = (float)((Rand.Value * 2 - 1) * luckFactor * fighter1Score);
            float luck2 = (float)((Rand.Value * 2 - 1) * luckFactor * fighter2Score);

            match.Fighter1Score = fighter1Score + luck1;
            match.Fighter2Score = fighter2Score + luck2;

            if (match.Fighter1Score > match.Fighter2Score)
            {
                match.Winner = match.Fighter1;
                match.Loser = match.Fighter2;
            }
            else
            {
                match.Winner = match.Fighter2;
                match.Loser = match.Fighter1;
            }

            match.IsComplete = true;
        }

        public void AdvanceToNextRound()
        {
            if (IsComplete) return;

            var currentMatches = GetCurrentRoundMatches();
            if (currentMatches == null || currentMatches.Any(m => !m.IsComplete))
            {
                Log.Warning($"Cannot advance round - current round {CurrentRound} has incomplete matches");
                return;
            }

            if (CurrentRound >= TotalRounds)
            {
                IsComplete = true;
                var winner = GetTournamentWinner();
                if (winner == null)
                {
                    Log.Error("Tournament completed but no winner found!");
                }
                return;
            }

            CurrentRound++;

            if (CurrentRound > TotalRounds)
            {
                IsComplete = true;
                var winner = GetTournamentWinner();
                if (winner == null)
                {
                    Log.Error("Tournament completed but no winner found!");
                }
                return;
            }

            var winners = currentMatches.Select(m => m.Winner).Where(w => w != null).ToList();
            if (winners.Count == 0)
            {
                Log.Error($"No winners found from round {CurrentRound - 1} matches");
                return;
            }

            var nextRoundMatches = new List<TournamentMatch>();

            for (int i = 0; i < winners.Count; i += 2)
            {
                if (i + 1 >= winners.Count)
                {
                    Log.Warning($"Odd number of winners in round {CurrentRound}");
                    continue;
                }

                var match = new TournamentMatch
                {
                    MatchId = $"R{CurrentRound}_M{nextMatchIndex}",
                    Fighter1 = winners[i],
                    Fighter2 = winners[i + 1],
                    Round = CurrentRound,
                    MatchIndex = nextMatchIndex++,
                    IsComplete = false
                };
                nextRoundMatches.Add(match);
                Matches.Add(match);
            }

            RoundMatches[CurrentRound] = nextRoundMatches;
        }

        public FighterData GetTournamentWinner()
        {
            // Try to get final round matches
            if (!RoundMatches.TryGetValue(TotalRounds, out var finalRoundMatches))
            {
                Log.Error($"No matches found for final round {TotalRounds}, defaulting to highest scoring player");
                // If something went wrong, find the player with the highest score
                return Fighters.Where(f => f.IsPawn)
                              .OrderByDescending(f => f.CalculateScore()["Total Score"])
                              .FirstOrDefault() ?? Fighters.First();
            }

            // Get the final match
            var finalMatch = finalRoundMatches.SingleOrDefault();
            if (finalMatch == null)
            {
                Log.Error("No final match found, defaulting to highest scoring player");
                return Fighters.Where(f => f.IsPawn)
                              .OrderByDescending(f => f.CalculateScore()["Total Score"])
                              .FirstOrDefault() ?? Fighters.First();
            }

            // If the match is complete and has a winner, use that
            if (finalMatch.IsComplete && finalMatch.Winner != null)
            {
                return finalMatch.Winner;
            }

            // If the match isn't complete or winner isn't set, determine winner based on scores
            if (finalMatch.Fighter1.IsPawn && !finalMatch.Fighter2.IsPawn)
            {
                return finalMatch.Fighter1; // Favor the player fighter
            }
            else if (!finalMatch.Fighter1.IsPawn && finalMatch.Fighter2.IsPawn)
            {
                return finalMatch.Fighter2; // Favor the player fighter
            }
            else
            {
                // If both or neither are player fighters, use scores
                var fighter1Score = finalMatch.Fighter1.CalculateScore()["Total Score"];
                var fighter2Score = finalMatch.Fighter2.CalculateScore()["Total Score"];

                // If scores are equal and one is a player, favor the player
                if (Math.Abs(fighter1Score - fighter2Score) < 0.001f)
                {
                    return finalMatch.Fighter1.IsPawn ? finalMatch.Fighter1 :
                           finalMatch.Fighter2.IsPawn ? finalMatch.Fighter2 :
                           finalMatch.Fighter1;
                }

                return fighter1Score > fighter2Score ? finalMatch.Fighter1 : finalMatch.Fighter2;
            }
        }

        public void ExposeData()
        {
            Scribe_Collections.Look(ref Matches, "matches", LookMode.Deep);
            Scribe_Collections.Look(ref Fighters, "fighters", LookMode.Deep);
            Scribe_Collections.Look(ref RoundMatches, "roundMatches", LookMode.Value, LookMode.Deep);
            Scribe_Values.Look(ref CurrentRound, "currentRound");
            Scribe_Values.Look(ref TotalRounds, "totalRounds");
            Scribe_Values.Look(ref IsComplete, "isComplete", false);
            Scribe_Values.Look(ref nextMatchIndex, "nextMatchIndex");

            if (Scribe.mode == LoadSaveMode.LoadingVars)
            {
                Matches = Matches ?? new List<TournamentMatch>();
                RoundMatches = RoundMatches ?? new Dictionary<int, List<TournamentMatch>>();

                if (TotalRounds <= 0 && Fighters != null)
                {
                    TotalRounds = (int)Math.Ceiling(Math.Log(Fighters.Count, 2));
                }
            }

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ValidateState();
            }
        }
    }
}
