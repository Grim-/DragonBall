using RimWorld;
using RimWorld.QuestGen;
using SaiyanMod;
using System.Collections.Generic;
using System.Linq;
using TaranMagicFramework;
using Verse;

namespace DragonBall
{
    public class AbilityOption : IExposable
    {
        public TaranMagicFramework.AbilityDef ability;
        public float weight = 1f;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref ability, "ability");
            Scribe_Values.Look(ref weight, "weight");
        }
    }

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

    public class QuestNode_LearnFromKingKai : QuestNode
    {
        public SlateRef<float> BaseXPGain;
        public List<AbilityOption> abilities;

        public SlateRef<bool> halfSaiyansUnlockPotential = true;

        public SlateRef<string> halfSaiyanAbilityUnlock;
        public SlateRef<string> humanAbilityUnlock;

        public SlateRef<bool> nonKiHumansGainKi = true;
        public SlateRef<string> kiUserGeneToGain;

        public SlateRef<bool> kiHumansGainXP = true;

        public SlateRef<bool> saiyansLearnSuperSaiyanORSuperSaiyanTwo = true;

        protected override bool TestRunInt(Slate slate)
        {
            return true;
        }

        protected override void RunInt()
        {
            Slate slate = QuestGen.slate;

            QuestGen.quest.AddPart(new QuestPart_SupremeKaiTraining
            {
                inSignalEnable = QuestGenUtility.HardcodedSignalWithQuestID("ColonistsReturned"),
                BaseXPGain = BaseXPGain.GetValue(slate),
                halfSaiyansUnlockPotential = halfSaiyansUnlockPotential.GetValue(slate),
                halfSaiyanAbilityUnlock = DefDatabase<TaranMagicFramework.AbilityDef>.GetNamed(halfSaiyanAbilityUnlock.GetValue(slate)),

            });
        }
    }
    public class QuestPart_SupremeKaiTraining : QuestPart
    {
        public string inSignalEnable;
        public float BaseXPGain = 25000f;


        public bool halfSaiyansUnlockPotential = true;

        public TaranMagicFramework.AbilityDef halfSaiyanAbilityUnlock;
        public TaranMagicFramework.AbilityDef humanAbilityUnlock;

        public bool nonKiHumansGainKi = true;
        public GeneDef kiUserGeneToGain;

        public bool kiHumansGainXP = true;

        public bool saiyansLearnSuperSaiyanORSuperSaiyanTwo = true;

        public override void Notify_QuestSignalReceived(Signal signal)
        {
            base.Notify_QuestSignalReceived(signal);
            if (signal.tag == inSignalEnable)
            {

                Log.Message("Supreme Kai Training Signal REceived");

                var lendPart = quest.PartsListForReading.OfType<QuestPart_LendColonistsToFaction>().FirstOrDefault();
                if (lendPart?.LentColonistsListForReading != null)
                {
                    foreach (Pawn pawn in lendPart.LentColonistsListForReading)
                    {
                        HandleClothes(pawn);
                        GiveItems(pawn);
                        HandleLevelUp(pawn);
                    }
                }
            }
        }

        private void HandleClothes(Pawn pawn)
        {
            // Handle apparel changes
            if (pawn.apparel != null)
            {
                var torsoApparel = pawn.apparel.WornApparel
                    .FirstOrDefault(x => x.def.apparel.layers.Contains(ApparelLayerDefOf.Shell));

                if (torsoApparel != null)
                {
                    pawn.apparel.Remove(torsoApparel);
                    pawn.inventory.innerContainer.TryAdd(torsoApparel);
                }

                ThingDef kaiRobes = DefDatabase<ThingDef>.GetNamed("SR_KaiRobes");
                if (kaiRobes != null)
                {
                    Apparel robes = (Apparel)ThingMaker.MakeThing(kaiRobes);
                    pawn.apparel.Wear(robes);
                }
            }
        }

        private void GiveItems(Pawn pawn)
        {
            ThingDef zSword = DefDatabase<ThingDef>.GetNamed("SR_ZSword");
            if (zSword != null)
            {
                Thing sword = ThingMaker.MakeThing(zSword);
                pawn.inventory.innerContainer.TryAdd(sword);
            }
        }


        private void HandleLevelUp(Pawn pawn)
        {
            if (pawn.TryGetKiAbilityClass(out AbilityClassKI abilityClassKI))
            {
                abilityClassKI.GainXP(BaseXPGain);

                if (abilityClassKI.def == DBDefOf.SR_HalfSaiyan)
                {
                    TaranMagicFramework.Ability potential = abilityClassKI.GetLearnedAbility(halfSaiyanAbilityUnlock);
                    if (potential != null)
                    {
                        potential.ChangeLevel(potential.def.abilityTiers.Count - 1);
                    }
                }
                else if (abilityClassKI.def == DBDefOf.SR_Human)
                {
                    TaranMagicFramework.Ability awakening = abilityClassKI.GetLearnedAbility(humanAbilityUnlock);
                    if (awakening != null)
                    {
                        awakening.ChangeLevel(awakening.def.abilityTiers.Count - 1);
                    }
                }
                else if(abilityClassKI.def == DBDefOf.SR_RegularSaiyan)
                {
                    TaranMagicFramework.Ability superSaiyan = abilityClassKI.GetLearnedAbility(SR_DefOf.SR_SuperSaiyan);
                    if (superSaiyan != null)
                    {
                        if (!superSaiyan.FullyLearned)
                        {
                            superSaiyan.ChangeLevel(superSaiyan.def.abilityTiers.Count - 1);
                        }
                        else
                        {
                            TaranMagicFramework.Ability ss2 = abilityClassKI.GetLearnedAbility(DBDefOf.SR_SuperSaiyan2);

                            if (ss2 != null)
                            {
                                ss2.ChangeLevel(ss2.def.abilityTiers.Count - 1);
                            }
                            else
                            {
                                abilityClassKI.LearnAbility(SR_DefOf.SR_SuperSaiyan, false, 1);
                            }
                        }
                    }
                }
                else if(abilityClassKI.def == DBDefOf.SR_LegendarySaiyan)
                {

                }
            }
            else
            {
                if (!pawn.genes.HasActiveGene(SR_DefOf.SR_KiUserGene))
                {
                    pawn.genes.AddGene(SR_DefOf.SR_KiUserGene, false);
                }
                if (pawn.TryGetKiAbilityClass(out AbilityClassKI newAbilityClassKI))
                {
                    newAbilityClassKI.GainXP(BaseXPGain);
                }
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref inSignalEnable, "inSignalEnable");
            Scribe_Values.Look(ref BaseXPGain, "xpAmount");
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
