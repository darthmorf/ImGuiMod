using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ImGUI;

internal class ImGUIConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;

	[Label("Debug Window")]
	[DefaultValue(false)]
	[Tooltip("Show debug window")]
	public bool DebugWindow;

	[Label("Window Style")]
	[DefaultValue(ImGuiStyle.Dark)]
	[Tooltip("Color Style of imgui gui")]
	public ImGuiStyle Style;

	public override void OnChanged()
	{
		ImGUI.UpdateStyle(Style);
	}
}
