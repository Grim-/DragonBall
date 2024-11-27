using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public class Scouter : Apparel, IExtraGUIDrawer
    {
        private float heightOffset = -1f;
        private float minRange = 1f;
        private float maxRange = 80f;
        public float scanRadius = 40f;
        public int normalScanTicks = 600;
        public int fastScanTicks = 300;
        private ScouterMode scouterMode = ScouterMode.TARGET;
        private ScanSpeed scanSpeed = ScanSpeed.NORMAL;
        private int currentTick = 0;
        private Pawn EquipOwner;
        private Dictionary<int, float> pawnPowerLevels = new Dictionary<int, float>();
        private Pawn targetedPawn;

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            EquipOwner = pawn;
            Log.Message("EQUIPPED SCOUTER");
        }

        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);
            EquipOwner = null;
        }

        public override void Tick()
        {
            base.Tick();
            if (scouterMode == ScouterMode.SCANNING && IsWearerSelected())
            {
                currentTick++;
                int tickThreshold = scanSpeed == ScanSpeed.NORMAL ? normalScanTicks : fastScanTicks;

                if (currentTick >= tickThreshold)
                {
                    UpdatePowerLevels(EquipOwner);
                    currentTick = 0;
                }
            }
        }

        private bool IsWearerSelected()
        {
            return EquipOwner != null && Find.Selector.SelectedObjects.Contains(EquipOwner);
        }

        public void ToggleMode()
        {
            if (scouterMode == ScouterMode.SCANNING)
            {
                SetScouterMode(ScouterMode.TARGET);
            }
            else
            {
                pawnPowerLevels.Clear();
                SetScouterMode(ScouterMode.SCANNING);
            }
        }

        public void SetScouterMode(ScouterMode newMode)
        {
            scouterMode = newMode;
            if (newMode == ScouterMode.TARGET)
            {
                targetedPawn = null;
            }
        }

        public override IEnumerable<Gizmo> GetWornGizmos()
        {
            foreach (var item in base.GetWornGizmos())
            {
                yield return item;
            }

            // Mode toggle
            yield return new Command_Action
            {
                defaultLabel = $"Toggle Scouter Mode [Active : {ModeToNiceString()}]",
                defaultDesc = "Switch between power level overlay and targeting mode",
                icon = ContentFinder<Texture2D>.Get("Things/Building/Misc/DropBeacon"),
                action = delegate { ToggleMode(); }
            };


            if (scouterMode == ScouterMode.SCANNING)
            {
                // Scan speed toggle
                yield return new Command_Action
                {
                    defaultLabel = $"Scan Speed: {scanSpeed}",
                    defaultDesc = "Toggle between normal and fast scanning speeds",
                    icon = ContentFinder<Texture2D>.Get("Things/Building/Misc/DropBeacon"),
                    action = delegate
                    {
                        scanSpeed = scanSpeed == ScanSpeed.NORMAL ? ScanSpeed.FAST : ScanSpeed.NORMAL;
                    }
                };

                yield return new ScouterRangeGizmo(
                    current: scanRadius,
                    min: minRange,
                    max: maxRange,
                    onChange: (float value) => scanRadius = value
                );
            }

            if (scouterMode == ScouterMode.TARGET)
            {
                yield return new Command_Target
                {
                    defaultLabel = "Scan Target",
                    defaultDesc = "Scan a specific target's power level",
                    icon = ContentFinder<Texture2D>.Get("Things/Building/Misc/DropBeacon"),
                    action = delegate (LocalTargetInfo target)
                    {
                        if (target.Pawn != null && target.Pawn.RaceProps.Humanlike)
                        {
                            targetedPawn = target.Pawn;
                            float powerLevel = PowerLevelUtility.GetCurrentPowerLevel(target.Pawn);
                            Messages.Message($"Tracking {targetedPawn.Label}", MessageTypeDefOf.NeutralEvent);
                            Messages.Message($"Power level reading: {PowerLevelUtility.GetPowerLevelDisplay(powerLevel)}", MessageTypeDefOf.NeutralEvent);
                        }
                    },
                    targetingParams = new TargetingParameters()
                    {
                        canTargetPawns = true,
                        canTargetSelf = true,
                        canTargetBuildings = false,
                        canTargetCorpses = false
                    }
                };
            }
        }

        private string ModeToNiceString()
        {
            string Result = "";

            switch (scouterMode)
            {
                case ScouterMode.TARGET:
                    Result = "Target";
                    break;
                case ScouterMode.SCANNING:
                    Result = "Scanner";
                    break;
            }

            return Result;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref scouterMode, "scouterMode");
            Scribe_Values.Look(ref scanSpeed, "scanSpeed");
            Scribe_Values.Look(ref currentTick, "currentTick");
            Scribe_Values.Look(ref scanRadius, "scanRadius", 40f);
            Scribe_References.Look(ref targetedPawn, "targetedPawn");
            Scribe_Collections.Look(ref pawnPowerLevels, "pawnPowerLevels", LookMode.Value, LookMode.Value);
        }

        private void UpdatePowerLevels(Pawn wearer)
        {
            pawnPowerLevels.Clear();

            foreach (Pawn pawn in wearer.Map.mapPawns.AllPawnsSpawned)
            {
                if (pawn == wearer || !pawn.RaceProps.Humanlike)
                    continue;

                float distance = pawn.Position.DistanceTo(wearer.Position);
                if (distance > scanRadius)
                    continue;

                float powerLevel = PowerLevelUtility.GetCurrentPowerLevel(pawn);
                pawnPowerLevels[pawn.thingIDNumber] = powerLevel;
            }
        }

        public void DrawExtraGUI()
        {
            if (EquipOwner == null || !EquipOwner.Spawned || !IsWearerSelected())
                return;

            if ((int)Current.CameraDriver.CurrentZoom >= 3)
                return;

            // Draw wearer's power level
            DrawPowerLevelLabel(EquipOwner, PowerLevelUtility.GetCurrentPowerLevel(EquipOwner), 0);

            if (scouterMode == ScouterMode.TARGET && targetedPawn != null)
            {
                // Draw only the targeted pawn's power level
                float distance = targetedPawn.Position.DistanceTo(EquipOwner.Position);
                if (distance <= scanRadius && !targetedPawn.Map.fogGrid.IsFogged(targetedPawn.Position))
                {
                    DrawPowerLevelLabel(targetedPawn, PowerLevelUtility.GetCurrentPowerLevel(targetedPawn), distance);
                }
            }
            else if (scouterMode == ScouterMode.SCANNING)
            {
                // Draw all pawns within range
                foreach (var pawn in EquipOwner.Map.mapPawns.AllPawnsSpawned)
                {
                    if (pawn == EquipOwner || !pawn.RaceProps.Humanlike)
                        continue;

                    if (pawn.Map.fogGrid.IsFogged(pawn.Position))
                        continue;

                    float distance = pawn.Position.DistanceTo(EquipOwner.Position);
                    if (distance > scanRadius)
                        continue;

                    float powerLevel;
                    if (!pawnPowerLevels.TryGetValue(pawn.thingIDNumber, out powerLevel))
                    {
                        powerLevel = PowerLevelUtility.GetCurrentPowerLevel(pawn);
                        pawnPowerLevels[pawn.thingIDNumber] = powerLevel;
                    }

                    DrawPowerLevelLabel(pawn, powerLevel, distance);
                }
            }
        }

        private void DrawPowerLevelLabel(Pawn pawn, float powerLevel, float distance)
        {
            float wearerPowerLevel = PowerLevelUtility.GetCurrentPowerLevel(EquipOwner);
            float ratio = Mathf.Clamp(powerLevel / wearerPowerLevel, 0f, 2f);

            Color powerLevelColor = ratio <= 1f ?
                Color.Lerp(Color.green, Color.yellow, ratio) :
                Color.Lerp(Color.yellow, Color.red, ratio - 1f);

            Vector2 powerLabelPos = GenMapUI.LabelDrawPosFor(pawn, heightOffset);
            string powerText = $"Power Level: {PowerLevelUtility.GetPowerLevelDisplay(powerLevel)}";

            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperCenter;
            GenMapUI.DrawThingLabel(powerLabelPos, powerText, powerLevelColor);

            Vector2 distanceLabelPos = GenMapUI.LabelDrawPosFor(pawn, heightOffset - 0.4f);
            string distanceText = $"Distance: {distance:F1}";

            GenMapUI.DrawThingLabel(distanceLabelPos, distanceText, Color.gray);
            Text.Anchor = TextAnchor.UpperLeft;
        }
    }
}
