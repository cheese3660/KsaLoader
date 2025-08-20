namespace KsaLoader.Core;

/// <summary>
/// Use this attribute on your mod class to say that it needs to be constructed after another type
/// It can be used multiple times
/// </summary>
/// <param name="t">Type to be constructed after (must be another mod class)</param>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
public class ConstructAfterTypeAttribute(Type t) : Attribute
{
    public Type ConstructAfter => t;
}