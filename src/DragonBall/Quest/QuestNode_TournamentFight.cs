using RimWorld;
using RimWorld.QuestGen;
using SaiyanMod;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public class QuestNode_TournamentFight : QuestNode
    {
        public SlateRef<string> inSignalEnable;
        public SlateRef<string> colonistsReturnSignal; 
        public SlateRef<float> luckFactor = 0.2f;
        public SlateRef<int> updateInterval = 60000;
        public SlateRef<int> tournamentDurationTicks = 60000;
        public SlateRef<int> maxFights;

        public SlateRef<float> tournamentWinXP = 10000;
        public SlateRef<float> tournamentLoseXP = 4000;

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;
            QuestGen.quest.AddPart(new QuestPart_SimulateTournament
            {
                inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID("pickupShipThing.SentSatisfied"),
                colonistsReturnSignal = QuestGenUtility.HardcodedSignalWithQuestID("ColonistsReturned"),
                luckFactor = luckFactor.GetValue(slate),
                maxFights = maxFights.GetValue(slate),
                tournamentDurationTicks = tournamentDurationTicks.GetValue(slate),
                tournamentWinXP = tournamentWinXP.GetValue(slate),
                tournamentLoseXP = tournamentLoseXP.GetValue(slate),
            });
        }
    }

    public class QuestPart_SimulateTournament : QuestPartActivable
    {
        public string colonistsReturnSignal;
        public float luckFactor = 0.2f;
        public int maxFights = 3;
        public int tournamentDurationTicks = 60000;

        public float tournamentWinXP;
        public float tournamentLoseXP;

        private int currentDay = 0;
        private int ticksUntilNextUpdate = -1;
        private bool tournamentActive = false;
        private Dictionary<Pawn, List<TournamentMatchResult>> fighterResults = new Dictionary<Pawn, List<TournamentMatchResult>>();
        private int ticksBetweenFights = 30000;


        private List<string> opponentNames = new List<string>
        {
            "Pikkon", "Olibu", "Maraikoh", "Tapkar", "Torbie", "Froug",
            "Arqua", "Giran", "Nam", "Pamput", "Ranfan", "Bacterian"
        };

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref colonistsReturnSignal, "colonistsReturnSignal");
            Scribe_Values.Look(ref luckFactor, "luckFactor", 0.2f);
            Scribe_Values.Look(ref currentDay, "currentDay");
            Scribe_Values.Look(ref tournamentDurationTicks, "tournamentDurationTicks");
            Scribe_Values.Look(ref ticksUntilNextUpdate, "ticksUntilNextUpdate");
            Scribe_Values.Look(ref tournamentActive, "tournamentActive");
            Scribe_Values.Look(ref maxFights, "maxFights", 3);
            Scribe_Collections.Look(ref fighterResults, "fighterResults", LookMode.Reference, LookMode.Deep, ref pawnKeys, ref matchResultValues);
        }

        private List<Pawn> pawnKeys;
        private List<List<TournamentMatchResult>> matchResultValues;



        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);

            //Log.Message($"[DBG] Tournament received signal: {signal.tag}");
            //Log.Message($"[DBG] Comparing against - Enable: {inSignalEnable}, Return: {colonistsReturnSignal}");

            if (signal.tag == QuestGenUtility.HardcodedSignalWithQuestID("SkipToFight"))
            {
                Log.Message("[DBG] Skipping to next fight");
                SkipToNextFight();
            }
            else if (signal.tag == inSignalEnable)
            {
                Log.Message("[DBG] Starting tournament");
                StartTournament();
            }
            else if (signal.tag == colonistsReturnSignal)
            {
                Log.Message("[DBG] Finishing tournament");
                FinishTournament();
            }
        }


        private void StartTournament()
        {
            var lendPart = quest.PartsListForReading.OfType<QuestPart_LendColonistsToFaction>().FirstOrDefault();
            if (lendPart?.LentColonistsListForReading != null)
            {
                foreach (Pawn pawn in lendPart.LentColonistsListForReading)
                {
                    fighterResults[pawn] = new List<TournamentMatchResult>();
                }

                tournamentActive = true;
                ticksUntilNextUpdate = ticksBetweenFights;
            }
        }


        private void SkipToNextFight()
        {
            if (!tournamentActive)
            {
                Log.Warning("Attempted to skip fight while tournament is not active");
                return;
            }

            if (currentDay >= maxFights)
            {
                Log.Warning("Attempted to skip fight when maximum fights reached");
                return;
            }

            Log.Message($"[DBG] Forcing next fight. Current day: {currentDay}, Max fights: {maxFights}");
            ticksUntilNextUpdate = 10;
        }

        private void FinishTournament()
        {
            tournamentActive = false;

            while (currentDay < maxFights)
            {
                currentDay++;
                SimulateDailyMatches();
            }


            HandleTournamentConclusion();
            this.Complete();
        }


        public override void QuestPartTick()
        {
            base.QuestPartTick();

            if (!tournamentActive) return;

            ticksUntilNextUpdate--;
            if (ticksUntilNextUpdate <= 0)
            {
                currentDay++;
                SimulateDailyMatches();

                // Check if we've reached max fights
                if (currentDay >= maxFights)
                {
                    tournamentActive = false;
                    HandleTournamentConclusion();
                }
                else
                {
                    ticksUntilNextUpdate = ticksBetweenFights;
                }
            }
        }

        private void SimulateDailyMatches()
        {
            foreach (var fighterEntry in fighterResults)
            {
                Pawn fighter = fighterEntry.Key;
                if (!fighter.TryGetKiAbilityClass(out AbilityClassKI abilityClassKI))
                    continue;

                // Generate match data
                string opponentName = opponentNames.RandomElement();
                Dictionary<string, float> fighterScoreContributions = new Dictionary<string, float>();
                float fighterScore = CalculateFighterScore(fighter, abilityClassKI, fighterScoreContributions);
                float opponentScore = GenerateOpponentScore(fighterScore);

                // Apply luck modifier
                float luckModifier = Rand.Range(-luckFactor, luckFactor);
                float luckImpact = fighterScore * luckModifier;
                fighterScore += luckImpact;
                fighterScoreContributions["Luck Factor"] = luckImpact;

                bool victory = fighterScore > opponentScore;
                float scoreMargin = Mathf.Abs(fighterScore - opponentScore) / fighterScore;

                // Record match result
                TournamentMatchResult matchResult = new TournamentMatchResult
                {
                    Victory = victory,
                    ScoreMargin = scoreMargin,
                    OpponentName = opponentName,
                    ExperienceGained = victory ? 200f * (1f + scoreMargin) : 100f * (0.5f + scoreMargin),
                    Round = currentDay,
                    TournamentID = quest.id,
                    ScoreContributions = fighterScoreContributions,
                    MatchID = $"match_{quest.id}_{currentDay}_{fighter.ThingID}"
                };

                fighterEntry.Value.Add(matchResult);

                // Send match notification
                string matchOutcome = victory ? "won against" : "lost to";
                Messages.Message($"Tournament Day {currentDay}: {fighter.LabelShort} {matchOutcome} {opponentName}! (Score: {fighterScore:F0} vs {opponentScore:F0})",
                    victory ? MessageTypeDefOf.PositiveEvent : MessageTypeDefOf.NeutralEvent);

                // Grant XP
                abilityClassKI.GainXP(matchResult.ExperienceGained);
            }
        }

        //class FighterData
        //{
        //    public float 
        //}

        private void HandleTournamentConclusion()
        {
            var tracker = Current.Game.GetComponent<TournamentTracker>();
            bool anyPlayerPawnWon = false;

            foreach (var fighterEntry in fighterResults)
            {
                Pawn fighter = fighterEntry.Key;
                var results = fighterEntry.Value;

                // Calculate overall performance
                int victories = results.Count(r => r.Victory);
                float avgMargin = results.Average(r => r.ScoreMargin);
                float totalXP = results.Sum(r => r.ExperienceGained);

                // Generate final rewards based on overall performance
                int dragonBallsWon = 0;
                int goldWon = 0;

                if (victories > results.Count / 2) // Won majority of matches
                {
                    PlayerPawnWonTournament(fighter, victories);
                    anyPlayerPawnWon = true;
                }
                else
                {
                    PlayerPawnsLost(fighter, victories); 
                }

                // Record tournament results
                tracker.RecordTournamentResults(fighter, results, dragonBallsWon, goldWon);
            }
        }


        private void PlayerPawnWonTournament(Pawn fighter, int victories)
        {
            if (fighter.TryGetKiAbilityClass(out AbilityClassKI abilityClassKI))
            {
                abilityClassKI.GainXP(this.tournamentWinXP);
            }

            GenerateRewards(fighter);
            Messages.Message($"{fighter.LabelShort} won the tournament with {victories} victories!", MessageTypeDefOf.PositiveEvent);
        }

        private void PlayerPawnsLost(Pawn fighter, int victories)
        {
            if (fighter.TryGetKiAbilityClass(out AbilityClassKI abilityClassKI))
            {
                abilityClassKI.GainXP(this.tournamentLoseXP);
            }


            Messages.Message($"{fighter.LabelShort} finished the tournament with {victories} victories, they did not win.", MessageTypeDefOf.NeutralEvent);
        }

        // Rest of the methods remain the same (CalculateFighterScore, GenerateOpponentScore, GenerateRewards)
        private float CalculateFighterScore(Pawn pawn, AbilityClassKI abilityClassKI, Dictionary<string, float> scoreContributions)
        {
            float score = 0f;

            // Melee skill contribution
            float meleeScore = pawn.skills.GetSkill(SkillDefOf.Melee).Level * 2f;
            score += meleeScore;
            scoreContributions["Melee Skill"] = meleeScore;


            float meleePassionScore = 0;

            switch (pawn.skills.GetSkill(SkillDefOf.Melee).passion)
            {
                case Passion.None:
                    meleePassionScore = 0;
                    break;
                case Passion.Minor:
                    meleePassionScore = 25f;
                    break;
                case Passion.Major:
                    meleePassionScore = 50f;
                    break;
            }

            score += meleePassionScore;
            scoreContributions["Melee Passion"] = meleePassionScore;


            float shootingScore = pawn.skills.GetSkill(SkillDefOf.Shooting).Level * 1.5f;
            score += shootingScore;
            scoreContributions["Shooting Skill"] = shootingScore;


            float movementScore = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Moving) * 10f;
            score += movementScore;
            scoreContributions["Movement"] = movementScore;


            float manipulationScore = pawn.health.capacities.GetLevel(PawnCapacityDefOf.Manipulation) * 8f;
            score += manipulationScore;
            scoreContributions["Manipulation"] = manipulationScore;

            float kiScore = abilityClassKI.abilityResource.MaxEnergy * 15f;
            score += kiScore;
            scoreContributions["Ki Level"] = kiScore;

            if (abilityClassKI.Learned(SR_DefOf.SR_SuperSaiyan))
            {
                float ssScore = 30f + abilityClassKI.GetLearnedAbility(SR_DefOf.SR_SuperSaiyan).LevelHumanReadable;
                score += ssScore;
                scoreContributions["Super Saiyan"] = ssScore;
            }

            if (abilityClassKI.Learned(DBDefOf.SR_SuperSaiyan2))
            {
                float ssScore = 60f + abilityClassKI.GetLearnedAbility(DBDefOf.SR_SuperSaiyan2).LevelHumanReadable;
                score += ssScore;
                scoreContributions["Super Saiyan 2"] = ssScore;
            }

            if (abilityClassKI.Learned(DBDefOf.SR_KaioKen))
            {
                float kaiokenScore = 10f + abilityClassKI.GetLearnedAbility(DBDefOf.SR_KaioKen).LevelHumanReadable;
                score += kaiokenScore;
                scoreContributions["Kaio-Ken"] = kaiokenScore;
            }

            return score;
        }

        private float GenerateOpponentScore(float playerScore)
        {
            // Generate base opponent score
            float difficultyVariance = Rand.Range(0.8f, 1.2f);
            float baseScore = playerScore * difficultyVariance;

            // Add some randomness to make fights more interesting
            float randomVariance = Rand.Range(-playerScore * 0.1f, playerScore * 0.1f);

            return baseScore + randomVariance;
        }

        private void GenerateRewards(Pawn pawn)
        {
            Thing dragonBall = ThingMaker.MakeThing(DefDatabase<ThingDef>.AllDefsListForReading.Where(x => x.IsWithinCategory(DBDefOf.DragonBallsCategory)).RandomElement());
            pawn.inventory.TryAddAndUnforbid(dragonBall);
            Messages.Message($"{pawn.LabelShort} won a Dragon Ball in the tournament!", MessageTypeDefOf.PositiveEvent);

            // Gold reward
            int goldAmount = UnityEngine.Random.Range(0, 700);
            if (goldAmount > 0)
            {
                Thing gold = ThingMaker.MakeThing(ThingDefOf.Gold);
                gold.stackCount = goldAmount;
                pawn.inventory.TryAddAndUnforbid(gold);
                Messages.Message($"{pawn.LabelShort} won {goldAmount} gold in the tournament!", MessageTypeDefOf.PositiveEvent);
            }
        }
    }
}
