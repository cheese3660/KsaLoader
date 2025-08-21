namespace KsaLoader.Core;

/// <summary>
/// Use this attribute to denote that a method should be called immediately after loading the mods assembly,
/// without waiting for any possible dependencies to load
///
/// The method must either be of the signature
/// public static void ...();
/// or
/// public static void ...(Mod definingMod);
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class CallImmediatelyAttribute : Attribute;