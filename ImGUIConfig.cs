using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ImGUI;

internal class ImGUIConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;

	[Header("ImGui config")]
	[Label("Debug Window")]
	[DefaultValue(false)]
	[Tooltip("Show debug window")]
	public bool DebugWindow;

	[Label("Terraria mouse")]
	[DefaultValue(true)]
	[Tooltip("Use terraria mouse in imgui instead of normal mouse")]
	public bool TerrariaMouse;

	[Label("Prevent Interaction")]
	[DefaultValue(true)]
	[Tooltip("Prevents iterations with the world behind imGUI, for example if you scroll in imgui it doesn't change your selected item")]
	public bool PreventInteraction;

	[Label("Window Style")]
	[DefaultValue(ImGuiStyle.Terraria)]
	[Tooltip("Color Style of imgui gui")]
	public ImGuiStyle Style;

	[Header("ImGui dev")]
	[Label("Metrics Window")]
	[DefaultValue(false)]
	[Tooltip("Show metrics window, for developers only")]
	public bool ShowMetricsWindow;

	public override void OnChanged()
	{
		ImGUI.UpdateStyle(Style);
	}
}
