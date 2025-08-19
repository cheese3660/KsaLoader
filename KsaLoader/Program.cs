// See https://aka.ms/new-console-template for more information

using System.Reflection;
using KsaLoader;

// Set up our custom assembly resolver
const string KsaPath = "./ksa.dll";
Console.WriteLine(Path.GetFullPath(KsaPath));
Console.WriteLine(File.Exists(KsaPath));
// Load the KSA assembly
var ksa = Assembly.LoadFile(Path.GetFullPath(KsaPath));
// Load our core assembly
var coreAssembly = Assembly.LoadFile(Path.GetFullPath("./KsaLoaderAssemblies/KsaLoader.Core.dll"));
AppDomain.CurrentDomain.AssemblyResolve += ModdedAssemblyResolver.ResolveAssembly;
// Execute our core assembly before the game
coreAssembly.GetType("KsaLoader.Core.CoreEntryPoint")!.GetMethod("Execute")!.Invoke(null, null);
// Launch the game
ksa.EntryPoint!.Invoke(null,[args]);