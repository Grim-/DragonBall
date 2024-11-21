using RimWorld;
using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public class ResearchWish : BaseWish
    {
        public ResearchWish()
        {
        }

        public ResearchWish(WishDef def) : base(def)
        {

        }

        private ResearchWishDef Def => (ResearchWishDef)def;

        public override bool CanBeGranted(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            return Find.ResearchManager.GetProject(KnowledgeCategoryDefOf.Basic) != null && !Find.ResearchManager.GetProject(KnowledgeCategoryDefOf.Basic).IsFinished;
        }

        public override IEnumerable<BaseWish> GenerateWishes(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            if (Def != null)
            {
                yield return new ResearchWish(def);
            }
        }

        public override void Grant(Map map, Building_DragonBallAltar altar, Pawn pawn)
        {
            ResearchProjectDef currentProject = Find.ResearchManager.GetProject(KnowledgeCategoryDefOf.Basic);
            if (currentProject == null || currentProject.IsFinished) return;

            float currentProgress = Find.ResearchManager.GetProgress(currentProject);
            float targetProgress = currentProject.CostFactor(TechLevel.Spacer) * currentProject.baseCost * Def.researchPercentage;

            if (currentProgress < targetProgress)
            {
                float progressToAdd = targetProgress - currentProgress;
                Find.ResearchManager.ResearchPerformed(progressToAdd, pawn);
            }
        }
    }
}
