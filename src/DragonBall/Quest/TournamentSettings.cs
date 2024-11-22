using System.Collections.Generic;
using Verse;

namespace DragonBall
{
    public class TournamentSettings : Def
    {
        // Base stat multipliers
        public float meleeSkillMultiplier = 2f;
        public float shootingSkillMultiplier = 1.5f;
        public float movementMultiplier = 10f;
        public float manipulationMultiplier = 8f;
        public float kiLevelMultiplier = 15f;

        // Ability bonuses
        public List<AbilityScoreBonus> abilityBonuses;

        // Reward settings
        public float baseXpGainWin = 1000f;
        public float baseXpGainLoss = 500f;
        public float baseGoldReward = 1000f;
        public float dragonBallBaseChance = 0.1f;

        // Tournament balance
        public float minDifficultyVariance = 0.8f;
        public float maxDifficultyVariance = 1.2f;
    }
}
