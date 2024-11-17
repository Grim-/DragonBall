using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public class Window_WishSelection : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private readonly Map map;
        private readonly Building_DragonBallAltar altar;
        private readonly List<BaseWish> availableWishes;
        private static readonly Vector2 WinSize = new Vector2(400f, 600f);

        public override Vector2 InitialSize => WinSize;

        public Window_WishSelection(Map map, Building_DragonBallAltar altar)
        {
            this.map = map;
            this.altar = altar;
            this.doCloseX = true;
            this.doCloseButton = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;

            // Generate all available wishes
            availableWishes = new List<BaseWish>();
            foreach (WishDef wishDef in DefDatabase<WishDef>.AllDefs)
            {
                // Create instance of the wish type
                BaseWish wishPrototype = (BaseWish)Activator.CreateInstance(wishDef.wishClass);
                wishPrototype.def = wishDef;

                // Generate all concrete wishes from this type
                availableWishes.AddRange(wishPrototype.GenerateWishes(map, altar));
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(0f, 0f, inRect.width, 35f), "Make Your Wish");
            Text.Font = GameFont.Small;

            Rect outRect = new Rect(0f, 40f, inRect.width, inRect.height - 40f);
            float viewHeight = availableWishes.Count * 50f; // Height per wish
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, viewHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);

            float curY = 0f;
            foreach (BaseWish wish in availableWishes)
            {
                Rect wishRect = new Rect(0f, curY, viewRect.width, 45f);

                if (Widgets.ButtonText(wishRect, wish.Label))
                {
                    if (wish.CanBeGranted(map, altar))
                    {
                        wish.Grant(map, altar);
                        altar.ScatterGatheredDragonBalls();
                        Close();
                    }
                }

                // Draw description
                Widgets.DrawHighlightIfMouseover(wishRect);
                TooltipHandler.TipRegion(wishRect, wish.Description);

                curY += 50f;
            }

            Widgets.EndScrollView();
        }
    }
}
