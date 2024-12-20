﻿using RimWorld;
using RimWorld.QuestGen;
using SaiyanMod;
using System.Collections.Generic;
using System.Linq;
using TaranMagicFramework;
using Verse;

namespace DragonBall
{
    public class QuestNode_LearnAbility : QuestNode
    {
        public SlateRef<float> xpChance;
        public SlateRef<float> xpAmount;
        public List<AbilityOption> abilities;

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;

            QuestGen.quest.AddPart(new QuestPart_TrainColonist
            {
                inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID("ColonistsReturned"),
                xpChance = xpChance.GetValue(slate),
                xpAmount = xpAmount.GetValue(slate),
                abilities = abilities
            });
        }
    }


    public class QuestPart_TrainColonist : QuestPart
    {
        public string inSignalEnable;
        public float xpChance;
        public float xpAmount;
        public List<AbilityOption> abilities;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);

            if (signal.tag == inSignalEnable && quest?.PartsListForReading != null)
            {
                var lendPart = quest.PartsListForReading.OfType<QuestPart_LendColonistsToFaction>().FirstOrDefault();
                if (lendPart?.LentColonistsListForReading != null)
                {
                    foreach (Pawn pawn in lendPart.LentColonistsListForReading)
                    {

                        if (pawn.TryGetKiAbilityClass(out AbilityClassKI abilityClassKI))
                        {
                            if (Rand.Chance(xpChance))
                            {
                                abilityClassKI.GainXP(xpAmount);
                                Messages.Message($"{pawn.LabelShort} gained {xpAmount} experience!", MessageTypeDefOf.PositiveEvent);
                                continue;
                            }

                            var availableAbilities = abilities.Where(a => !abilityClassKI.Learned(a.ability)).ToList();
                            if (!availableAbilities.Any())
                            {
                                abilityClassKI.GainXP(xpAmount);
                                continue;
                            }

                            float totalWeight = availableAbilities.Sum(a => a.weight);
                            float roll = Rand.Range(0f, totalWeight);
                            float currentWeight = 0f;

                            foreach (var option in availableAbilities)
                            {
                                currentWeight += option.weight;
                                if (roll <= currentWeight)
                                {
                                    abilityClassKI.LearnAbility(option.ability, false, option.ability.abilityTiers.Count - 1);
                                    Messages.Message($"{pawn.LabelShort} learned {option.ability.label}!", MessageTypeDefOf.PositiveEvent);
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignalEnable, "inSignalEnable");
            Scribe_Values.Look(ref xpChance, "xpChance");
            Scribe_Values.Look(ref xpAmount, "xpAmount");
            Scribe_Collections.Look(ref abilities, "abilities", LookMode.Deep);
        }
    }


}
