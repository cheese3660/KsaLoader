using KSA;

namespace KsaLoader.Core;


/// <summary>
/// Define a code mod to automatically be constructed when the mod is loaded
/// It's constructor must be public and follow the signature T(Mod definingMod) : base(definingMod)
/// </summary>
public abstract class CodeMod
{
    protected Mod DefiningMod;
    /// <summary>
    /// The constructor that gets called when constructed in descendent classes
    /// </summary>
    /// <param name="definingMod">The mod that defines this code mod</param>
    protected CodeMod(Mod definingMod)
    {
        DefiningMod = definingMod;
    }
}