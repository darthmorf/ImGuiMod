using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ImGUI.extras;

internal class GamePauseFix : ILoadable
{
	public void Load(Mod mod)
	{
		On.Terraria.Main.CanPauseGame += Main_CanPauseGame;
	}

	private bool Main_CanPauseGame(On.Terraria.Main.orig_CanPauseGame orig) =>
		orig() || (Main.netMode == NetmodeID.SinglePlayer && Main.InGameUI.IsVisible && Main.InGameUI.CurrentState?.GetType()?.Name is "UIModConfig" or "UIModConfigList");

	public void Unload()
	{
		On.Terraria.Main.CanPauseGame -= Main_CanPauseGame;
	}
}
