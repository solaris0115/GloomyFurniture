using RimWorld;
using Verse;

namespace Gloomylynx
{
	public class PlaceWorker_PassableObject : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			if (GenGrid.Impassable(loc, map))
			{
				return Translator.Translate("PassableObject_CantPass");
			}
			return true;
		}
	}

	public class PlaceWorker_StandableObject : PlaceWorker
	{
		public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map, Thing thingToIgnore = null, Thing thing = null)
		{
			if (!GenGrid.Standable(loc, map))
			{
				return Translator.Translate("StandableObject_CantPass");
			}
			return true;
		}
	}
}
