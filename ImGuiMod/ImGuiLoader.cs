using ImGuiMod.Internals;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Terraria;
using Terraria.ModLoader.Core;
// ReSharper disable InconsistentNaming

namespace ImGuiMod;

/// <summary>
/// Hooks for <see cref="ModImGui"/> instances.
/// </summary>
public static class ImGuiLoader
{
	internal static readonly List<ModImGui> guis = new();

	static readonly List<HookList> hooks = new();

	class HookList
	{
		public int[] arr = Array.Empty<int>();
		public readonly MethodInfo method;

		public HookList(MethodInfo method)
		{
			this.method = method;
		}
	}

	static readonly HookList HookForeroundDraw = AddHook<Action<ImDrawListPtr>>(p => p.ForeroundDraw);

	/// <summary>
	/// Call <see cref="ModImGui.ForeroundDraw(ImDrawListPtr)"/> hook.
	/// </summary>
	public static void ForeroundDraw(ImDrawListPtr drawList)
	{
		foreach (int gui in HookForeroundDraw.arr)
		{
			if (Main.gameMenu && !guis[gui].RenderInMainMenu) continue;
			ImGuiIlEdit.CurrentModGui = guis[gui].Mod.Name;
			guis[gui].ForeroundDraw(drawList);
			ImGuiIlEdit.CurrentModGui = null;
		}
	}
	 
	static readonly HookList HookBackgroundDraw = AddHook<Action<ImDrawListPtr>>(p => p.BackgroundDraw);

	/// <summary>
	/// Call <see cref="ModImGui.BackgroundDraw(ImDrawListPtr)"/> hook.
	/// </summary>
	public static void BackgroundDraw(ImDrawListPtr drawList)
	{
		foreach (int gui in HookBackgroundDraw.arr)
		{
			if (Main.gameMenu && !guis[gui].RenderInMainMenu) continue;
			ImGuiIlEdit.CurrentModGui = guis[gui].Mod.Name;
			guis[gui].BackgroundDraw(drawList);
			ImGuiIlEdit.CurrentModGui = null;
		}
	}

	static readonly HookList HookDebugGUI = AddHook<Action>(p => p.DebugGUI);

	/// <summary>
	/// Call <see cref="ModImGui.DebugGUI()"/> hook.
	/// </summary>
	public static void DebugGUI()
	{
		foreach (int gui in HookDebugGUI.arr)
		{
			if (!ImGui.CollapsingHeader(guis[gui].Mod.DisplayName)) continue;
			ImGuiIlEdit.CurrentModGui = guis[gui].Mod.Name;
			guis[gui].DebugGUI();
			ImGuiIlEdit.CurrentModGui = null;
		}
	}

	internal static bool RenderDebugInPause => HookDebugGUI.arr.Any(x => guis[x].RenderInPause);

	internal static bool RenderDebugInMainMenu => HookDebugGUI.arr.Any(x => guis[x].RenderInMainMenu);

	static readonly HookList HookOverlayGUI = AddHook<Action>(p => p.OverlayGUI);

	/// <summary>
	/// Call <see cref="ModImGui.OverlayGUI()"/> hook.
	/// </summary>
	public static void OverlayGUI()
	{
		foreach (int gui in HookOverlayGUI.arr)
		{
			if (Main.gameMenu && !guis[gui].RenderInMainMenu) continue;
			if (InputHelper.PauseMenu && !guis[gui].RenderInPause) continue;
			ImGuiIlEdit.CurrentModGui = guis[gui].Mod.Name;
			guis[gui].OverlayGUI();
			ImGuiIlEdit.CurrentModGui = null;
		}
		
	}

	static readonly HookList HookCustomGUI = AddHook<Action>(p => p.CustomGUI);

	/// <summary>
	/// Call <see cref="ModImGui.CustomGUI()"/> hook.
	/// </summary>
	public static void CustomGUI()
	{
		foreach (int gui in HookCustomGUI.arr)
		{
			// TODO - we need to work out if we want the developer to handle these checks themselves.

			//if (ImGUIMod.Visible || guis[gui].AlwaysVisible)
			{
				//if (Main.gameMenu && !guis[gui].RenderInMainMenu) continue;
				//if (InputHelper.PauseMenu && !guis[gui].RenderInPause) continue;
				ImGuiIlEdit.CurrentModGui = guis[gui].Mod.Name;
				guis[gui].CustomGUI();
				ImGuiIlEdit.CurrentModGui = null;
			}
		}
	}

	internal static void UpdateHooks()
	{
		foreach (HookList hook in hooks)
		{
			IEnumerable<ModImGui> overridenGuids = guis.WhereMethodIsOverridden(g => g.CustomGUI);
            hook.arr = overridenGuids.Select(p => (int)p.Index).ToArray();
		}
	}

	static HookList AddHook<F>(Expression<Func<ModImGui, F>> func) where F : Delegate
	{
        HookList hook = new HookList(func.ToMethodInfo());

		hooks.Add(hook);

		return hook;
	}
}
