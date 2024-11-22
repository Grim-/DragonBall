using System.Linq;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public class TournamentHistoryWindow : Window
    {
        private TournamentTracker tracker;
        private Vector2 scrollPosition = Vector2.zero;
        private TournamentHistoryEntry selectedFighter = null;
        private string searchText = "";
        private Vector2 matchScrollPosition = Vector2.zero;


        public TournamentHistoryWindow(TournamentTracker tracker)
        {
            this.tracker = tracker;
            this.forcePause = true;
            this.doCloseX = true;
            this.doCloseButton = true;
            this.absorbInputAroundWindow = true;
            this.draggable = true;
            this.resizeable = true;
        }

        public override Vector2 InitialSize => new Vector2(800f, 600f);

        public override void DoWindowContents(Rect inRect)
        {
            // Layout
            float padding = 10f;
            float searchHeight = 30f;
            float summaryHeight = 60f;
            float fighterListWidth = 200f;

            // Search bar
            Rect searchRect = new Rect(inRect.x, inRect.y, inRect.width, searchHeight);
            Text.Font = GameFont.Small;
            searchText = Widgets.TextField(searchRect, searchText);

            // Global summary
            Rect summaryRect = new Rect(inRect.x, searchRect.yMax + padding, inRect.width, summaryHeight);
            DrawGlobalSummary(summaryRect);

            // Fighter list (left side)
            Rect fighterListRect = new Rect(inRect.x, summaryRect.yMax + padding,
                fighterListWidth, inRect.height - summaryRect.yMax - padding * 2);
            DrawFighterList(fighterListRect);

            // Fighter details (right side)
            if (selectedFighter != null)
            {
                Rect detailsRect = new Rect(fighterListRect.xMax + padding, fighterListRect.y,
                    inRect.width - fighterListRect.width - padding * 2, fighterListRect.height);
                DrawFighterDetails(detailsRect);
            }
        }

        private void DrawGlobalSummary(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, new Color(0.2f, 0.2f, 0.2f));
            GUI.BeginGroup(rect);
            Text.Font = GameFont.Small;
            Text.Anchor = TextAnchor.MiddleLeft;

            float columnWidth = rect.width / 3;
            Widgets.Label(new Rect(10f, 0f, columnWidth, rect.height),
                $"Total Tournaments: {tracker.GetTotalTournaments()}");
            Widgets.Label(new Rect(columnWidth + 10f, 0f, columnWidth, rect.height),
                $"Dragon Balls Won: {tracker.GetTotalDragonBalls()}");
            Widgets.Label(new Rect(columnWidth * 2 + 10f, 0f, columnWidth, rect.height),
                $"Total Gold Won: {tracker.GetTotalGold()}");

            GUI.EndGroup();
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawFighterList(Rect rect)
        {
            Widgets.DrawMenuSection(rect);

            var fighters = tracker.GetAllHistory()
                .Where(f => string.IsNullOrEmpty(searchText) ||
                           f.fighterName.ToLower().Contains(searchText.ToLower()))
                .ToList();

            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, fighters.Count * 30f);
            Widgets.BeginScrollView(rect, ref scrollPosition, viewRect);

            float curY = 0f;
            foreach (var fighter in fighters)
            {
                Rect rowRect = new Rect(0f, curY, viewRect.width, 30f);
                if (Widgets.ButtonText(rowRect, $"{fighter.fighterName} ({fighter.winRate:P0})"))
                {
                    selectedFighter = fighter;
                }

                if (selectedFighter == fighter)
                {
                    Widgets.DrawHighlightSelected(rowRect);
                }

                curY += 30f;
            }

            Widgets.EndScrollView();
        }

        private void DrawFighterDetails(Rect rect)
        {
            Widgets.DrawMenuSection(rect);
            GUI.BeginGroup(rect);

            float curY = 10f;

            // Fighter header info
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(10f, curY, rect.width - 20f, 30f), selectedFighter.fighterName);
            curY += 40f;

            // Fighter stats
            Text.Font = GameFont.Small;
            float statsHeight = 140f;
            Rect statsRect = new Rect(10f, curY, rect.width - 20f, statsHeight);
            DrawFighterStats(statsRect);
            curY += statsHeight + 10f;

            // Tournament history
            Text.Font = GameFont.Medium;
            Widgets.Label(new Rect(10f, curY, rect.width - 20f, 30f), "Tournament History");
            curY += 35f;

            // Scrollable tournament history
            Rect historyRect = new Rect(10f, curY, rect.width - 20f, rect.height - curY - 10f);
            DrawTournamentHistory(historyRect);

            GUI.EndGroup();
        }

        private void DrawFighterStats(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, new Color(0.2f, 0.2f, 0.2f, 0.3f));
            GUI.BeginGroup(rect);
            float lineHeight = 25f;
            float curY = 5f;

            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(10f, curY, rect.width - 20f, lineHeight),
                $"Total Fights: {selectedFighter.totalFights}");
            curY += lineHeight;

            Widgets.Label(new Rect(10f, curY, rect.width - 20f, lineHeight),
                $"Victories: {selectedFighter.victories} ({selectedFighter.winRate:P0})");
            curY += lineHeight;

            Widgets.Label(new Rect(10f, curY, rect.width - 20f, lineHeight),
                $"Total Experience Gained: {selectedFighter.totalExperience:F0}");
            curY += lineHeight;

            Widgets.Label(new Rect(10f, curY, rect.width - 20f, lineHeight),
                $"Dragon Balls Won: {selectedFighter.dragonBallsWon}");
            curY += lineHeight;

            Widgets.Label(new Rect(10f, curY, rect.width - 20f, lineHeight),
                $"Gold Won: {selectedFighter.goldWon}");

            GUI.EndGroup();
        }

        private void DrawTournamentHistory(Rect rect)
        {
            float tournamentHeight = 200f; // Height per tournament section
            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, selectedFighter.tournaments.Count * tournamentHeight);
            Widgets.BeginScrollView(rect, ref matchScrollPosition, viewRect);

            float curY = 0f;
            foreach (var tournament in selectedFighter.tournaments.OrderByDescending(t => t.TournamentID))
            {
                Rect tournamentRect = new Rect(0f, curY, viewRect.width, tournamentHeight - 10f);
                DrawTournamentSection(tournamentRect, tournament);
                curY += tournamentHeight;
            }

            Widgets.EndScrollView();
        }

        private void DrawTournamentSection(Rect rect, Tournament tournament)
        {
            Widgets.DrawBoxSolid(rect, new Color(0.2f, 0.2f, 0.2f, 0.3f));
            GUI.BeginGroup(rect);

            // Tournament header
            Text.Font = GameFont.Small;
            Widgets.Label(new Rect(10f, 5f, rect.width - 20f, 25f),
                $"Tournament #{tournament.TournamentID}");

            float curY = 35f;
            float matchHeight = 30f;
            float matchWidth = (rect.width - 30f) / tournament.Matches.Count;

            // Draw matches side by side
            float curX = 10f;
            foreach (var match in tournament.Matches.OrderBy(m => m.Round))
            {
                Rect matchRect = new Rect(curX, curY, matchWidth, rect.height - curY - 10f);
                DrawMatchDetails(matchRect, match);
                curX += matchWidth + 5f;
            }

            GUI.EndGroup();
        }

        private void DrawMatchDetails(Rect rect, TournamentMatchResult match)
        {
            GUI.color = match.Victory ? new Color(0.2f, 0.5f, 0.2f, 0.3f) : new Color(0.5f, 0.2f, 0.2f, 0.3f);
            Widgets.DrawBoxSolid(rect, GUI.color);
            GUI.color = Color.white;

            GUI.BeginGroup(rect);
            float curY = 5f;
            float lineHeight = 20f;

            Text.Font = GameFont.Small;
            string result = match.Victory ? "Victory" : "Defeat";
            Widgets.Label(new Rect(5f, curY, rect.width - 10f, lineHeight),
                $"Round {match.Round}: {result}");
            curY += lineHeight;

            Widgets.Label(new Rect(5f, curY, rect.width - 10f, lineHeight),
                $"vs {match.OpponentName}");
            curY += lineHeight;

            Widgets.Label(new Rect(5f, curY, rect.width - 10f, lineHeight),
                $"Margin: {match.ScoreMargin:P0}");
            curY += lineHeight;

            Widgets.Label(new Rect(5f, curY, rect.width - 10f, lineHeight),
            $"Margin: {match.ScoreMargin:P0}");
                    curY += lineHeight;

            // Score breakdowns will go here once added
            if (match.ScoreContributions.Any())
            {
                foreach (var contribution in match.ScoreContributions)
                {
                    Widgets.Label(new Rect(5f, curY, rect.width - 10f, lineHeight),
                        $"{contribution.Key}: {contribution.Value:F1}");
                    curY += lineHeight;
                }
            }

            GUI.EndGroup();
        }
    }
}
