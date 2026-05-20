using RimWorld;
using UnityEngine;
using Verse;

namespace Gloomylynx
{
	/// <summary>
	/// Vanilla <see cref="Building_Heater"/> only resolves the room at <see cref="Thing.Position"/>.
	/// Multi-tile fireplaces (2x1, wall edge) often have no enclosed room on that cell,
	/// so <see cref="GenTemperature.ControlTemperatureTempChange"/> returns 0 while temp gizmos still work.
	/// </summary>
	public class Building_GloomyHeater : Building_Heater
	{
		private bool TryGetIndoorRoom(out Room room, out IntVec3 tempControlCell)
		{
			room = null;
			tempControlCell = Position;
			Map map = Map;
			if (map == null)
			{
				return false;
			}

			foreach (IntVec3 c in this.OccupiedRect())
			{
				if (TryCell(c, map, ref room, ref tempControlCell))
				{
					return true;
				}
			}

			CellRect occupied = this.OccupiedRect();
			foreach (IntVec3 c in GenAdj.CellsAdjacent8Way(Position, Rotation, def.size))
			{
				if (!c.InBounds(map) || occupied.Contains(c))
				{
					continue;
				}
				if (TryCell(c, map, ref room, ref tempControlCell))
				{
					return true;
				}
			}

			return false;
		}

		private static bool TryCell(IntVec3 c, Map map, ref Room room, ref IntVec3 tempControlCell)
		{
			Room r = c.GetRoom(map);
			if (r == null || r.UsesOutdoorTemperature)
			{
				return false;
			}
			room = r;
			tempControlCell = c;
			return true;
		}

		public override void TickRare()
		{
			if (!Spawned)
			{
				return;
			}
			if (compPowerTrader == null || !compPowerTrader.PowerOn)
			{
				return;
			}
			if (compTempControl == null)
			{
				return;
			}
			if (!TryGetIndoorRoom(out Room room, out IntVec3 tempControlCell))
			{
				compTempControl.operatingAtHighPower = false;
				return;
			}

			float ambientTemperature = room.Temperature;
			float efficiency;
			if (ambientTemperature < 20f)
			{
				efficiency = 1f;
			}
			else if (ambientTemperature > 120f)
			{
				efficiency = 0f;
			}
			else
			{
				efficiency = Mathf.InverseLerp(120f, 20f, ambientTemperature);
			}

			float energyLimit = compTempControl.Props.energyPerSecond * efficiency * 4.1666665f;
			float tempChange = GenTemperature.ControlTemperatureTempChange(tempControlCell, Map, energyLimit, compTempControl.TargetTemperature);
			bool atHighPower = !Mathf.Approximately(tempChange, 0f);
			CompProperties_Power powerProps = compPowerTrader.Props;
			if (atHighPower)
			{
				room.Temperature += tempChange;
				compPowerTrader.PowerOutput = -powerProps.PowerConsumption;
			}
			else
			{
				compPowerTrader.PowerOutput = -powerProps.PowerConsumption * compTempControl.Props.lowPowerConsumptionFactor;
			}
			compTempControl.operatingAtHighPower = atHighPower;
		}
	}
}
