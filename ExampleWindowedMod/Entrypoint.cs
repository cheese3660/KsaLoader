using Brutal;
using KSA;
using KsaLoader.Core;

namespace ExampleWindowedMod;

public class Entrypoint : CodeMod
{
    public Entrypoint(Mod definingMod) : base(definingMod)
    {
        var window = new Window(new float2(100, 100));
    }
}