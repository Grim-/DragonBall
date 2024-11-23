using System;
using System.Collections.Generic;
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
        private HashSet<int> expandedTournaments = new HashSet<int>();
        private HashSet<string> expandedMatches = new HashSet<string>();
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

            // Global summary
            Rect summaryRect = new Rect(inRect.x, inRect.y + padding, inRect.width, summaryHeight);
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
                GUI.color = Color.black;
                Widgets.Label(rowRect, $"{fighter.fighterName} ({fighter.winRate:P0})");

                if (Widgets.ButtonImageWithBG(rowRect,SolidColorMaterials.NewSolidColorTexture(Color.gray)))
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

            GUI.EndGroup();
        }

        private void DrawTournamentHistory(Rect rect)
        {
            var tournaments = selectedFighter.tournaments.OrderByDescending(t => t.TournamentID).ToList();

            // Calculate the total height needed
            float totalHeight = 0f;
            foreach (var tournament in tournaments)
            {
                totalHeight += 40f; // Base tournament row height
                if (expandedTournaments.Contains(tournament.TournamentID))
                {
                    foreach (var match in tournament.Matches.OrderBy(m => m.Round))
                    {
                        totalHeight += 30f; // Base match row height
                        if (expandedMatches.Contains(match.MatchID))
                        {
                            // Calculate score breakdown height
                            int playerScoreLines = match.ScoreContributions.Count + 4;
                            int opponentScoreLines = match.OpponentScoreContributions.Count + 2;
                            totalHeight += (playerScoreLines + opponentScoreLines) * 20f;
                        }
                    }
                }
            }

            Rect viewRect = new Rect(0f, 0f, rect.width - 16f, totalHeight);
            Widgets.BeginScrollView(rect, ref matchScrollPosition, viewRect);

            float curY = 0f;
            foreach (var tournament in tournaments)
            {
                curY = DrawTournamentRow(new Rect(0f, curY, viewRect.width, totalHeight - curY), tournament, tournament.WinnerID == selectedFighter.fighterID);
            }

            Widgets.EndScrollView();
        }
        private float DrawTournamentRow(Rect rect, Tournament tournament, bool WasWin)
        {
            float curY = rect.y;
            bool expanded = expandedTournaments.Contains(tournament.TournamentID);

            Rect headerRect = new Rect(rect.x, curY, rect.width, 40f);
            Widgets.DrawBoxSolid(headerRect, WasWin ? Color.yellow : new Color(0.2f, 0.2f, 0.2f, 0.3f));

            Rect arrowRect = new Rect(headerRect.x + 5f, headerRect.y + 10f, 20f, 20f);
            if (Widgets.ButtonText(arrowRect, expanded ? "▼" : "►", false))
            {
                if (expanded)
                    expandedTournaments.Remove(tournament.TournamentID);
                else
                    expandedTournaments.Add(tournament.TournamentID);
            }

            Text.Font = GameFont.Small;
            Rect labelRect = new Rect(headerRect.x + 30f, headerRect.y + 10f, rect.width - 40f, 25f);
            int victories = tournament.Matches.Count(m => m.Victory);
            Widgets.Label(labelRect, $"Tournament #{tournament.TournamentID} - {victories}/{tournament.Matches.Count} Victories");

            curY += 40f;

            if (expanded)
            {
                foreach (var match in tournament.Matches.OrderBy(m => m.Round))
                {
                    curY = DrawMatchRow(new Rect(rect.x + 20f, curY, rect.width - 20f, 30f), match);
                }
            }

            return curY;
        }

        private float DrawMatchRow(Rect rect, TournamentMatchResult match)
        {
            float curY = rect.y;
            bool expanded = expandedMatches.Contains(match.MatchID);

            // Match header row
            Rect headerRect = new Rect(rect.x, curY, rect.width, 30f);
            GUI.color = match.Victory ? new Color(0.2f, 0.5f, 0.2f, 0.3f) : new Color(0.5f, 0.2f, 0.2f, 0.3f);
            Widgets.DrawBoxSolid(headerRect, GUI.color);
            GUI.color = Color.white;

            // Draw expand/collapse arrow
            Rect arrowRect = new Rect(headerRect.x + 5f, headerRect.y + 5f, 20f, 20f);
            if (Widgets.ButtonText(arrowRect, expanded ? "▼" : "►", false))
            {
                if (expanded)
                    expandedMatches.Remove(match.MatchID);
                else
                    expandedMatches.Add(match.MatchID);
            }

            // Match summary
            Text.Font = GameFont.Small;
            string result = match.Victory ? "Victory" : "Defeat";
            Rect labelRect = new Rect(headerRect.x + 30f, headerRect.y + 5f, rect.width - 40f, 20f);
            Widgets.Label(labelRect, $"Round {match.Round}: {result} vs {match.OpponentName} (Margin: {match.ScoreMargin:P0})");

            curY += 30f;

            if (expanded)
            {
                GUI.color = Color.white;
                float scoreHeight = 20f;
                float columnWidth = (rect.width - 35f) / 2;
                float leftColumn = rect.x + 30f;
                float rightColumn = leftColumn + columnWidth + 5f;
                float startY = curY;

                // Left column
                Text.Font = GameFont.Tiny;
                Widgets.Label(new Rect(leftColumn, curY, columnWidth, scoreHeight), "Your Score Breakdown:");
                curY += scoreHeight;

                foreach (var score in match.ScoreContributions)
                {
                    Widgets.Label(new Rect(leftColumn + 15f, curY, columnWidth - 15f, scoreHeight),
                        $"{score.Key}: {score.Value:F1}");
                    curY += scoreHeight;
                }


                float leftHeight = curY + scoreHeight - startY;

                // Right column - Opponent scores
                curY = startY;  // Reset Y
                Widgets.Label(new Rect(rightColumn, curY, columnWidth, scoreHeight),
                    $"{match.OpponentName}'s Score Breakdown:");
                curY += scoreHeight;

                foreach (var score in match.OpponentScoreContributions)
                {
                    Widgets.Label(new Rect(rightColumn + 15f, curY, columnWidth - 15f, scoreHeight),
                        $"{score.Key}: {score.Value:F1}");
                    curY += scoreHeight;
                }


                float rightHeight = curY + scoreHeight - startY;

                curY = startY + Math.Max(leftHeight, rightHeight);
            }

            return curY;
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

            // Match basic info
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

            // Player's score section
            Text.Font = GameFont.Tiny;
            Widgets.Label(new Rect(5f, curY, rect.width - 10f, lineHeight),
                "Your Score Breakdown:");
            curY += lineHeight;

            if (match.ScoreContributions.Any())
            {
                foreach (var contribution in match.ScoreContributions)
                {
                    Widgets.Label(new Rect(15f, curY, rect.width - 20f, lineHeight),
                        $"{contribution.Key}: {contribution.Value:F1}");
                    curY += lineHeight;
                }
                Widgets.Label(new Rect(15f, curY, rect.width - 20f, lineHeight),
                    $"Total Score: {match.TotalScore:F1}");
                curY += lineHeight;
            }
            else
            {
                Widgets.Label(new Rect(15f, curY, rect.width - 20f, lineHeight),
                    "No score details available");
                curY += lineHeight;
            }

            // Add some spacing between sections
            curY += 5f;

            // Opponent's score section
            Widgets.Label(new Rect(5f, curY, rect.width - 10f, lineHeight),
                $"{match.OpponentName}'s Score Breakdown:");
            curY += lineHeight;

            if (match.OpponentScoreContributions.Any())
            {
                foreach (var contribution in match.OpponentScoreContributions)
                {
                    Widgets.Label(new Rect(15f, curY, rect.width - 20f, lineHeight),
                        $"{contribution.Key}: {contribution.Value:F1}");
                    curY += lineHeight;
                }
                Widgets.Label(new Rect(15f, curY, rect.width - 20f, lineHeight),
                    $"Total Score: {match.OpponentTotalScore:F1}");
                curY += lineHeight;
            }
            else
            {
                Widgets.Label(new Rect(15f, curY, rect.width - 20f, lineHeight),
                    "No score details available");
                curY += lineHeight;
            }

            Text.Font = GameFont.Small;
            GUI.EndGroup();
        }
    }
}
