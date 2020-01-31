using BattleTech;
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using Harmony;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TheEngineInitiative
{
	internal class ModSettings
	{
		public float MechPhaseSpecialMinSpeed = 209.0F;
		public float MechPhaseLightMinSpeed = 164.0F;
		public float MechPhaseMediumMinSpeed = 139.0F;
		public float MechPhaseHeavyMinSpeed = 119.0F;

		public float VehiclePhaseSpecialMinSpeed = 229.0F;
		public float VehiclePhaseLightMinSpeed = 190.0F;
		public float VehiclePhaseMediumMinSpeed = 164.0F;
		public float VehiclePhaseHeavyMinSpeed = 139.0F;
	}
	public static class TheEngineInitiative
	{
		// create variable to store our settings
		internal static ModSettings Settings = new ModSettings();

		public static void Init(string directory, string settingsJSON)
		{
			var harmony = HarmonyInstance.Create("TheEngineInitiative");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			// read settings
			try
			{
				Settings = JsonConvert.DeserializeObject<ModSettings>(File.ReadAllText(Path.Combine(directory, "settings.json")));
			}
			catch (Exception)  //if it fails, use defaults.
			{
				Settings = new ModSettings();
			}
		}
	}

	[HarmonyPatch(typeof(Mech), "InitStats")]
	public static class Mech_InitStats_Patch
	{
		public static void Postfix(Mech __instance)
		{
			if (!__instance.Combat.IsLoadingFromSave)
			{
				// to account for Master Tactician, need to check if initiative differs from weight based initiative
				WeightClass weightClass = __instance.MechDef.Chassis.weightClass;
				int i = __instance.StatCollection.GetValue<int>("BaseInitiative");
				int i_phaseModifier = 0;
				if (weightClass == WeightClass.LIGHT && i != __instance.Combat.Constants.Phase.PhaseLight)
					i_phaseModifier = i - __instance.Combat.Constants.Phase.PhaseLight;
				else if (weightClass == WeightClass.MEDIUM && i != __instance.Combat.Constants.Phase.PhaseMedium)
					i_phaseModifier = i - __instance.Combat.Constants.Phase.PhaseMedium;
				else if (weightClass == WeightClass.HEAVY && i != __instance.Combat.Constants.Phase.PhaseHeavy)
					i_phaseModifier = i - __instance.Combat.Constants.Phase.PhaseHeavy;
				else if	(weightClass == WeightClass.ASSAULT && i != __instance.Combat.Constants.Phase.PhaseAssault)
					i_phaseModifier = i - __instance.Combat.Constants.Phase.PhaseAssault;

				float f_walkSpeed = __instance.MovementCaps.MaxWalkDistance;
				if (f_walkSpeed >= TheEngineInitiative.Settings.MechPhaseSpecialMinSpeed)
				{
					__instance.Initiative = __instance.Combat.Constants.Phase.PhaseSpecial + i_phaseModifier;
				}
				else if (f_walkSpeed >= TheEngineInitiative.Settings.MechPhaseLightMinSpeed)
				{
					__instance.Initiative = __instance.Combat.Constants.Phase.PhaseLight + i_phaseModifier;
				}
				else if (f_walkSpeed >= TheEngineInitiative.Settings.MechPhaseMediumMinSpeed)
				{
					__instance.Initiative = __instance.Combat.Constants.Phase.PhaseMedium + i_phaseModifier;
				}
				else if (f_walkSpeed >= TheEngineInitiative.Settings.MechPhaseHeavyMinSpeed)
				{
					__instance.Initiative = __instance.Combat.Constants.Phase.PhaseHeavy + i_phaseModifier;
				}
				else
				{
					__instance.Initiative = __instance.Combat.Constants.Phase.PhaseAssault + i_phaseModifier;
				}

				// now wipe out the old BaseInitiative and add the new one.
				if (__instance.StatCollection.RemoveStatistic("BaseInitiative"))
				{
					__instance.StatCollection.AddStatistic<int>("BaseInitiative", __instance.Initiative);
				}
			}
		}
	}

	[HarmonyPatch(typeof(Vehicle), "InitStats")]
	public static class Vehicle_InitStats_Patch
	{
		public static void Postfix(Vehicle __instance)
		{
			if (!__instance.Combat.IsLoadingFromSave)
			{
				// to account for Master Tactician, need to check if initiative differs from weight based initiative
				WeightClass weightClass = __instance.VehicleDef.Chassis.weightClass;
				int i = __instance.StatCollection.GetValue<int>("BaseInitiative");
				int i_phaseModifier = 0;
				if (weightClass == WeightClass.LIGHT && i != __instance.Combat.Constants.Phase.PhaseLightVehicle)
					i_phaseModifier = i - __instance.Combat.Constants.Phase.PhaseLightVehicle;
				else if (weightClass == WeightClass.MEDIUM && i != __instance.Combat.Constants.Phase.PhaseMediumVehicle)
					i_phaseModifier = i - __instance.Combat.Constants.Phase.PhaseMediumVehicle;
				else if (weightClass == WeightClass.HEAVY && i != __instance.Combat.Constants.Phase.PhaseHeavyVehicle)
					i_phaseModifier = i - __instance.Combat.Constants.Phase.PhaseHeavyVehicle;
				else if (weightClass == WeightClass.ASSAULT && i != __instance.Combat.Constants.Phase.PhaseAssaultVehicle)
					i_phaseModifier = i - __instance.Combat.Constants.Phase.PhaseAssaultVehicle;

				float f_walkSpeed = __instance.MovementCaps.MaxWalkDistance;
				if (f_walkSpeed >= TheEngineInitiative.Settings.VehiclePhaseSpecialMinSpeed)
				{
					__instance.Initiative = __instance.Combat.Constants.Phase.PhaseSpecial + i_phaseModifier;
				}
				else if (f_walkSpeed >= TheEngineInitiative.Settings.VehiclePhaseLightMinSpeed)
				{
					__instance.Initiative = __instance.Combat.Constants.Phase.PhaseLight + i_phaseModifier;
				}
				else if (f_walkSpeed >= TheEngineInitiative.Settings.VehiclePhaseMediumMinSpeed)
				{
					__instance.Initiative = __instance.Combat.Constants.Phase.PhaseMedium + i_phaseModifier;
				}
				else if (f_walkSpeed >= TheEngineInitiative.Settings.VehiclePhaseHeavyMinSpeed)
				{
					__instance.Initiative = __instance.Combat.Constants.Phase.PhaseHeavy + i_phaseModifier;
				}
				else
				{
					__instance.Initiative = __instance.Combat.Constants.Phase.PhaseAssault + i_phaseModifier;
				}

				// now wipe out the old BaseInitiative and add the new one.
				if (__instance.StatCollection.RemoveStatistic("BaseInitiative"))
				{
					__instance.StatCollection.AddStatistic<int>("BaseInitiative", __instance.Initiative);
				}
			}
		}
	}

	[HarmonyPatch(typeof(MechBayMechInfoWidget), "SetInitiative")]
	[HarmonyPatch(new Type[] { })]
	public static class MechBayMechInfoWidget_SetInitiative_Patch
	{
		// pass in a bunch of ___ variables so we can get access to private members of MechBayMechInfoObject.
		// And we'll return false so that the original function doesn't get called.
		public static bool Prefix(MechBayMechInfoWidget __instance, MechDef ___selectedMech,
			GameObject ___initiativeObj, TextMeshProUGUI ___initiativeText, HBSTooltip ___initiativeTooltip)
		{
			if (___initiativeObj == null || ___initiativeText == null)
			{
				return false;
			}
			if (___selectedMech == null)
			{
				___initiativeObj.SetActive(false);
				return false;
			}

			___initiativeObj.SetActive(true);
			int num = 1; // default to assault phase

			float f_walkSpeed = ___selectedMech.Chassis.MovementCapDef.MaxWalkDistance;
			if (f_walkSpeed >= TheEngineInitiative.Settings.MechPhaseSpecialMinSpeed)
			{
				num = 5;  //special phase
			}
			else if (f_walkSpeed >= TheEngineInitiative.Settings.MechPhaseLightMinSpeed)
			{
				num = 4;  //light phase
			}
			else if (f_walkSpeed >= TheEngineInitiative.Settings.MechPhaseMediumMinSpeed)
			{
				num = 3;  //medium phase
			}
			else if (f_walkSpeed >= TheEngineInitiative.Settings.MechPhaseHeavyMinSpeed)
			{
				num = 2;  //heavy phase
			}

			___initiativeText.SetText($"{num}");
			if (___initiativeTooltip != null)
			{
				// build the tooltip.  Going to use the mech's name and tell where its speed puts it, initiative-wise.
				string tooltipTitle = $"{___selectedMech.Name}";
				string tooltipText = "A max walking speed of " + $"{f_walkSpeed}" +  "m per turn means this mech moves in initiative phase " + $"{num}" + ".";
				BaseDescriptionDef initiativeData = new BaseDescriptionDef("MB_MIW_MECH_TT", tooltipTitle, tooltipText, null);
				___initiativeTooltip.enabled = true;
				___initiativeTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(initiativeData));
			}
			return false;
		}

	}

	// Now to set the initiative for the lance screen. 
	[HarmonyPatch(typeof(LanceLoadoutSlot), "RefreshInitiativeData")]
	[HarmonyPatch(new Type[] { })]
	public static class LanceLoadoutSlot_RefreshInitiativeData
	{
		// return false so original function does not get called.
		public static bool Prefix(LanceLoadoutSlot __instance, GameObject ___initiativeObj, TextMeshProUGUI ___initiativeText,
			UIColorRefTracker ___initiativeColor, HBSTooltip ___initiativeTooltip, LanceConfiguratorPanel ___LC)
		{
			if (___initiativeObj == null || ___initiativeText == null || ___initiativeColor == null)
			{
				return false;
			}
			if (__instance.SelectedMech == null || __instance.SelectedMech.MechDef == null || __instance.SelectedMech.MechDef.Chassis == null)
			{
				___initiativeObj.SetActive(false);
				return false;
			}
			if (__instance.SelectedPilot == null || __instance.SelectedPilot.Pilot == null || __instance.SelectedPilot.Pilot.pilotDef == null)
			{
				___initiativeObj.SetActive(false);
				return false;
			}
			___initiativeObj.SetActive(true);
			
			int num = 1; // default to assault phase
			int num2 = 0; // default to no modification by effects

			float f_walkSpeed = __instance.SelectedMech.MechDef.Chassis.MovementCapDef.MaxWalkDistance;
			if (f_walkSpeed >= TheEngineInitiative.Settings.MechPhaseSpecialMinSpeed)
			{
				num = 5;  //special phase
			}
			else if (f_walkSpeed >= TheEngineInitiative.Settings.MechPhaseLightMinSpeed)
			{
				num = 4;  //light phase
			}
			else if (f_walkSpeed >= TheEngineInitiative.Settings.MechPhaseMediumMinSpeed)
			{
				num = 3;  //medium phase
			}
			else if (f_walkSpeed >= TheEngineInitiative.Settings.MechPhaseHeavyMinSpeed)
			{
				num = 2;  //heavy phase
			}
			// check if pilot mods initiative
			if (__instance.SelectedPilot.Pilot.pilotDef.AbilityDefs != null)
			{
				foreach (AbilityDef abilityDef in __instance.SelectedPilot.Pilot.pilotDef.AbilityDefs)
				{
					foreach (EffectData effect in abilityDef.EffectData)
					{
						if (MechStatisticsRules.GetInitiativeModifierFromEffectData(effect, true, null) == 0)
						{
							num2 += MechStatisticsRules.GetInitiativeModifierFromEffectData(effect, false, null);
						}
					}
				}
			}
			// check if any of the mech's inventory changes initiative.
			if (__instance.SelectedMech.MechDef.Inventory != null)
			{
				foreach (MechComponentRef mechComponentRef in __instance.SelectedMech.MechDef.Inventory)
				{
					if (mechComponentRef.Def != null && mechComponentRef.Def.statusEffects != null)
					{
						foreach (EffectData effect2 in mechComponentRef.Def.statusEffects)
						{
							if (MechStatisticsRules.GetInitiativeModifierFromEffectData(effect2, true, null) == 0)
							{
								num2 += MechStatisticsRules.GetInitiativeModifierFromEffectData(effect2, false, null);
							}
						}
					}
				}
			}
			// is there a lance bonus?
			int num3 = 0;
			if (___LC != null)
			{
				num3 = ___LC.lanceInitiativeModifier;
			}
			num2 += num3;

			// make sure initiative is within the valid range.
			int num4 = Mathf.Clamp(num + num2, 1, 5);

			//set our text.
			___initiativeText.SetText($"{num4}");
			if (___initiativeTooltip != null)
			{
				// build the tooltip.  Going to use the mech's name and tell where its speed puts it, initiative-wise.
				string tooltipTitle = $"{__instance.SelectedMech.MechDef.Name}";
				string tooltipText = "A max walking speed of " + $"{f_walkSpeed}" + "m per turn means this mech moves in initiative phase " + $"{num}" + ".";

				// if there are effects, tell the player what they've changed initiative to.
				if (num2 != 0)
				{
					tooltipText += " Effects have modified this to phase " + $"{num4}" + ".";
				}
				BaseDescriptionDef initiativeData = new BaseDescriptionDef("MB_MIW_MECH_TT", tooltipTitle, tooltipText, null);
				___initiativeTooltip.enabled = true;
				___initiativeTooltip.SetDefaultStateData(TooltipUtilities.GetStateDataFromObject(initiativeData));
			}
			// if we've been bumped up, make it gold, if bumped down, make it red, else white.
			___initiativeColor.SetUIColor((num2 > 0) ? UIColor.Gold : ((num2 < 0) ? UIColor.Red : UIColor.White));

			return false;
		}
	}
}

