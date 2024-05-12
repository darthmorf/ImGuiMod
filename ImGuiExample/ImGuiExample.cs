using ImGuiMod;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace ImGuiExample
{
	public class ImGuiExample : Mod
	{

    }

    [ExtendsFromMod("ImGuiMod")]
    class ImGuiImpl01 : ImGuiInterface
	{
        public override ImGuiMod.ImGuiStyle Style => base.Style;

        public override void StandardDraw()
        {
            ImGui.Begin("Hello World!");
            ImGui.Text("This is ImGui working from another mod!!");
            ImGui.End();

            ImGui.ShowDemoWindow();
        }

        public override void BackgroundDraw(ImDrawListPtr drawList)
        {
            ImGui.Begin("Background");
            ImGui.Text("This is a background window.");
            ImGui.End();
        }

        public override void ForegroundDraw(ImDrawListPtr drawList)
        {
            ImGui.Begin("Foreground");
            ImGui.Text("This is a foreground window.");
            ImGui.End();
        }
    }

    [ExtendsFromMod("ImGuiMod")]
    class ImGuiImpl02 : ImGuiInterface
    {
        public override ImGuiMod.ImGuiStyle Style => ImGuiMod.ImGuiStyle.Dark;

        public override void StandardDraw()
        {
            ImGui.Begin("D:");
            ImGui.Text("Traditional style");
            ImGui.End();
        }
    }

    [ExtendsFromMod("ImGuiMod")]
    class ImGuiImpl03 : ImGuiInterface
    {
        public override ImGuiMod.ImGuiStyle Style => ImGuiMod.ImGuiStyle.Light;

        public override void StandardDraw()
        {
            ImGui.Begin("OW!!!");
            ImGui.Text("My eyes!!!!");
            ImGui.End();
        }
    }
}
