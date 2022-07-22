using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ImGUI;

public class ImGUIConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;

	[Label("Soaring Insignia Flight")]
	[DefaultValue(true)]
	[Tooltip("Show debug window")]
	public bool DebugWindow;
}
