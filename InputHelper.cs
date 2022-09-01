using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace ImGUI;

public class InputHelper : ILoadable
{
	public static bool Hover;
	public static bool Text;

	public static bool PauseMenu => !Main.gameMenu && (Main.ingameOptionsWindow || Main.InGameUI.IsVisible);

	public void Load(Mod mod)
	{
		if (!ImGUI.CanGui) return;
		On.Terraria.GameInput.PlayerInput.UpdateInput += Updateinput;
		On.Terraria.Main.DoUpdate_HandleInput += DoUpdate_HandleInput;
	}

	private void DoUpdate_HandleInput(On.Terraria.Main.orig_DoUpdate_HandleInput orig, Main self)
	{
		orig(self);
		if (!ImGUI.Visible || !ImGUI.Config.PreventInteraction) return;
		if (Text)
		{
			Main.keyState = new();
		}
	}

	void Updateinput(On.Terraria.GameInput.PlayerInput.orig_UpdateInput orig)
	{
		orig();
		if (!ImGUI.Visible || !ImGUI.Config.PreventInteraction) return;

		if (Hover)
		{
			Main.mouseLeft =
			Main.mouseRight =
			false;
			PlayerInput.ScrollWheelDelta = 0;
			PlayerInput.ScrollWheelDeltaForUI = 0;
			if (PlayerInput.Triggers.Current.MouseLeft)
			{
				PlayerInput.Triggers.JustReleased.MouseLeft = true;
				Main.mouseLeftRelease = true;
				PlayerInput.Triggers.Current.MouseLeft = false;
			}
			if (PlayerInput.Triggers.Current.MouseRight)
			{
				PlayerInput.Triggers.JustReleased.MouseRight = true;
				Main.mouseRightRelease = true;
				PlayerInput.Triggers.Current.MouseRight = false;
			}

		}
	}

	public void Unload()
	{
		if (!ImGUI.CanGui) return;
		On.Terraria.GameInput.PlayerInput.UpdateInput -= Updateinput;
		On.Terraria.Main.DoUpdate_HandleInput -= DoUpdate_HandleInput;

	}
}
