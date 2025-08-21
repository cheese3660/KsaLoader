using Brutal;
using Brutal.ImGuiApi;
using KSA;
using ImGuiWindow = KSA.ImGuiWindow;

namespace ExampleWindowedMod;

public class Window : ImGuiWindow, IStaticWindow
{
    public Window(float2 initialSize, bool lockAspectRatio = false) : base(initialSize, lockAspectRatio)
    {
        SetWindowTitle("Hello world window!");
    }

    public override void DrawContent(Viewport viewport)
    {
        ImGui.Text("Hello World!");
    }

}