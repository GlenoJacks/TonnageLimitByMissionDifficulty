using System;
using System.Collections.Generic;
using System.Linq;
using Harmony;
using BattleTech;
using BattleTech.UI;
using System.Reflection;
using Newtonsoft.Json;
using TMPro;

namespace TonnageLimitByMissionDifficulty
{

	// CODE FROM MORPHYUM
	public static class ReflectionHelper
	{
		public static object InvokePrivateMethode(object instance, string methodname, object[] parameters)
		{
			var type = instance.GetType();
			var methodInfo = type.GetMethod(methodname, BindingFlags.NonPublic | BindingFlags.Instance);
			return methodInfo.Invoke(instance, parameters);
		}

		public static object InvokePrivateMethode(object instance, string methodname, object[] parameters, Type[] types)
		{
			var type = instance.GetType();
			var methodInfo = type.GetMethod(methodname, BindingFlags.NonPublic | BindingFlags.Instance, null, types, null);
			return methodInfo.Invoke(instance, parameters);
		}

		public static void SetPrivateProperty(object instance, string propertyname, object value)
		{
			var type = instance.GetType();
			var property = type.GetProperty(propertyname, BindingFlags.NonPublic | BindingFlags.Instance);
			property.SetValue(instance, value, null);
		}

		public static void SetPrivateField(object instance, string fieldname, object value)
		{
			var type = instance.GetType();
			var field = type.GetField(fieldname, BindingFlags.NonPublic | BindingFlags.Instance);
			field.SetValue(instance, value);
		}

		public static object GetPrivateField(object instance, string fieldname)
		{
			var type = instance.GetType();
			var field = type.GetField(fieldname, BindingFlags.NonPublic | BindingFlags.Instance);
			return field.GetValue(instance);
		}
	}

	[HarmonyPatch(typeof(LanceConfiguratorPanel), "ValidateLance")]
    public static class LanceConfiguratorPanel_ValidateLance_Patch
    {
        public static void Postfix(LanceConfiguratorPanel __instance, ref bool __result)
        {
            if (!__instance.IsSimGame)
                return;

            if (__instance.activeContract == null)
                return;

            int difficulty = __instance.activeContract.Difficulty;
			int tonnageLimit = 0;
			if (!TonnageLimitByMissionDifficulty.GetTonnageLimit(difficulty, out tonnageLimit))
				return;

			float lanceTonnage = 0;
			List<MechDef> mechs = TonnageLimitByMissionDifficulty.GetMechDefs(__instance);
			foreach(MechDef mech in mechs)
			{
				lanceTonnage += mech.Chassis.Tonnage;
			}

            if (lanceTonnage <= tonnageLimit)
                return;

            __instance.lanceValid = false;

			String errorString = String.Format("Lance exceeds tonnage limit by {0} tons\n", lanceTonnage - tonnageLimit);

			LanceHeaderWidget headerWidget = (LanceHeaderWidget)ReflectionHelper.GetPrivateField(__instance, "headerWidget");
			headerWidget.RefreshLanceInfo(__instance.lanceValid, errorString, mechs);

            ReflectionHelper.SetPrivateField(__instance, "lanceErrorText", errorString);

            __result = __instance.lanceValid;
        }
    }

	[HarmonyPatch(typeof(LanceHeaderWidget), "RefreshLanceInfo")]
	public static class LanceHeaderWidget_RefreshLanceInfo_Patch
	{
		public static void Postfix(LanceHeaderWidget __instance, List<MechDef> mechs)
		{
			LanceConfiguratorPanel lanceConfiguratorPanel = (LanceConfiguratorPanel)ReflectionHelper.GetPrivateField(__instance, "LC");
			if (!lanceConfiguratorPanel.IsSimGame)
				return;

			Contract activeContract = (Contract)ReflectionHelper.GetPrivateField(__instance, "activeContract");
			if (activeContract == null)
				return;

			int difficulty = activeContract.Difficulty;
			int tonnageLimit = 0;
			if(!TonnageLimitByMissionDifficulty.GetTonnageLimit(difficulty, out tonnageLimit))
				return;

			float lanceTonnage = 0;
			foreach (MechDef mech in mechs)
			{
				lanceTonnage += mech.Chassis.Tonnage;
			}

			TextMeshProUGUI tonnageText = (TextMeshProUGUI)ReflectionHelper.GetPrivateField(__instance, "simLanceTonnageText");
			tonnageText.text = string.Format("{0} / {1} TONS", (int)lanceTonnage, tonnageLimit);
		}
	}

	public class Settings
	{
		public int[] tonnageByDifficulty = new int[] { 120, 145, 170, 195, 220, 245, 270, 295, 320, 350 };
	}

	public static class TonnageLimitByMissionDifficulty
	{
		public static Settings settings = new Settings();

		public static void Init(string directory, string settingsJSON)
		{
			var harmony = HarmonyInstance.Create("com.github.glenojacks.TonnageLimitByMissionDifficulty");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			// read settings
			try
			{
				settings = JsonConvert.DeserializeObject<Settings>(settingsJSON);
			}
			catch (Exception)
			{
				settings = new Settings();
			}
		}

		public static List<MechDef> GetMechDefs(LanceConfiguratorPanel lancePanel)
		{
			List<MechDef> mechs = new List<MechDef>();
			for (var i = 0; i < lancePanel.maxUnits; i++)
			{
				var lanceLoadoutSlot = ((LanceLoadoutSlot[])ReflectionHelper.GetPrivateField(lancePanel, "loadoutSlots"))[i];

				if (lanceLoadoutSlot.SelectedMech == null)
					continue;

				mechs.Add(lanceLoadoutSlot.SelectedMech.MechDef);
			}

			return mechs;
		}
		

		public static bool GetTonnageLimit(int difficulty, out int tonnageLimit)
		{
			tonnageLimit = 0;

			//no settings or invalid difficulty.
			if (difficulty <= 0 || settings.tonnageByDifficulty.Count() == 0)
			{
				return false;
			}

			//in valid settings range.
			if (difficulty <= settings.tonnageByDifficulty.Count())
			{
				tonnageLimit = settings.tonnageByDifficulty[difficulty - 1];
				return true;
			}

			//beyond the range of settings
			tonnageLimit = settings.tonnageByDifficulty.Last();
			return true;
		}
	}
}
