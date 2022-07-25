using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using HookList = Terraria.ModLoader.Core.HookList<ImGUI.ModImGui>;

namespace ImGUI;

public class ImGuiLoader
{
	internal static readonly List<ModImGui> guis = new();
	private static readonly List<HookList> hooks = new();

	private static HookList HookForeroundDraw = AddHook<Action<ImDrawListPtr>>(p => p.ForeroundDraw);

	public static void ForeroundDraw(ImDrawListPtr drawList)
	{
		foreach (var gui in HookForeroundDraw.Enumerate(guis))
		{
			gui.ForeroundDraw(drawList);
		}
	}

	private static HookList HookBackgroundDraw = AddHook<Action<ImDrawListPtr>>(p => p.BackgroundDraw);

	public static void BackgroundDraw(ImDrawListPtr drawList)
	{
		foreach (var gui in HookBackgroundDraw.Enumerate(guis))
		{
			gui.BackgroundDraw(drawList);
		}
	}

	private static HookList HookDebugGUI = AddHook<Action>(p => p.DebugGUI);

	public static void DebugGUI()
	{
		foreach (var gui in HookDebugGUI.Enumerate(guis))
		{
			gui.DebugGUI();
		}
	}

	private static HookList HookCustomGUI = AddHook<Action>(p => p.CustomGUI);

	public static void CustomGUI()
	{
		foreach (var gui in HookCustomGUI.Enumerate(guis))
		{
			gui.CustomGUI();
		}
	}

	internal static void UpdateHooks()
	{
		foreach (var hook in hooks)
		{
			hook.Update(guis);
		}
	}

	private static HookList AddHook<F>(Expression<Func<ModImGui, F>> func) where F : Delegate
	{
		var hook = HookList.Create(func);

		hooks.Add(hook);

		return hook;
	}
}
