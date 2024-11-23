
using RimWorld;
using RimWorld.QuestGen;
using SaiyanMod;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaranMagicFramework;
using UnityEngine;
using Verse;
using Verse.Grammar;

namespace DragonBall
{
    public class QuestNode_TournamentFight : QuestNode
    {
        public SlateRef<string> inSignalEnable;
        public SlateRef<string> colonistsReturnSignal;
        public SlateRef<float> luckFactor = 0.2f;
        public SlateRef<int> updateInterval = 60000;
        public SlateRef<int> tournamentDurationTicks = 60000;
        public SlateRef<int> winsRequired = 2;
        public SlateRef<int> maxFights = 3;

        public SlateRef<float> tournamentWinXP = 10000;
        public SlateRef<float> tournamentLoseXP = 4000;

        public SlateRef<int> timesToRoll = 1;
        public List<RewardItemOption> rewards;

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            QuestGen.quest.AddPart(new QuestPart_SimulateTournament
            {
                inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID("pickupShipThing.SentSatisfied"),
                colonistsReturnSignal = QuestGenUtility.HardcodedSignalWithQuestID("ColonistsReturned"),
                luckFactor = luckFactor.GetValue(slate),
                maxFights = maxFights.GetValue(slate),
                winsRequired = winsRequired.GetValue(slate),
                tournamentDurationTicks = tournamentDurationTicks.GetValue(slate),
                tournamentWinXP = tournamentWinXP.GetValue(slate),
                tournamentLoseXP = tournamentLoseXP.GetValue(slate),
                rewards = rewards.ToList(),
                timesToRoll = timesToRoll.GetValue(slate)
            });
        }
    }

    public class QuestPart_SimulateTournament : QuestPartActivable
    {
        public string colonistsReturnSignal;
        public float luckFactor = 0.2f;
        public int maxFights = 3;
        public int winsRequired = 2;
        public int timesToRoll = 1;
        public int tournamentDurationTicks = 60000;
        public float tournamentWinXP;
        public float tournamentLoseXP;
        public List<RewardItemOption> rewards;


        private int currentDay = 0;
        private int ticksUntilNextUpdate = -1;
        private bool tournamentActive = false;
        private Dictionary<string, List<TournamentMatchResult>> fighterResults = new Dictionary<string, List<TournamentMatchResult>>();
        private Dictionary<string, FighterData> tournamentFighters = new Dictionary<string, FighterData>();
        public override string DescriptionPart => base.DescriptionPart + GetDebugDescription();


        private int ticksBetweenFights
        {
            get
            {
                if (tournamentDurationTicks <= 0)
                {
                    tournamentDurationTicks = 60000;
                }

                if (maxFights <= 0)
                {
                    maxFights = 3;
                }

                return tournamentDurationTicks / (maxFights + 1);
            }
        }

        private List<string> pawnThingIDs;
        private List<List<TournamentMatchResult>> matchResultValues;
        private TournamentBracket bracket;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref colonistsReturnSignal, "colonistsReturnSignal");
            Scribe_Values.Look(ref luckFactor, "luckFactor", 0.2f);
            Scribe_Values.Look(ref currentDay, "currentDay");
            Scribe_Values.Look(ref tournamentDurationTicks, "tournamentDurationTicks", 60000);
            Scribe_Values.Look(ref ticksUntilNextUpdate, "ticksUntilNextUpdate", -1);
            Scribe_Values.Look(ref tournamentActive, "tournamentActive", false);
            Scribe_Values.Look(ref maxFights, "maxFights", 3);
            Scribe_Values.Look(ref timesToRoll, "timesToRoll", 1);
            Scribe_Values.Look(ref winsRequired, "winsRequired", 2);
            Scribe_Values.Look(ref tournamentWinXP, "tournamentWinXP");
            Scribe_Values.Look(ref tournamentLoseXP, "tournamentLoseXP");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (fighterResults == null)
                    fighterResults = new Dictionary<string, List<TournamentMatchResult>>();

                if (tournamentFighters == null)
                    tournamentFighters = new Dictionary<string, FighterData>();

                if (pawnThingIDs == null)
                    pawnThingIDs = new List<string>();

                if (matchResultValues == null)
                    matchResultValues = new List<List<TournamentMatchResult>>();
            }

            Scribe_Collections.Look(ref fighterResults, "fighterResults", LookMode.Value, LookMode.Deep, ref pawnThingIDs, ref matchResultValues);
            Scribe_Collections.Look(ref tournamentFighters, "tournamentFighters", LookMode.Value, LookMode.Deep);
            Scribe_Collections.Look(ref rewards, "rewards", LookMode.Deep);
            Scribe_Deep.Look(ref bracket, "bracket");

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                ValidateAndRestoreState();
            }
        }

        private void ValidateAndRestoreState()
        {
            if (bracket != null)
            {
                // Ensure RoundMatches is initialized
                if (bracket.RoundMatches == null)
                {
                    bracket.RoundMatches = new Dictionary<int, List<TournamentMatch>>();
                }

                // Validate all matches have proper state
                for (int round = 1; round <= bracket.TotalRounds; round++)
                {
                    if (!bracket.RoundMatches.ContainsKey(round))
                        bracket.RoundMatches[round] = new List<TournamentMatch>();

                    foreach (var match in bracket.RoundMatches[round])
                    {
                        // Restore match state based on scores
                        if (match.Fighter1Score > 0 || match.Fighter2Score > 0)
                        {
                            if (match.Winner == null)
                            {
                                match.Winner = match.Fighter1Score > match.Fighter2Score ? match.Fighter1 : match.Fighter2;
                                match.Loser = match.Fighter1Score > match.Fighter2Score ? match.Fighter2 : match.Fighter1;
                            }
                            match.IsComplete = true;
                        }
                    }
                }

                // Update tournament state
                if (bracket.CurrentRound > bracket.TotalRounds)
                {
                    bracket.IsComplete = true;
                    tournamentActive = false;
                }
            }


            var lendPart = quest.PartsListForReading.OfType<QuestPart_LendColonistsToFaction>().FirstOrDefault();
            if (lendPart?.LentColonistsListForReading != null)
            {
                foreach (Pawn pawn in lendPart.LentColonistsListForReading)
                {
                    if (tournamentFighters.ContainsKey(pawn.ThingID))
                    {
                        if (!fighterResults.ContainsKey(pawn.ThingID))
                        {
                            fighterResults[pawn.ThingID] = new List<TournamentMatchResult>();
                        }
                    }
                }
            }
        }

        public int WinsRequired
        {
            get
            {
                if (winsRequired > maxFights)
                {
                    winsRequired = maxFights - 1;
                }
                else if (winsRequired < 0)
                {
                    winsRequired = maxFights - 1;
                }

                return winsRequired;
            }
        }

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);

            if (signal.tag == QuestGenUtility.HardcodedSignalWithQuestID("SkipToFight"))
            {
                SkipToNextFight();
            }
            else if (signal.tag == inSignalEnable)
            {
                StartTournament();
            }
            else if (signal.tag == colonistsReturnSignal)
            {
                FinishTournament();
            }
        }

        private void StartTournament()
        {
            var lendPart = quest.PartsListForReading.OfType<QuestPart_LendColonistsToFaction>().FirstOrDefault();
            if (lendPart?.LentColonistsListForReading != null)
            {
                List<FighterData> allFighters = new List<FighterData>();

                fighterResults.Clear();
                foreach (Pawn pawn in lendPart.LentColonistsListForReading)
                {
                    fighterResults[pawn.ThingID] = new List<TournamentMatchResult>();
                    var fighterData = FighterData.FromPawn(pawn);
                    if (fighterData != null)
                    {
                        fighterData.IsPawn = true;
                        allFighters.Add(fighterData);
                        tournamentFighters[pawn.ThingID] = fighterData;
                    }
                }

                // Generate NPC opponents
                GenerateOpponents(allFighters);

                bracket = new TournamentBracket(allFighters);
                maxFights = bracket.TotalRounds;
                tournamentActive = true;
                ticksUntilNextUpdate = ticksBetweenFights;
            }
        }

        private void GenerateOpponents(List<FighterData> allFighters)
        {
            // Get quest difficulty (defaults to 100 if not found)
            float questDifficulty = 100f;
            if (quest?.challengeRating != null)
            {
                questDifficulty = quest.challengeRating;
            }

            // Calculate how many fighters we need
            int desiredFighters = Mathf.NextPowerOfTwo(allFighters.Count + 2);

            while (allFighters.Count < desiredFighters)
            {
                // Calculate average score of existing fighters
                float avgScore = allFighters.Average(f => f.CalculateScore()["Total Score"]);

                // Generate opponent using quest difficulty
                var npc = OpponentGenerator.GenerateOpponent(
                    targetScore: avgScore,
                    difficultyLevel: questDifficulty
                );

                npc.IsPawn = false;
                allFighters.Add(npc);
            }
        }

        private float GetScaledDifficulty(int currentRound, int totalRounds)
        {
            if (quest?.challengeRating == null)
                return 100f;

            float baseDifficulty = quest.challengeRating;

            //Scale difficulty up in later rounds
            float roundProgression = (float)currentRound / totalRounds;
            float difficultyScaling = 1f + (roundProgression * 0.5f); // Up to 50% harder in final round

            return baseDifficulty * difficultyScaling;
        }

        private void SkipToNextFight()
        {
            if (!tournamentActive || currentDay >= maxFights)
                return;

            ticksUntilNextUpdate = 10;
        }

        private void FinishTournament()
        {
            if (tournamentActive)
            {
                tournamentActive = false;

                while (currentDay < maxFights)
                {
                    currentDay++;
                    SimulateDailyMatches();
                }

                HandleTournamentConclusion();

            }

            if (this.State == QuestPartState.Enabled) this.Complete();
        }

        public override void QuestPartTick()
        {
            base.QuestPartTick();

            if (!tournamentActive || bracket == null)
                return;

            ticksUntilNextUpdate--;
            if (ticksUntilNextUpdate <= 0)
            {
                // Sync currentDay with bracket rounds
                currentDay = bracket.CurrentRound;

                if (currentDay > maxFights || bracket.IsComplete)
                {
                    tournamentActive = false;
                    HandleTournamentConclusion();
                    return;
                }

                SimulateDailyMatches();

                if (bracket.IsComplete || currentDay >= maxFights)
                {
                    tournamentActive = false;
                    HandleTournamentConclusion();
                }
                else
                {
                    ticksUntilNextUpdate = ticksBetweenFights;
                }
            }
        }

        private void SimulateDailyMatches()
        {
            if (bracket == null || bracket.IsComplete) return;

            // Update currentDay to match bracket round
            currentDay = bracket.CurrentRound;

            var currentMatches = bracket.GetCurrentRoundMatches();
            if (currentMatches == null)
            {
                Log.Warning($"No matches found for round {bracket.CurrentRound}");
                return;
            }

            foreach (var match in currentMatches)
            {
                if (!match.IsComplete)
                {
                    SimulateMatch(match);
                    Messages.Message($"Tournament Round {bracket.CurrentRound}: {match.Fighter1.Name} vs {match.Fighter2.Name} - Winner: {match.Winner.Name} (Score: {(match.Winner == match.Fighter1 ? match.Fighter1Score : match.Fighter2Score):F0} vs {(match.Winner == match.Fighter1 ? match.Fighter2Score : match.Fighter1Score):F0})", MessageTypeDefOf.NeutralEvent);
                }
            }

            bracket.AdvanceToNextRound();
        }

        private void SimulateMatch(TournamentMatch match)
        {
            if (match.IsComplete || match.Winner != null) return;

            match.Fighter1ScoreData = match.Fighter1.CalculateScore();
            match.Fighter2ScoreData = match.Fighter2.CalculateScore();

            float fighter1Score = match.Fighter1ScoreData["Total Score"];
            float fighter2Score = match.Fighter2ScoreData["Total Score"];


            match.Fighter1Score = fighter1Score;
            match.Fighter2Score = fighter2Score;

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
            TournamentBattleLogger.LogTournamentFight(match.Winner, match.Loser);
            RecordAllMatchResults(match);
        }
        private void RecordAllMatchResults(TournamentMatch match)
        {
            foreach (var thingId in fighterResults.Keys.ToList())
            {
                if (tournamentFighters.TryGetValue(thingId, out var fighterData))
                {
                    RecordMatchResult(FindPawnByID(thingId), fighterData, match);
                }
            }
        }

        private void RecordMatchResult(Pawn pawn, FighterData fighterData, TournamentMatch match)
        {
            if (!tournamentFighters.ContainsKey(pawn.ThingID)) return;
            if (fighterData.Name != match.Fighter1.Name && fighterData.Name != match.Fighter2.Name) return;
            if (match.Winner == null || match.Loser == null) return;

            bool isWinner = fighterData.Name == match.Winner.Name;
            float xpGain = isWinner ? tournamentWinXP : tournamentLoseXP;

            // Award XP if possible
            if (pawn.TryGetKiAbilityClass(out AbilityClassKI abilityClassKI))
            {
                abilityClassKI.GainXP(xpGain);
            }


            var playerScoreData = (fighterData == match.Fighter1)
                ? match.Fighter1ScoreData
                : match.Fighter2ScoreData;

            var opponentScoreData = (fighterData == match.Fighter1)
                ? match.Fighter2ScoreData
                : match.Fighter1ScoreData;


            float playerScore = fighterData == match.Fighter1 ? match.Fighter1Score : match.Fighter2Score;
            float opponentScore = fighterData == match.Fighter1 ? match.Fighter2Score : match.Fighter1Score;

            var result = new TournamentMatchResult
            {
                Victory = isWinner,
                ScoreMargin = Mathf.Abs(playerScore - opponentScore) / playerScore,
                OpponentName = isWinner ? match.Loser.Name : match.Winner.Name,
                ExperienceGained = xpGain,
                Round = bracket.CurrentRound,
                TournamentID = quest.id,
                ScoreContributions = playerScoreData,
                OpponentScoreContributions = opponentScoreData,
                MatchID = match.MatchId
            };

            Log.Message($"Recording {(isWinner ? "win" : "loss")} for {pawn.Label} in round {match.Round}");
            fighterResults[pawn.ThingID].Add(result);
        }

        private void HandleTournamentConclusion()
        {
            if (bracket == null)
            {
                Log.Warning("Tournament concluded but bracket is null");
                Complete();
                return;
            }

            // Ensure matches in final round have winners set
            if (bracket.CurrentRound >= bracket.TotalRounds)
            {
                var finalMatches = bracket.GetCurrentRoundMatches();
                foreach (var match in finalMatches)
                {
                    if (match.Winner == null && !match.IsComplete)
                    {
                        SimulateMatch(match);
                    }
                }
            }

            bracket.IsComplete = true;

            var winner = bracket.GetTournamentWinner();
            if (winner == null)
            {
                Log.Warning("Tournament concluded but no winner found - simulating final matches");
                // Force simulation of remaining matches if needed
                while (!bracket.IsComplete && bracket.CurrentRound <= bracket.TotalRounds)
                {
                    SimulateDailyMatches();
                    bracket.AdvanceToNextRound();
                }
                winner = bracket.GetTournamentWinner();
            }

            if (winner == null)
            {
                Log.Error("Failed to determine tournament winner");
                Complete();
                return;
            }

            var tracker = Current.Game.GetComponent<TournamentTracker>();
            if (tracker == null)
            {
                Log.Message($"Tournament Tracker not found, cannot record tournament details.");
                return;
            }


            Log.Message($"Tournament Tracker Winner is {winner.Name}");

            foreach (var fighterEntry in fighterResults)
            {
                string thingID = fighterEntry.Key;
                var results = fighterEntry.Value;
                // Find the pawn by ThingID
                Pawn pawn = FindPawnByID(thingID);
                if (pawn == null)
                {
                    //Log.Message($"Could not find Pawn with ThingID {thingID}");
                    continue;
                }

                if (tournamentFighters.TryGetValue(thingID, out var fighterData))
                {
                    bool isWinner = fighterData.Name == winner.Name;
                    int victories = results.Count(r => r.Victory);

                    if (isWinner)
                    {
                        OnPlayerPawnWonTournament(pawn, victories);
                    }
                    else
                    {
                        OnPlayerPawnLostTournament(pawn, victories);
                    }

                    tracker.RecordTournamentResults(pawn, results, 0, 0, pawn.ThingID);
                }
            }

            

            Log.Message($"Tournament concluded! Checking results:");
            foreach (var fighterEntry in fighterResults)
            {
                string thingID = fighterEntry.Key;
                var results = fighterEntry.Value;
                Pawn pawn = FindPawnByID(thingID);
                if (pawn == null) continue;

                int victories = results.Count(r => r.Victory);
                Log.Message($"{pawn.Label}: {victories} wins out of {results.Count} matches");
            }

            Messages.Message($"Tournament Complete! Winner: {winner.Name}", MessageTypeDefOf.PositiveEvent);
            if(this.State == QuestPartState.Enabled) this.Complete();
        }

        public Pawn FindPawnByID(string thingID)
        {
            var lendPart = quest.PartsListForReading.OfType<QuestPart_LendColonistsToFaction>().FirstOrDefault();
            if (lendPart?.LentColonistsListForReading != null)
            {
                return lendPart.LentColonistsListForReading.Find(x => x.ThingID == thingID) as Pawn;
            }

           Pawn foundPawn = null;

            // Check all maps
            foreach (Map map in Current.Game.Maps)
            {
                foreach (var pawn in map.mapPawns.AllPawns)
                {
                    if (pawn.ThingID == thingID)
                        foundPawn =  pawn;
                }
            }

            if (foundPawn != null)
            {
                return foundPawn;
            }

            foundPawn = Find.WorldPawns.AllPawnsAliveOrDead
                .FirstOrDefault(p => p.ThingID == thingID);
            return foundPawn;
        }


        private List<Pawn> TryGetLentColonists()
        {
            var lendPart = quest.PartsListForReading.OfType<QuestPart_LendColonistsToFaction>().FirstOrDefault();
            if (lendPart?.LentColonistsListForReading != null)
            {
                return lendPart.LentColonistsListForReading.Cast<Pawn>().ToList();
            }

            return null;
        }

        private string GetDebugDescription()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[Tournament] Info");
            sb.AppendLine($"[Tournament] Active: {tournamentActive}");
            sb.AppendLine($"Current Day: {currentDay}/{maxFights}");
            sb.AppendLine($"Time until next update: {ticksUntilNextUpdate.ToStringTicksToPeriod(true, true, true)}");

            if (bracket != null)
            {
                sb.AppendLine($"Bracket - Round: {bracket.CurrentRound}/{bracket.TotalRounds}");
                sb.AppendLine($"Bracket Complete: {bracket.IsComplete}");


                var currentMatches = bracket.GetCurrentRoundMatches();
                if (currentMatches != null && currentMatches.Any())
                {
                    sb.AppendLine("Current Matches:");
                    foreach (var match in currentMatches)
                    {
                        sb.AppendLine($"- {match.Fighter1?.Name} vs {match.Fighter2?.Name}");
                        sb.AppendLine($"  Complete: {match.IsComplete}");
                        if (match.IsComplete)
                        {
                            sb.AppendLine($"  Winner: {match.Winner?.Name}");
                            sb.AppendLine($"  Scores: {match.Fighter1Score:F0} vs {match.Fighter2Score:F0}");
                        }
                    }
                }

                //var previousRoundMatches = bracket.GetPreviousRoundMatches();
                //if (previousRoundMatches != null && previousRoundMatches.Any())
                //{
                //    sb.AppendLine("Previous Matches:");
                //    foreach (var match in previousRoundMatches)
                //    {
                //        sb.AppendLine($"- {match.Fighter1?.Name} vs {match.Fighter2?.Name}");
                //        sb.AppendLine($"  Complete: {match.IsComplete}");
                //        if (match.IsComplete)
                //        {
                //            sb.AppendLine($"  Winner: {match.Winner?.Name}");
                //            sb.AppendLine($"  Scores: {match.Fighter1Score:F0} vs {match.Fighter2Score:F0}");
                //        }
                //    }
                //}

                // Show tournament results so far
                sb.AppendLine("\nFighter Results:");
                foreach (var kvp in fighterResults)
                {
                    var pawn = kvp.Key;
                    var results = kvp.Value;
                    if (results != null)
                    {
                        Pawn foundPawn = FindPawnByID(pawn);

                        if (foundPawn != null)
                        {
                            int wins = results.Count(r => r.Victory);
                            int totalMatches = results.Count;
                            sb.AppendLine($"{foundPawn.LabelShort}: {wins}/{totalMatches} wins");
                        }

    
                    }
                }
            }
            else
            {
                sb.AppendLine("Bracket is null!");
            }

            return sb.ToString().TrimEnd();
        }
    

        private void OnPlayerPawnWonTournament(Pawn fighter, int victories)
        {
            if (fighter.TryGetKiAbilityClass(out AbilityClassKI abilityClassKI))
            {
                abilityClassKI.GainXP(this.tournamentWinXP);
            }

            fighter.needs?.mood?.thoughts?.memories?.TryGainMemory(DBDefOf.DragonBallWonTournament);
            GenerateRewards(fighter);
            Messages.Message($"{fighter.LabelShort} won the tournament with {victories} victories! They gained {this.tournamentWinXP} experience.", MessageTypeDefOf.PositiveEvent);
        }

        private void OnPlayerPawnLostTournament(Pawn fighter, int victories)
        {
            if (fighter.TryGetKiAbilityClass(out AbilityClassKI abilityClassKI))
            {
                abilityClassKI.GainXP(this.tournamentLoseXP);
            }

            fighter.needs?.mood?.thoughts?.memories?.TryGainMemory(DBDefOf.DragonBallTookPartInTournament);
            Messages.Message($"{fighter.LabelShort} finished the tournament with {victories} victories, they did not win. They gained {this.tournamentLoseXP} experience.", MessageTypeDefOf.NeutralEvent);
        }

        private void GenerateRewards(Pawn pawn)
        {
            var rewardsCopy = rewards.ToList();

            for (int i = 0; i < timesToRoll; i++)
            {
                RewardItemOption rewardItemOption = rewardsCopy.RandomElement();
                rewardsCopy.Remove(rewardItemOption);

                Thing thing = ThingMaker.MakeThing(rewardItemOption.thing);
                thing.stackCount = rewardItemOption.count.RandomInRange;
                pawn.inventory.TryAddAndUnforbid(thing);
                Messages.Message($"{pawn.LabelShort} won {thing.Label} in the tournament!", MessageTypeDefOf.PositiveEvent);

                if (rewardsCopy.Count == 0)
                {
                    rewardsCopy = rewards.ToList();
                }
            }
        }
    }

    public static class TournamentBattleLogger
    {
        public static void LogTournamentFight(FighterData winner, FighterData loser)
        {
            // Only log if the winner or loser is an actual pawn
            if (!winner.IsPawn && !loser.IsPawn)
                return;

            //Find.BattleLog.Add(
            //    new BattleLogEntry_Event(
            //        initiator,
            //        recipient,
            //        DefDatabase<RulePackDef>.GetNamed(ruleDef),
            //        new Dictionary<string, string>()
            //        {
            //        { "winner_name", winner.Name },
            //        { "loser_name", loser.Name }
            //        }
            //    )
            //);
        }
    }
}
