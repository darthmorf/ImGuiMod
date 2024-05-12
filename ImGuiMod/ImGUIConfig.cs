using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace ImGuiMod;

internal class ImGUIConfig : ModConfig
{
	public override ConfigScope Mode => ConfigScope.ClientSide;

	[DefaultValue(true)]
	public bool TerrariaMouse;

	[DefaultValue(true)]
	public bool PreventInteraction;
}
