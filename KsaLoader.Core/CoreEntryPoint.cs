using System.Reflection;
using Brutal.Framework;
using HarmonyLib;
using KSA;

namespace KsaLoader.Core;

[HarmonyPatch]
public static class CoreEntryPoint
{
    private static List<Assembly> _assemblies = [];
    private static Harmony _harmony = new Harmony("KsaLoader.Core");
    public static void Execute()
    {
        _harmony.PatchAll();
    }
    
    [HarmonyPatch(typeof(Mod), nameof(Mod.Load))]
    [HarmonyPrefix]
    public static void LoadAssemblies(this Mod __instance)
    {
        var assemblyFolder = Path.Combine(__instance.DirectoryPath, "Assemblies");
        if (!Directory.Exists(assemblyFolder)) return;
        ModdedAssemblyResolver.AddModAssemblyFolder(assemblyFolder);
        // TODO: At some point add into the toml definition to only load a single assembly as the "Main Assembly"
        Log.Print($"Loading assemblies from mod {__instance.Name}");
        foreach (var dll in Directory.EnumerateFiles(assemblyFolder, "*.dll", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var asm = Assembly.LoadFile(Path.GetFullPath(dll));
                _assemblies.Add(asm);
                Log.Print($"Loaded {asm.GetName().Name}");
            }
            catch (Exception e)
            {
                Log.Print($"Failed to load {dll}, reason {e}");
            }
        }
    }

    [HarmonyPatch(typeof(ModLibrary), nameof(ModLibrary.LoadAll))]
    [HarmonyPostfix]
    public static void AfterLoad()
    {
        foreach (var type in _assemblies.SelectMany(x => x.GetTypes())
                     .Where(x => typeof(CodeMod).IsAssignableFrom(x) && !x.IsAbstract))
        {
            Activator.CreateInstance(type);
        }
        _harmony.UnpatchAll(_harmony.Id);
    }
}