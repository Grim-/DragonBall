using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.AI;

namespace DragonBall
{
    public class Building_DragonBallAltar : Building
    {
        private bool isWishInProgress = false;

        public void SetWishInProgress(bool inProgress)
        {
            isWishInProgress = inProgress;
        }

        public void MakeDragonBallsUntouchable()
        {
            foreach (Thing dragonBall in GetDragonBallsInProximity())
            {
                dragonBall.def.selectable = false;
                dragonBall.SetForbidden(true, false);
            }
        }

        public void MakeDragonBallsTouchable()
        {
            foreach (Thing dragonBall in GetDragonBallsInProximity())
            {
                dragonBall.def.selectable = true;
                dragonBall.SetForbidden(false, false);
            }
        }


        public void ShowWishUI(Map map, Pawn TargetPawn)
        {
            Find.WindowStack.Add(new Window_WishSelection(map, this, TargetPawn));

            MakeDragonBallsUntouchable();
        }

        public Thing GetDragonBallAtPosition(ThingDef dragonBallDef)
        {
            IntVec3 position = GetDragonBallPosition(dragonBallDef);
            return Map.thingGrid.ThingsAt(position)
                .FirstOrDefault(t => t.def == dragonBallDef);
        }

        public IEnumerable<Thing> FindCollectibleDragonBalls()
        {
            return Map.listerThings.AllThings
                .Where(t => t.def.thingCategories?.Contains(DBDefOf.DragonBallsCategory) ?? false)
                .Where(t => GetDragonBallAtPosition(t.def) == null)
                .Where(t => !t.IsForbidden(Faction.OfPlayer));
        }

        public bool HasDragonBallOfTypeAlready(Thing thing)
        {
            return GetDragonBallAtPosition(thing.def) != null;
        }

        public bool HasAllDragonBalls()
        {
            return DBUtil.DragonBallPositions.Keys
                .All(dragonBallDef => GetDragonBallAtPosition(dragonBallDef) != null);
        }

        public IntVec3 GetDragonBallPosition(ThingDef dragonBall)
        {
            return this.TrueCenter().ToIntVec3() + DBUtil.GetDragonBallOffset(dragonBall);
        }

        public bool TryStartPlaceDragonBallJob(Thing dragonBall, Pawn pawn)
        {
            Job job = JobMaker.MakeJob(DBDefOf.PlaceDragonBall, dragonBall, this);
            return pawn.jobs.TryTakeOrderedJob(job);
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Pawn selPawn)
        {
            foreach (Thing thing in selPawn.inventory.innerContainer)
            {
                if (thing.HasThingCategory(DBDefOf.DragonBallsCategory))
                {
                    yield return new FloatMenuOption(
                        "Place " + thing.Label,
                        delegate { TryStartPlaceDragonBallJob(thing, selPawn); },
                        MenuOptionPriority.Default);
                }
            }
        }


        public void ScatterGatheredDragonBalls()
        {
            foreach (var item in GetDragonBallsInProximity())
            {
                IntVec3 targetCell = CellFinder.RandomEdgeCell(Rot4.Random, Map);
                DragonBallFlying flying = (DragonBallFlying)ThingMaker.MakeThing(DBDefOf.DragonBallFlying);
                GenSpawn.Spawn(flying, item.Position, Map);
                flying.Launch(
                    launcher: this, 
                    origin : item.Position.ToVector3(),
                    usedTarget : targetCell,
                    intendedTarget: targetCell,
                    hitFlags: ProjectileHitFlags.IntendedTarget 
                );
                item.DeSpawn();
            }
        }

        public List<Thing> GetDragonBallsInProximity()
        {
            List<Thing> dragonBallsToScatter = new List<Thing>();

            foreach (ThingDef dragonBallDef in DBUtil.DragonBallPositions.Keys)
            {
                Thing dragonBall = GetDragonBallAtPosition(dragonBallDef);
                if (dragonBall != null)
                {
                    dragonBallsToScatter.Add(dragonBall);
                }
            }

            return dragonBallsToScatter;
        }



        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (DebugSettings.godMode)
            {
                foreach (var pawn in Map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer))
                {
                    if (pawn.RaceProps.Humanlike)
                    {
                        yield return new Command_Action
                        {
                            defaultLabel = $"DEV Instant Summon Dragon for {pawn.LabelShort}",
                            defaultDesc = "Instantly summons the wish UI",
                            icon = ContentFinder<Texture2D>.Get("Things/Building/Misc/MarriageSpot"),
                            action = delegate
                            {
                                ShowWishUI(Map, pawn);
                            }
                        };
                    }
                }

                yield return new Command_Action
                {
                    defaultLabel = $"DEV Reset CS",
                    defaultDesc = "Resets the Wish cooldown",
                    icon = ContentFinder<Texture2D>.Get("Things/Building/Misc/MarriageSpot"),
                    action = delegate
                    {
                        var wishTracker = Current.Game.GetComponent<DragonBallWishTracker>();

                        if (wishTracker != null)
                        {
                            wishTracker.ResetCooldowns();
                        }
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = $"DEV Reset Wishes",
                    defaultDesc = "Resets wishses",
                    icon = ContentFinder<Texture2D>.Get("Things/Building/Misc/MarriageSpot"),
                    action = delegate
                    {
                        var wishTracker = Current.Game.GetComponent<DragonBallWishTracker>();

                        if (wishTracker != null)
                        {
                            wishTracker.ResetWishes();
                        }
                    }
                };
            }

            yield return new Command_Action
            {
                defaultLabel = $"Show Tournament UI",
                defaultDesc = "Displays the quest tournament UI.",
                icon = ContentFinder<Texture2D>.Get("Things/Building/Misc/MarriageSpot"),
                action = delegate
                {
                    TournamentTracker TournamentTracker = Current.Game.GetComponent<TournamentTracker>();

                    if (TournamentTracker != null)
                    {
                        TournamentTracker.OpenHistoryWindow();
                    }
                }
            };
            if (HasAllDragonBalls())
            {
                var summonOptions =  CreateSummonGizmo();

                foreach (var item in summonOptions)
                {
                    yield return item;
                }
            }

            if (!HasAllDragonBalls())
            {
                var collectibleDragonBalls = FindCollectibleDragonBalls().ToList();
                if (collectibleDragonBalls.Count > 0)
                {
                    var gatherOptions = CreateGatherGizmo(collectibleDragonBalls);

                    foreach (var item in gatherOptions)
                    {
                        yield return item;
                    }            
                }
            }
        }

        private void StartGatherJob(List<Thing> CollectibleDragonBalls, Pawn pawn)
        {


            if (pawn != null)
            {
                if (CollectibleDragonBalls.FirstOrDefault() != null)
                {
                    Job job = JobMaker.MakeJob(DBDefOf.GatherDragonBalls, CollectibleDragonBalls.FirstOrDefault(), this);
                    pawn.jobs.TryTakeOrderedJob(job);
                }
            }
        }


        private IEnumerable<Gizmo> CreateGatherGizmo(List<Thing> CollectibleDragonBalls)
        {
            foreach (var item in Map.mapPawns.FreeColonistsSpawned)
            {
                if (item.RaceProps.Humanlike)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = $"Collect Dragon Balls ({item.LabelShort})",
                        defaultDesc = "Order a colonist to collect all available dragon balls and bring them to the altar.",
                        icon = ContentFinder<Texture2D>.Get("Things/Building/Misc/MarriageSpot"),
                        action = delegate
                        {
                            var list = CollectibleDragonBalls.ToList();
                            StartGatherJob(list, item);
                        }
                    };
                }
            }
        }

        private IEnumerable<Gizmo> CreateSummonGizmo()
        {
            var wishTracker = Current.Game.GetComponent<DragonBallWishTracker>();
            foreach (var item in Map.mapPawns.FreeColonistsSpawned)
            {              
                if (wishTracker != null && item.RaceProps.Humanlike)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = $"Start Dragon Summoning ({item.LabelShort})",
                        defaultDesc = "Begin the ritual to summon the dragon.",
                        icon = ContentFinder<Texture2D>.Get("Things/Building/Misc/DropBeacon"),
                        action = delegate
                        {
                            if (item != null)
                            {
                                var pawnScopeCache = item;
                                Log.Message($"Starting sUmmon Job");
                                Log.Message(pawnScopeCache);
                                Job job = JobMaker.MakeJob(DBDefOf.DragonBallSummoning, this);
                                pawnScopeCache.jobs.TryTakeOrderedJob(job);
                            }
                        },
                        Disabled = !wishTracker.IsWishOffCooldown(this.Map),
                        disabledReason = $"You still have {wishTracker.GetCooldownTicksRemaining(Map).ToStringTicksToDays()} remaining before you can wish again."
                    };
                }
            }
        }

    }
}
