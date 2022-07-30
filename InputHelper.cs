using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace ImGUI;

internal class InputHelper : ILoadable
{
	internal static bool Hover;
	internal static bool Text;

	public void Load(Mod mod)
	{
		if (!ImGUI.CanGui) return;
		On.Terraria.GameInput.PlayerInput.UpdateInput += Updateinput;
	}

	private void Updateinput(On.Terraria.GameInput.PlayerInput.orig_UpdateInput orig)
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
		// todo: prevent keyboard
		if(Text)
		{
			Main.keyState = new();
		}
	}

	public void Unload()
	{
		if (!ImGUI.CanGui) return;
		On.Terraria.GameInput.PlayerInput.UpdateInput -= Updateinput;

	}
}
