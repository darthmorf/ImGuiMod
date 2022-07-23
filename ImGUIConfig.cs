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
}
