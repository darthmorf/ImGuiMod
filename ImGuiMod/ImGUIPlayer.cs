using Terraria.GameInput;
using Terraria.ModLoader;

namespace ImGuiMod;

internal class ImGUIPlayer : ModPlayer
{
	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (ImGUIMod.DebugKey.JustPressed)
			ImGUIMod.Config.DebugWindow = !ImGUIMod.Config.DebugWindow;
		if (ImGUIMod.ToggleImGui.JustPressed)
			ImGUIMod.Visible = !ImGUIMod.Visible;
	}
}