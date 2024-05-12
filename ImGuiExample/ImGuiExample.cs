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
       ImGuiImpl impl = new ImGuiImpl();
	}

	class ImGuiImpl : ModImGui
	{
        public override void CustomGUI()
        {
            ImGui.Begin("Hello World!");
            ImGui.Text("This is ImGui working from another mod!!");
            ImGui.End();
        }
    }
}
