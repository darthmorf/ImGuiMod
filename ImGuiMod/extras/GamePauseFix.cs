using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ImGuiMod.extras;

internal class GamePauseFix : ILoadable
{
	public void Load(Mod mod)
	{
		Terraria.On_Main.CanPauseGame += Main_CanPauseGame;
	}

	private bool Main_CanPauseGame(Terraria.On_Main.orig_CanPauseGame orig) =>
		orig() || (Main.netMode == NetmodeID.SinglePlayer && Main.InGameUI.IsVisible && Main.InGameUI.CurrentState?.GetType()?.Name is "UIModConfig" or "UIModConfigList");

	public void Unload()
	{
		Terraria.On_Main.CanPauseGame -= Main_CanPauseGame;
	}
}
