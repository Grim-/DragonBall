using RimWorld;
using RimWorld.QuestGen;
using SaiyanMod;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace DragonBall
{
    public class QuestNode_LearnFromSupremeKai : QuestNode
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
                humanAbilityUnlock = DefDatabase<TaranMagicFramework.AbilityDef>.GetNamed(humanAbilityUnlock.GetValue(slate)),
                nonKiHumansGainKi = nonKiHumansGainKi.GetValue(slate),
                kiHumansGainXP = kiHumansGainXP.GetValue(slate),
                saiyansLearnSuperSaiyanORSuperSaiyanTwo = saiyansLearnSuperSaiyanORSuperSaiyanTwo.GetValue(slate)

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
            ThingDef zSword = DefDatabase<ThingDef>.GetNamed("SR_Zsword");
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
                Log.Message("Supreme Kai Training : Gaining XP");
                abilityClassKI.GainXP(BaseXPGain);

                if (abilityClassKI.def == DBDefOf.SR_HalfSaiyan)
                {
                    Log.Message("Supreme Kai Training : Is Half Saiyan");
                    TaranMagicFramework.Ability potential = abilityClassKI.GetLearnedAbility(halfSaiyanAbilityUnlock);
                    if (potential != null)
                    {
                        potential.ChangeLevel(potential.def.abilityTiers.Count - 1);
                    }
                }
                else if (abilityClassKI.def == DBDefOf.SR_Human)
                {
                    Log.Message("Supreme Kai Training : Is Human");
                    TaranMagicFramework.Ability awakening = abilityClassKI.GetLearnedAbility(humanAbilityUnlock);
                    if (awakening != null)
                    {
                        awakening.ChangeLevel(awakening.def.abilityTiers.Count - 1);
                    }
                }
                else if (abilityClassKI.def == DBDefOf.SR_RegularSaiyan)
                {
                    Log.Message("Supreme Kai Training : Is Saiyan");

                    //if they dont have super saiyan then grant it
                    if (!abilityClassKI.Learned(SR_DefOf.SR_SuperSaiyan))
                    {
                        Log.Message("Supreme Kai Training : does not know Super saiyan, learning..");

                        abilityClassKI.LearnAbility(SR_DefOf.SR_SuperSaiyan, false, 0);
                    }
                    else
                    {

                        Log.Message("Supreme Kai Training : Does know Super saiyan, but is not maxed upgrading skill..");
                        TaranMagicFramework.Ability superSaiyan = abilityClassKI.GetLearnedAbility(SR_DefOf.SR_SuperSaiyan);

                        if (superSaiyan != null && !superSaiyan.FullyLearned)
                        {
                            if (superSaiyan != null)
                            {
                                superSaiyan.LevelToMax();
                            }
                        }
                        else
                        {

                            Log.Message("Supreme Kai Training : Does know Super saiyan and is fully learnt");
                            TaranMagicFramework.Ability ss2 = abilityClassKI.GetLearnedAbility(DBDefOf.SR_SuperSaiyan2);
                            if (ss2 != null)
                            {
                                Log.Message("Supreme Kai Training : Does know Super saiyan 2 and is NOT fully learnt, upgrading...");
                                ss2.LevelToMax();
                            }
                            else
                            {
                                Log.Message("Supreme Kai Training : Does NOT know Super saiyan 2 learning");
                                abilityClassKI.LearnAbility(DBDefOf.SR_SuperSaiyan2, false, 1);
                            }


                        }
                    }
                }
                else if (abilityClassKI.def == DBDefOf.SR_LegendarySaiyan)
                {
                    Log.Message("Supreme Kai Training : Is Legendary Saiyan");

                    if (!abilityClassKI.Learned(SR_DefOf.SR_LegendarySuperSaiyan))
                    {
                        Log.Message("Supreme Kai Training : does not know legendary Super saiyan, learning..");
                        abilityClassKI.LearnAbility(SR_DefOf.SR_LegendarySuperSaiyan, false, 1);
                    }
                }
            }
            else
            {
                Log.Message("Supreme Kai Training : Is Human non ki-user");

                if (!pawn.genes.HasActiveGene(SR_DefOf.SR_KiUserGene))
                {
                    Log.Message("Supreme Kai Training : Human given Ki Gene");

                    pawn.genes.AddGene(SR_DefOf.SR_KiUserGene, false);
                }

                if (pawn.TryGetKiAbilityClass(out AbilityClassKI newAbilityClassKI))
                {
                    Log.Message("Supreme Kai Training : Human granted XP.");
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
}
