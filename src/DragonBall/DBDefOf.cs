using RimWorld;
using SaiyanMod;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace DragonBall
{
    [DefOf]
    public class DBDefOf
    {
        public static ThingDef DragonBallOneStar;
        public static ThingDef DragonBallTwoStar;
        public static ThingDef DragonBallThreeStar;
        public static ThingDef DragonBallFourStar;
        public static ThingDef DragonBallFiveStar;
        public static ThingDef DragonBallSixStar;
        public static ThingDef DragonBallSevenStar;

        public static ThingDef DragonBallRitualSpot;

        public static ThingDef DragonBallFlying;

        public static JobDef PlaceDragonBall;
        public static JobDef GatherDragonBalls;
        public static JobDef DragonBallSummoning;

        public static ThingCategoryDef DragonBallsCategory;

        public static TaranMagicFramework.AbilityClassDef SR_Human;
        public static TaranMagicFramework.AbilityClassDef SR_RegularSaiyan;
        public static TaranMagicFramework.AbilityClassDef SR_LegendarySaiyan;
        public static TaranMagicFramework.AbilityClassDef SR_HalfSaiyan;


        public static TaranMagicFramework.AbilityDef SR_SuperSaiyan2;
        public static TaranMagicFramework.AbilityDef SR_KaioKen;
    }


    public class KIAbilityKaioKen : KIAbility_SuperSaiyan
    {
        protected override Color TransformedColor => Color.red;
    }
}
