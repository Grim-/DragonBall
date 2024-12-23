//using RimWorld;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using Verse;
//using Verse.AI;

//namespace DragonBall
//{
//    public class HyperbolicChamber_Building : Building
//    {
//        public Map pocketMap;
//        public int MaxOccupants = 2;
//        public float TimeMultiplier = 365f;
//        private HashSet<Pawn> occupants = new HashSet<Pawn>();
//        private Dictionary<Pawn, int> entryTicks = new Dictionary<Pawn, int>();
//        private ChamberState state = ChamberState.UNOCCUPIED;


//        private TournamentComp _TournamentComp;
//        private TournamentComp TournamentComp
//        {
//            get
//            {
//                if (_TournamentComp == null)
//                {
//                    _TournamentComp = this.GetComp<TournamentComp>();
//                }

//                return _TournamentComp;
//            }
//        }

//        private bool EnableTimeDilation = true;
//        public override void ExposeData()
//        {
//            base.ExposeData();
//            Scribe_References.Look(ref pocketMap, "pocketMap");
//            Scribe_Collections.Look(ref occupants, "occupants", LookMode.Reference);
//            Scribe_Collections.Look(ref entryTicks, "entryTicks", LookMode.Reference, LookMode.Value);
//            Scribe_Values.Look(ref state, "chamberState");

//            // Initialize collections if null after loading
//            if (occupants == null)
//            {
//                occupants = new HashSet<Pawn>();
//            }
//            if (entryTicks == null)
//            {
//                entryTicks = new Dictionary<Pawn, int>();
//            }
//        }

//        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
//        {
//            if (!selPawn.CanReach(this, PathEndMode.Touch, Danger.Deadly, false, false, TraverseMode.ByPawn))
//            {
//                yield break;
//            }


//            if (TournamentComp != null && TournamentComp.state == TournamentState.PREPARING)
//            {
//                yield return new FloatMenuOption("Enter as Contender", delegate
//                {
//                    TournamentComp.StartMatch(selPawn);
//                });
//            }

//            if (IsPawnInChamber(selPawn))
//            {
//                yield return new FloatMenuOption("Exit Hyperbolic Chamber", () => ExitChamber(selPawn));
//            }
//            else if (CanEnterChamber(selPawn))
//            {
//                yield return new FloatMenuOption("Enter Hyperbolic Chamber", () => CreateAndEnterChamber(selPawn));
//            }
//            else
//            {
//                string reason = GetEntryDenialReason(selPawn);
//                yield return new FloatMenuOption(reason, null);
//            }
//        }

//        public override IEnumerable<Gizmo> GetGizmos()
//        {
//            foreach (var item in base.GetGizmos())
//            {
//                yield return item;
//            }



//            yield return new Command_Action
//            {
//                defaultLabel = $"Eject All",
//                defaultDesc = "Eject all Occupants",
//                icon = ContentFinder<Texture2D>.Get("Things/Building/Misc/MarriageSpot"),
//                action = delegate
//                {
//                    EjectAllOccupants();
//                }
//            };
//        }


//        public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
//        {
//            EjectAllOccupants();
//            base.Destroy(mode);
//        }


//        public void EjectAllOccupants()
//        {
//            Log.Message($"Ejecting All Occupants.");

//            foreach (var item in occupants.ToList())
//            {
//                ExitChamber(item);
//            }
//        }


//        public bool IsPawnInChamber(Pawn pawn)
//        {
//            return occupants.Contains(pawn);
//        }

//        public bool CanEnterChamber(Pawn pawn)
//        {
//            return state != ChamberState.DISABLED &&
//                   occupants.Count < MaxOccupants &&
//                   !IsPawnInChamber(pawn);
//        }

//        private string GetEntryDenialReason(Pawn pawn)
//        {
//            if (state == ChamberState.DISABLED)
//                return "Chamber is currently disabled";
//            if (occupants.Count >= MaxOccupants)
//                return "Chamber is at maximum capacity (2 occupants)";
//            return "Cannot enter chamber";
//        }

//        public void CreateAndEnterChamber(Pawn pawn)
//        {
//            try
//            {
//                Log.Message($"Attempting to enter hyperbolic chamber with pawn {pawn.LabelShort}...");

//                // Create map if it doesn't exist
//                if (pocketMap == null)
//                {
//                    pocketMap = HyperbolicMapUtility.CreateHyperbolicDimension(this.Map);
//                    state = ChamberState.UNOCCUPIED;
//                }

//                if (pocketMap != null)
//                {
//                    TransferToMap(pawn, pocketMap, pocketMap.Center);
//                    occupants.Add(pawn);
//                    entryTicks[pawn] = Find.TickManager.TicksGame;
//                    UpdateChamberState();
//                    Log.Message($"Pawn {pawn.LabelShort} entered chamber successfully");
//                }
//            }
//            catch (Exception e)
//            {
//                Log.Error($"Failed to create/enter hyperbolic chamber: {e.Message}\n{e.StackTrace}");
//            }
//        }

//        public void ExitChamber(Pawn pawn)
//        {
//            if (!IsPawnInChamber(pawn)) return;

//            try
//            {
//                Log.Message($"Pawn {pawn.LabelShort} exiting chamber...");

//                // Calculate time spent in chamber
//                int entryTick;
//                if (entryTicks.TryGetValue(pawn, out entryTick))
//                {
//                    int ticksSpent = Find.TickManager.TicksGame - entryTick;
//                    ApplyTimeEffects(pawn, ticksSpent);
//                    entryTicks.Remove(pawn);
//                }

//                TransferToMap(pawn, this.Map, this.Position);
//                occupants.Remove(pawn);
//                UpdateChamberState();

//                // Only destroy the map if it's empty
//                if (!occupants.Any())
//                {
//                    HyperbolicMapUtility.DestroyHyperbolicDimension(pocketMap);
//                    pocketMap = null;
//                }

//                Log.Message($"Pawn {pawn.LabelShort} exited chamber successfully");
//            }
//            catch (Exception e)
//            {
//                Log.Error($"Failed to exit hyperbolic chamber: {e.Message}\n{e.StackTrace}");
//            }
//        }

//        private void UpdateChamberState()
//        {
//            if (occupants.Count == 0)
//            {
//                state = ChamberState.UNOCCUPIED;
//            }
//            else if (occupants.Count >= MaxOccupants)
//            {
//                state = ChamberState.FULL;
//            }
//            else
//            {
//                state = ChamberState.OCCUPIED;
//            }
//        }

//        private void ApplyTimeEffects(Pawn pawn, int ticksSpent)
//        {
//            if (EnableTimeDilation)
//            {
//                // Convert real time to chamber time using multiplier
//                float chamberTicks = ticksSpent * TimeMultiplier;

//                Log.Message($"Pawn {pawn.LabelShort} aged {GenDate.ToStringTicksToPeriod((int)chamberTicks)} in the chamber.");

//                pawn.ageTracker.AgeBiologicalTicks += (long)chamberTicks;
//            }


//        }

//        private void TransferToMap(Pawn pawn, Map targetMap, IntVec3 position)
//        {
//            if (!pawn.Spawned) return;
//            pawn.DeSpawn();
//            GenSpawn.Spawn(pawn, position, targetMap);
//        }

//        public override void Tick()
//        {
//            base.Tick();

//            var invalidPawns = occupants.Where(p => p.Dead || p.Destroyed).ToList();
//            foreach (var pawn in invalidPawns)
//            {
//                occupants.Remove(pawn);
//                entryTicks.Remove(pawn);
//            }

//            if (invalidPawns.Any())
//            {
//                UpdateChamberState();
//            }
//        }
//    }
//}
