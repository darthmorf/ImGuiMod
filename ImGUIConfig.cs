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

	[Label("Terraria mouse")]
	[DefaultValue(false)]
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

	public override void OnChanged()
	{
		ImGUI.UpdateStyle(Style);
	}
}
