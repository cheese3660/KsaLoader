using HarmonyLib;
using KSA;
using KsaLoader.Core;

namespace ExampleThrustMod;

public class Entrypoint : CodeMod
{
    
    public Entrypoint(Mod definingMod) : base(definingMod)
    {
        var harmony = new Harmony("ThrusterMod");
        harmony.PatchAll();
    }
}