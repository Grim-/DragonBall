using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public class Window_WishSelection : Window
    {
        private Vector2 scrollPosition = Vector2.zero;
        private readonly Map map;
        private readonly Pawn TargetPawn;
        private readonly Building_DragonBallAltar altar;
        private readonly Dictionary<string, List<BaseWish>> categorizedWishes;
        private static readonly Vector2 WinSize = new Vector2(800f, 600f);
        private const float WISH_BUTTON_SIZE = 120f;
        private const float WISH_BUTTON_PADDING = 10f;
        private const float SECTION_PADDING = 20f;
        private const int WISHES_PER_ROW = 4;
        private const float CATEGORY_HEADER_HEIGHT = 30f;

        private const float LABEL_HEIGHT = 40f;
        private const float TITLE_HEIGHT = 35f;
        private Color TITLE_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        private Color FOOTER_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        private Color CATEGORY_COLOR = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        private const float INNER_PADDING = 5f;
        
        public override Vector2 InitialSize => WinSize;
        private DragonBallWishTracker WishTracker = null;

        public Window_WishSelection(Map map, Building_DragonBallAltar altar, Pawn TargetPawn)
        {
            this.map = map;
            this.altar = altar;
            this.doCloseX = true;
            this.doCloseButton = true;
            this.absorbInputAroundWindow = true;
            this.forcePause = true;
            this.TargetPawn = TargetPawn;

            if (this.WishTracker == null)
            {
                this.WishTracker = Current.Game.GetComponent<DragonBallWishTracker>();
            }

            if (WishTracker.GetRemainingWishes(map) <= 0)
            {
                WishTracker.InitializeWishes(map);
            }

            categorizedWishes = new Dictionary<string, List<BaseWish>>();

            foreach (WishDef wishDef in DefDatabase<WishDef>.AllDefs)
            {
                if (wishDef == null)
                {
                    Log.Warning("Encountered null WishDef in DefDatabase");
                    continue;
                }

                if (wishDef.wishClass == null)
                {
                    Log.Warning($"WishDef {wishDef.defName} has null wishClass");
                    continue;
                }

                BaseWish wishPrototype;
                try
                {
                    wishPrototype = (BaseWish)Activator.CreateInstance(wishDef.wishClass);
                    if (wishPrototype == null)
                    {
                        Log.Warning($"Failed to create wish instance for {wishDef.defName}");
                        continue;
                    }

                    wishPrototype.def = wishDef;
                    var wishes = wishPrototype.GenerateWishes(map, altar, TargetPawn);

                    string category = wishDef.category ?? "Miscellaneous";
                    if (!categorizedWishes.ContainsKey(category))
                    {
                        categorizedWishes[category] = new List<BaseWish>();
                    }

                    if (wishes != null && wishes.Any())
                    {
                        foreach (var wish in wishes)
                        {
                            if (wish.def == null)
                            {
                                Log.Warning($"Generated wish of type {wish.GetType().Name} has null def");
                                wish.def = wishDef;
                            }
                        }
                        categorizedWishes[category].AddRange(wishes);
                    }
                }
                catch (Exception e)
                {
                    Log.Error($"Error creating wish for def {wishDef.defName}: {e}");
                    continue;
                }
            }
        }



        private void TryDrawIcon(Rect iconRect, BaseWish wish)
        {
            Texture2D iconTexture = wish.GetIcon();
            if (iconTexture != null)
            {
                Widgets.DrawTextureFitted(iconRect, iconTexture, 0.8f);
            }
        }


        private void DrawTitle(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Text.Anchor = TextAnchor.MiddleCenter;
            Rect titleRect = new Rect(0f, 0f, inRect.width, TITLE_HEIGHT);
            GUI.DrawTexture(titleRect, SolidColorMaterials.NewSolidColorTexture(TITLE_COLOR));

            int remainingWishes = WishTracker.GetRemainingWishes(map);
            string titleText = $"Make Your Wish ({remainingWishes} remaining)";
            Widgets.Label(titleRect, titleText);
            Text.Anchor = TextAnchor.UpperLeft;
        }

        public override void DoWindowContents(Rect inRect)
        {
            DrawTitle(inRect);

            // Calculate total height needed for all sections
            float totalHeight = 40f;
            foreach (var category in categorizedWishes)
            {
                if (category.Value.Count > 0)
                {
                    totalHeight += CATEGORY_HEADER_HEIGHT;
                    totalHeight += Mathf.Ceil(category.Value.Count / (float)WISHES_PER_ROW) * (WISH_BUTTON_SIZE + WISH_BUTTON_PADDING);
                    totalHeight += SECTION_PADDING;
                }

            }

            // Create view rect
            Rect outRect = new Rect(0f, 40f, inRect.width, inRect.height - 40f);
            Rect viewRect = new Rect(0f, 0f, outRect.width - 16f, totalHeight);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            float curY = 0f;

            foreach (var category in categorizedWishes)
            {
                if (category.Value.Count <= 0)
                {
                    continue;
                }

                // Draw category header
                Text.Font = GameFont.Medium;
                Rect headerRect = new Rect(0f, curY, viewRect.width, CATEGORY_HEADER_HEIGHT);
                GUI.DrawTexture(headerRect, SolidColorMaterials.NewSolidColorTexture(CATEGORY_COLOR));
                Widgets.Label(headerRect, category.Key);
                curY += 35f;

                // Draw wishes in a grid
                float curX = 0f;
                foreach (BaseWish wish in category.Value)
                {
                    if (curX + WISH_BUTTON_SIZE > viewRect.width)
                    {
                        curX = 0f;
                        curY += WISH_BUTTON_SIZE + WISH_BUTTON_PADDING;
                    }

                    Rect wishRect = new Rect(curX, curY, WISH_BUTTON_SIZE, WISH_BUTTON_SIZE);
                    DrawWishButton(wishRect, wish);
                    curX += WISH_BUTTON_SIZE + WISH_BUTTON_PADDING;
                }

                curY += WISH_BUTTON_SIZE + SECTION_PADDING;
            }

            Widgets.EndScrollView();
        }


        private void DrawWishButton(Rect buttonRect, BaseWish wish)
        {
            bool canBeGranted = wish.CanBeGranted(map, altar, TargetPawn) && WishTracker.GetRemainingWishes(map) > 0;
            GUI.color = canBeGranted ? Color.white : Color.gray;

            Widgets.DrawBox(buttonRect, 1, SolidColorMaterials.NewSolidColorTexture(Color.grey));
            Rect innerRect = buttonRect.ContractedBy(INNER_PADDING);

            float labelHeight = LABEL_HEIGHT;
            Rect iconRect = new Rect(
                innerRect.x,
                innerRect.y,
                innerRect.width,
                innerRect.height - labelHeight
            );

            TryDrawIcon(iconRect, wish);


            Rect footerRect = new Rect(
                buttonRect.x,
                buttonRect.y + buttonRect.height - labelHeight,
                buttonRect.width,
                labelHeight
            );
            GUI.DrawTexture(footerRect, SolidColorMaterials.NewSolidColorTexture(FOOTER_COLOR));


            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(footerRect, wish.Label);


            if (Mouse.IsOver(buttonRect))
            {
                Widgets.DrawHighlight(buttonRect);
                TooltipHandler.TipRegion(buttonRect, wish.Description);

                if (Widgets.ButtonInvisible(buttonRect) && canBeGranted)
                {
                    wish.Grant(map, altar, TargetPawn);
                    WishTracker.UseWish(map);

                    if (WishTracker.GetRemainingWishes(map) <= 0)
                    {
                        altar.ScatterGatheredDragonBalls();
                        Close();
                    }
                }
            }


            Text.Anchor = TextAnchor.UpperLeft;
            GUI.color = Color.white;
        }
    }
}
