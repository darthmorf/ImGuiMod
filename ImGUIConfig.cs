using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ImGUI;

internal class ImGUIConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;

	[Header("DebugWindow.Header")]
	[DefaultValue(false)]
	public bool DebugWindow;

	[DefaultValue(true)]
	public bool TerrariaMouse;

	[DefaultValue(true)]
	public bool PreventInteraction;

	[DefaultValue(ImGuiStyle.Terraria)]
	public ImGuiStyle Style;

	[Header("MetricsWindow.Header")]
	[DefaultValue(false)]
	public bool ShowMetricsWindow;

	public override void OnChanged()
	{
		ImGUI.UpdateStyle(Style);
	}
}
