using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public class HealWish : BaseWish
    {

        private Pawn TargetPawn;


        public override string Label => $"Heal {TargetPawn.LabelShort}";


        public HealWish(WishDef def, Pawn targetPawn) : base(def)
        {
            this.def = def;
            this.TargetPawn = targetPawn;
        }

        public HealWish(WishDef def) : base(def)
        {

        }

        public HealWish()
        {

        }

        public override IEnumerable<BaseWish> GenerateWishes(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            foreach (var item in map.mapPawns.AllPawns.Where(x=> x.Faction == Faction.OfPlayer).ToList())
            {
                yield return new HealWish(def, item);
            }    
        }


        private Texture2D PortraitCache = null;

        public override Texture2D GetIcon()
        {
            if (TargetPawn != null)
            {
                if (PortraitCache == null)
                {
                    CachePortrait(TargetPawn);
                }
                return PortraitCache;
            }
            return base.GetIcon();
        }

        private void CachePortrait(Pawn pawn)
        {
            Vector2 size = new Vector2(50f, 50f);
            RenderTexture renderTexture = PortraitsCache.Get(pawn, size, Rot4.South);

            PortraitCache = new Texture2D(renderTexture.width, renderTexture.height);
            RenderTexture.active = renderTexture;
            PortraitCache.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
            PortraitCache.Apply();
            RenderTexture.active = null;
        }

        public override bool CanBeGranted(Map map, Building_DragonBallAltar altar, Pawn pawn) => true;

        public override void Grant(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            if (TargetPawn != null && !TargetPawn.Destroyed && !TargetPawn.Dead)
            {
                RestorePawnHealth(TargetPawn);
            }
            else
            {
                if (pawn != null && !pawn.Destroyed && !pawn.Dead)
                {
                    RestorePawnHealth(pawn);
                }
            }

        }

        public static void RestorePawnHealth(Pawn Pawn)
        {
            List<Hediff> hediffsToRemove = Pawn.health.hediffSet.hediffs
            .Where(h =>
                h.def.isBad)
            .ToList();


            foreach (Hediff hediff in hediffsToRemove)
            {
                Pawn.health.RemoveHediff(hediff);
            }

            if (Pawn.needs != null)
            {
                foreach (Need need in Pawn.needs.AllNeeds)
                {
                    need.CurLevel = need.MaxLevel;
                }
            }
        }
    }
}
