using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ImGUI;

public class ImGUIConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;

	[Label("Debug Window")]
	[DefaultValue(false)]
	[Tooltip("Show debug window")]
	public bool DebugWindow;

	[Label("Info Window")]
	[DefaultValue(false)]
	[Tooltip("Show info window")]
	public bool InfoWindow;

	[Label("Window Style")]
	[DefaultValue(ImGuiStyle.Dark)]
	[Tooltip("Color Style of imgui gui")]
	public ImGuiStyle Style;

	public override void OnChanged()
	{
		ImGUI.UpdateStyle(Style);
	}
}
