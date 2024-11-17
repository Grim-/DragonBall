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
        public static bool HasDragonBallOfType(ThingDef dragonBall)
        {
            return Find.Maps
                .SelectMany(m => m.listerBuildings.AllBuildingsColonistOfClass<Building_DragonBallAltar>())
                .Any(altar => altar.GetDragonBallAtPosition(dragonBall) != null);
        }

        // Helper method to get dragon ball at a specific position
        public Thing GetDragonBallAtPosition(ThingDef dragonBallDef)
        {
            IntVec3 position = GetDragonBallPosition(dragonBallDef);
            return Map.thingGrid.ThingsAt(position)
                .FirstOrDefault(t => t.def == dragonBallDef);
        }

        // Add this method to find collectible dragon balls
        public IEnumerable<Thing> FindCollectibleDragonBalls()
        {
            return Map.listerThings.AllThings
                .Where(t => t.def.thingCategories?.Contains(DBDefOf.DragonBallsCategory) ?? false)
                .Where(t => GetDragonBallAtPosition(t.def) == null) // Check if position is empty
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
                IntVec3 targetCell = CellFinder.RandomEdgeCell(Map);

                DragonBallFlying flying = (DragonBallFlying)ThingMaker.MakeThing(DBDefOf.DragonBallFlying);
                flying.Launch(
                    launcher: this, 
                    usedTarget : this,
                    intendedTarget: new LocalTargetInfo(targetCell),
                    hitFlags: ProjectileHitFlags.None 
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

        private Gizmo CreateSummonGizmo()
        {
            return new Command_Action
            {
                defaultLabel = "Start Dragon Summoning",
                defaultDesc = "Begin the ritual to summon the dragon.",
                icon = ContentFinder<Texture2D>.Get("Things/Building/Misc/DropBeacon"),
                action = delegate
                {
                    Pawn bestPawn = this.Map.mapPawns.FreeColonistsSpawned
                        .OrderByDescending(p => p.skills.GetSkill(SkillDefOf.Intellectual).Level)
                        .FirstOrDefault();

                    if (bestPawn != null)
                    {
                        Job job = JobMaker.MakeJob(DBDefOf.DragonBallSummoning, this);
                        bestPawn.jobs.TryTakeOrderedJob(job);
                    }
                }
            };
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo gizmo in base.GetGizmos())
            {
                yield return gizmo;
            }

            if (HasAllDragonBalls())
            {
                yield return CreateSummonGizmo();
            }

            // Add collect dragon balls button
            if (!HasAllDragonBalls())
            {
                var collectibleDragonBalls = FindCollectibleDragonBalls().ToList();
                if (collectibleDragonBalls.Count > 0)
                {
                    yield return new Command_Action
                    {
                        defaultLabel = "Collect Dragon Balls",
                        defaultDesc = "Order a colonist to collect all available dragon balls and bring them to the altar.",
                        icon = ContentFinder<Texture2D>.Get("Things/Building/Misc/MarriageSpot"),
                        action = delegate
                        {
                            Pawn bestPawn = this.Map.mapPawns.FreeColonistsSpawned
                                .Where(p => p.CanReach(this.Position, PathEndMode.Touch, Danger.Deadly))
                                .OrderBy(p => p.Position.DistanceTo(this.Position))
                                .FirstOrDefault();

                            if (bestPawn != null)
                            {
                                if (collectibleDragonBalls.FirstOrDefault() != null)
                                {
                                    Job job = JobMaker.MakeJob(DBDefOf.GatherDragonBalls, collectibleDragonBalls.FirstOrDefault(), this);
                                    bestPawn.jobs.TryTakeOrderedJob(job);
                                }
                            }
                        }
                    };
                }
            }
        }
    }

    public class DragonBallFlying : Projectile
    {
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            base.Impact(hitThing, blockedByShield);
            // Destroy the flyer itself
            this.Destroy();
        }
    }
}
