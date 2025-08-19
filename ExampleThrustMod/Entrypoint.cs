using HarmonyLib;
using KsaLoader.Core;

namespace ExampleThrustMod;

public class Entrypoint : CodeMod
{
    public Entrypoint()
    {
        var harmony = new Harmony("ThrusterMod");
        harmony.PatchAll();
    }
}