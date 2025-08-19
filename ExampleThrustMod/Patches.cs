using Brutal;
using Brutal.GlfwApi;
using HarmonyLib;
using KSA;

namespace ThrusterMod;

[HarmonyPatch]
public static class Patches
{
    private static int _manualOverride = 8;
    private static AccessTools.FieldRef<Vehicle, EngineFlags> _engineFlagsFieldRef = AccessTools.FieldRefAccess<Vehicle, EngineFlags>("_engineFlags");
    private static AccessTools.FieldRef<VehicleUpdateTask,ManualControlInputs> _manualControlInputsRef = AccessTools.FieldRefAccess<VehicleUpdateTask,ManualControlInputs>("_manualControlInputs");
    
    [HarmonyPatch(typeof(Vehicle),nameof(Vehicle.OnKey))]
    [HarmonyPrefix]
    public static bool AddSpaceBarThrustAgain(Vehicle __instance, GlfwWindow window, GlfwKey key, int scanCode, GlfwKeyAction action, GlfwModifier mods, ref bool __result)
    {
        switch (action)
        {
            case GlfwKeyAction.Release:
                if (key == GlfwKey.Space)
                {
                    _engineFlagsFieldRef(__instance) = _engineFlagsFieldRef(__instance) & ~(EngineFlags)_manualOverride;
                    __result = true;
                    return false;
                }
                break;
            case GlfwKeyAction.Press:
                if (key == GlfwKey.Space)
                {
                    _engineFlagsFieldRef(__instance) = _engineFlagsFieldRef(__instance) | (EngineFlags)_manualOverride;
                    __result = true;
                    return false;
                }
                break;
        }

        return true;
    }
    
    [HarmonyPatch(typeof(Vehicle), nameof(Vehicle.PrepareWorker))]
    [HarmonyPostfix]
    public static void MaxOutThrust(Vehicle __instance, ref VehicleUpdateTask __result)
    {
        if ((_engineFlagsFieldRef(__instance) & (EngineFlags)_manualOverride) != 0)
        {
            _manualControlInputsRef(__result).EngineOn = true;
            _manualControlInputsRef(__result).EngineThrottle = 1f;
        }
    }
}