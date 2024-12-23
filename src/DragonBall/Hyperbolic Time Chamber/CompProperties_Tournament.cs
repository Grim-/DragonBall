//using RimWorld;
//using System;
//using System.Collections.Generic;
//using Verse;

//namespace DragonBall
//{
//    public class CompProperties_Tournament : CompProperties
//    {
//        public CompProperties_Tournament()
//        {
//            compClass = typeof(TournamentComp);
//        }
//    }

//    public class TournamentComp : ThingComp
//    {
//        public TournamentState state = TournamentState.INACTIVE;
//        public TournamentRules currentRules = new TournamentRules();
//        public TournamentMatchNew currentMatch;
//        private TrapArenaMutator arenaMutator;
//        private HyperbolicChamber_Building ParentChamber =>
//            (HyperbolicChamber_Building)parent;

//        public override void CompTick()
//        {
//            base.CompTick();
//            if (state == TournamentState.MATCH_IN_PROGRESS)
//            {
//                TickMatch();
//            }
//        }

//        public void StartTournament(TournamentRules rules)
//        {
//            if (state != TournamentState.INACTIVE) return;

//            currentRules = rules;
//            state = TournamentState.PREPARING;
//            // Tournament initialization logic will go here
//        }

//        public void StartMatch(Pawn challenger)
//        {
//            if (state != TournamentState.PREPARING) return;

//            try
//            {
//                Pawn opponent = GenerateOpponent();
//                currentMatch = new TournamentMatchNew
//                {
//                    challenger = challenger,
//                    opponent = opponent,
//                    rules = currentRules,
//                    startTick = Find.TickManager.TicksGame
//                };

//                // Spawn both pawns in the chamber
//                ParentChamber.CreateAndEnterChamber(challenger);
//                SpawnOpponentInChamber(opponent);

//                state = TournamentState.MATCH_IN_PROGRESS;
//                arenaMutator = new TrapArenaMutator(ParentChamber.pocketMap);
//            }
//            catch (Exception e)
//            {
//                Log.Error($"Failed to start tournament match: {e.Message}");
//                CleanupMatch();
//            }
//        }

//        private void TickMatch()
//        {
//            if (currentMatch == null || !currentMatch.IsValid)
//            {
//                HandleMatchEnd(MatchEndReason.INVALID_STATE);
//                return;
//            }

//            if (currentMatch.HasTimedOut)
//            {
//                HandleMatchEnd(MatchEndReason.TIME_OUT);
//                return;
//            }

//            // Check victory conditions
//            if (IsDefeated(currentMatch.challenger))
//            {
//                currentMatch.winner = currentMatch.opponent;
//                HandleMatchEnd(MatchEndReason.DEFEAT);
//            }
//            else if (IsDefeated(currentMatch.opponent))
//            {
//                currentMatch.winner = currentMatch.challenger;
//                HandleMatchEnd(MatchEndReason.DEFEAT);
//            }

//            arenaMutator?.Tick();
//        }

//        private bool IsDefeated(Pawn pawn)
//        {
//            if (pawn.Dead) return true;
//            if (pawn.health.summaryHealth.SummaryHealthPercent < currentRules.MinimumHealthPercentage)
//                return true;

//            return false;
//        }

//        private enum MatchEndReason
//        {
//            INVALID_STATE,
//            TIME_OUT,
//            DEFEAT,
//            MANUAL_END
//        }

//        private void HandleMatchEnd(MatchEndReason reason)
//        {
//            try
//            {
//                state = TournamentState.MATCH_ENDED;

//                // Handle different end conditions
//                switch (reason)
//                {
//                    case MatchEndReason.TIME_OUT:
//                        DetermineTimeoutWinner();
//                        break;
//                    case MatchEndReason.INVALID_STATE:
//                        // Handle invalid state (e.g., one pawn disappeared)
//                        break;
//                }

//                // Return challenger to main map if still alive
//                if (currentMatch.challenger != null && !currentMatch.challenger.Dead)
//                {
//                    ParentChamber.ExitChamber(currentMatch.challenger);
//                }

//                // Cleanup opponent
//                if (currentMatch.opponent != null)
//                {
//                    CleanupOpponent(currentMatch.opponent);
//                }

//                // Process tournament progression here
//                // (Will be implemented with bracket system)

//                CleanupMatch();
//            }
//            catch (Exception e)
//            {
//                Log.Error($"Failed to handle match end: {e.Message}");
//                CleanupMatch();
//            }
//        }

//        private void CleanupMatch()
//        {
//            currentMatch = null;
//            state = TournamentState.PREPARING;
//        }

//        private void DetermineTimeoutWinner()
//        {
//            // Compare health percentages to determine winner on timeout
//            float challengerHealth = currentMatch.challenger.health.summaryHealth.SummaryHealthPercent;
//            float opponentHealth = currentMatch.opponent.health.summaryHealth.SummaryHealthPercent;

//            currentMatch.winner = challengerHealth >= opponentHealth ?
//                currentMatch.challenger : currentMatch.opponent;
//        }

//        private Pawn GenerateOpponent()
//        {
//            // Placeholder - implement opponent generation logic
//            return PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist);
//        }

//        private void SpawnOpponentInChamber(Pawn opponent)
//        {
//            // Spawn opponent in the chamber's pocket map
//            if (ParentChamber.pocketMap != null)
//            {
//                GenSpawn.Spawn(opponent, ParentChamber.pocketMap.Center, ParentChamber.pocketMap);
//            }
//        }

//        private void CleanupOpponent(Pawn opponent)
//        {
//            // Destroy or cleanup the generated opponent
//            if (opponent.Spawned)
//                opponent.Destroy();
//        }

//        public override IEnumerable<Gizmo> CompGetGizmosExtra()
//        {
//            // Tournament control buttons
//            if (state == TournamentState.INACTIVE)
//            {
//                yield return new Command_Action
//                {
//                    defaultLabel = "Start Tournament",
//                    action = delegate
//                    {
//                        StartTournament(new TournamentRules());
//                    }
//                };
//            }

//            if (state == TournamentState.MATCH_IN_PROGRESS)
//            {
//                yield return new Command_Action
//                {
//                    defaultLabel = "End Match",
//                    action = delegate
//                    {
//                        HandleMatchEnd(MatchEndReason.MANUAL_END);
//                    }
//                };
//            }
//        }
//    }

//    public enum TournamentState
//    {
//        INACTIVE,
//        PREPARING,
//        MATCH_IN_PROGRESS,
//        MATCH_ENDED,
//        TOURNAMENT_ENDED
//    }

//    public class TournamentRules
//    {
//        public bool AllowDeath = false;
//        public bool AllowWeapons = true;
//        public int TimeLimit = 2000; // ticks
//        public float MinimumHealthPercentage = 0.1f; // below this % counts as defeat
//    }

//    public class TournamentMatchNew
//    {
//        public Pawn challenger;
//        public Pawn opponent;
//        public int startTick;
//        public TournamentRules rules;
//        public Pawn winner;

//        public bool HasTimedOut => Find.TickManager.TicksGame - startTick > rules.TimeLimit;

//        public bool IsValid => challenger != null && opponent != null &&
//                             !challenger.Dead && !opponent.Dead;
//    }
//}
