using Terraria.GameInput;
using Terraria.ModLoader;

namespace ImGUI;

internal class ImGUIPlayer : ModPlayer
{
	public override void ProcessTriggers(TriggersSet triggersSet)
	{
		if (ImGUI.DebugKey.JustPressed)
			ImGUI.Config.DebugWindow = !ImGUI.Config.DebugWindow;
		if (ImGUI.ToggleImGui.JustPressed)
			ImGUI.Visible = !ImGUI.Visible;
	}
}