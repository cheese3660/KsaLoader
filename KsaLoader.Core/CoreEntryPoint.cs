using System.Reflection;
using System.Reflection.Emit;
using Brutal.Framework;
using HarmonyLib;
using KSA;

namespace KsaLoader.Core;

[HarmonyPatch]
public static class CoreEntryPoint
{
    private static List<(Mod mod, Assembly assembly)> _assemblies = [];
    private static Harmony _harmony = new Harmony("KsaLoader.Core");
    public static void Execute()
    {
        _harmony.PatchAll();

    }

    /// <summary>
    /// Load the mods in the preparation stage, has to be a transpiler patch as I can't postfix patch Mod.RegisterTo&lt;T&gt;
    /// </summary>
    [HarmonyPatch(typeof(ModLibrary), nameof(ModLibrary.PrepareAll))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> PrepareAllTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        // We want to get the local index for the Mod variable so that this can remain usable if the code changes
        var originalMethod = AccessTools.Method(typeof(ModLibrary), nameof(ModLibrary.PrepareAll));
        var originalMethodBody = originalMethod.GetMethodBody();
        var modLocal = originalMethodBody!.LocalVariables.First(x => x.LocalType == typeof(Mod));
        var loadInstruction = CodeInstruction.LoadLocal(modLocal.LocalIndex);
        foreach (var instruction in instructions)
        {
            yield return instruction;
            if (instruction.opcode != OpCodes.Callvirt) continue;
            var operand = instruction.operand as MethodInfo;
            if (operand == null) continue;
            if (operand.DeclaringType != typeof(Mod)) continue;
            if (operand.Name != nameof(Mod.RegisterTo)) continue;
            // load the assemblies for the mod
            // ldloc <mod>
            yield return loadInstruction;
            // call void CoreEntryPoint::LoadAssemblies(Mod)
            yield return CodeInstruction.Call(typeof(CoreEntryPoint),nameof(LoadAssemblies),[typeof(Mod)]);
        }
    }
    
    
    // [HarmonyPatch(typeof(Mod), nameof(Mod.RegisterTo))]
    // [HarmonyPostfix]
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
                _assemblies.Add((__instance, asm));
                foreach (var m in asm.GetTypes().SelectMany(x =>
                             x.GetMethods().Where(y => y.GetCustomAttributes<CallImmediatelyAttribute>().Any() && y.IsStatic)))
                {
                    if (m.GetParameters().Length == 1)
                    {
                        m.Invoke(null, [__instance]);
                    }
                    else
                    {
                        m.Invoke(null, null);
                    }
                }
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
        LinkedList<(Mod mod, Type codeMod, List<Type> dependencies)> types = new();
        foreach (var (mod, assembly) in _assemblies)
        {
            foreach (var type in assembly.GetTypes().Where(x => typeof(CodeMod).IsAssignableFrom(x) && !x.IsAbstract))
            {
                var deps = type.GetCustomAttributes<ConstructAfterTypeAttribute>().Select(x => x.ConstructAfter)
                    .ToList();
                types.AddLast((mod, type, deps));
            }
        }

        // List<(Mod mod, bool passModIntoConstructor, ConstructorInfo constructor)> constructorCallOrder = new();
        HashSet<Type> loadedTypes = [];
        
        while (types.Count > 0)
        {
            var wasChanged = false;
            List<Type> toRemove = [];
            var node = types.First;
            while (node != null)
            {
                var next = node.Next;
                if (node.ValueRef.dependencies.All(x => loadedTypes.Contains(x)))
                {
                    wasChanged = true;
                    loadedTypes.Add(node.ValueRef.codeMod);
                    var t = node.ValueRef.codeMod;
                    try
                    {
                        foreach (var constructor in t.GetConstructors())
                        {
                            if (constructor.GetParameters().Length != 1 ||
                                constructor.GetParameters()[0].ParameterType != typeof(Mod)) continue;
                            constructor.Invoke([node.ValueRef.mod]);
                            Log.PrintMessage($"Instantiated type {t} from mod {node.ValueRef.mod.Id}");
                            break;
                        }
                    }
                    catch (Exception e)
                    {
                        Log.PrintError($"Failed to instantiate type {t} from mod {node.ValueRef.mod.Id}, reason {e}");
                    }
                    types.Remove(node);
                }
                node = next;
            }

            if (wasChanged) continue;
            
            foreach (var (mod, type, deps) in types)
            {
                Log.PrintError($"Cannot construct mod type {type} from mod {mod.Id} as it is missing dependencies, its dependencies are as follows");
                foreach (var dep in deps)
                {
                    Log.PrintError($"\t- {dep}");
                }
            }
            break;
        }
        
        _harmony.UnpatchAll(_harmony.Id);
    }
}