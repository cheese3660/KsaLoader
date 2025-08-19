using System.Reflection;

namespace KsaLoader;

public static class ModdedAssemblyResolver
{
    public static List<string> AssemblyLoadPaths { get; private set; } = [".", "./KsaLoaderAssemblies"];

    public static void AddModAssemblyFolder(string folder)
    {
        AssemblyLoadPaths.Add(folder);
    }

    public static Assembly? ResolveAssembly(object? sender, ResolveEventArgs args)
    {
        if (AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(x => x.FullName == args.Name) is { } assembly)
        {
            return assembly;
        }
        var assemblyName = new AssemblyName(args.Name);
        var culture = assemblyName.CultureName;
        foreach (var path in AssemblyLoadPaths)
        {
            var culturedAssembly = Path.Combine(path, culture ?? "en", assemblyName.Name + ".dll");
            if (File.Exists(culturedAssembly))
            {
                return Assembly.LoadFile(Path.GetFullPath(culturedAssembly));
            }
            var regularAssembly = Path.Combine(path, assemblyName.Name + ".dll");
            if (File.Exists(regularAssembly))
            {
                return Assembly.LoadFile(Path.GetFullPath(regularAssembly));
            }
        }
        return null;
    }
}