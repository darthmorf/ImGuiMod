﻿using System.Diagnostics;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using tModPorter;

namespace ImGuiMod;

public class InputHelper : ILoadable
{
	public static bool ImGuiHasHover;
	public static bool Text;

	public static bool PauseMenu => !Main.gameMenu && (Main.ingameOptionsWindow || Main.InGameUI.IsVisible);

	public void Load(Mod mod)
	{
		if (!ImGUIMod.CanGui) return;
		Terraria.GameInput.On_PlayerInput.UpdateInput += Updateinput;
		Terraria.On_Main.DoUpdate_HandleInput += DoUpdate_HandleInput;
	}

	private void DoUpdate_HandleInput(Terraria.On_Main.orig_DoUpdate_HandleInput orig, Main self)
	{
		orig(self);

		if (ImGUIMod.Config == null || !ImGUIMod.Config.PreventInteraction) return;

		if (Text)
		{
			Main.keyState = new();
		}
	}

	void Updateinput(Terraria.GameInput.On_PlayerInput.orig_UpdateInput orig)
	{
        if (ImGUIMod.Config == null) return;

		if (ImGUIMod.Config.PreventInteraction && ImGuiHasHover)
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
		else
		{
            orig();
        }

    }

	public void Unload()
	{
		if (!ImGUIMod.CanGui) return;
        Terraria.GameInput.On_PlayerInput.UpdateInput -= Updateinput;
		Terraria.On_Main.DoUpdate_HandleInput -= DoUpdate_HandleInput;

	}
}
