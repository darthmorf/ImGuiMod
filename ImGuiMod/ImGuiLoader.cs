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
/// Hooks for <see cref="ImGuiInterface"/> instances.
/// </summary>
public static class ImGuiLoader
{
	internal static readonly List<ImGuiInterface> guis = new();

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

	static readonly HookList HookForegroundDraw = AddHook<Action<ImDrawListPtr>>(p => p.ForegroundDraw);

	/// <summary>
	/// Call <see cref="ImGuiInterface.ForegroundDraw(ImDrawListPtr)"/> hook.
	/// </summary>
	public static void ForegroundDraw(ImDrawListPtr drawList)
	{
		foreach (int gui in HookForegroundDraw.arr)
		{
			if (Main.gameMenu && !guis[gui].RenderInMainMenu) continue;
			ImGuiIlEdit.CurrentModGui = guis[gui].Mod.Name;
			guis[gui].ForegroundDraw(drawList);
			ImGuiIlEdit.CurrentModGui = null;
		}
	}
	 
	static readonly HookList HookBackgroundDraw = AddHook<Action<ImDrawListPtr>>(p => p.BackgroundDraw);

	/// <summary>
	/// Call <see cref="ImGuiInterface.BackgroundDraw(ImDrawListPtr)"/> hook.
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

	static readonly HookList HookStandardDraw = AddHook<Action>(p => p.StandardDraw);

    /// <summary>
    /// Call <see cref="ImGuiInterface.HookStandardDraw()"/> hook.
    /// </summary>
    public static void StandardDraw()
	{
		foreach (int gui in HookStandardDraw.arr)
		{
			ImGuiIlEdit.CurrentModGui = guis[gui].Mod.Name;
			guis[gui].StandardDraw();
			ImGuiIlEdit.CurrentModGui = null;
		}
	}

	internal static void UpdateHooks()
	{
		foreach (HookList hook in hooks)
		{
			// TODO - this line is probably wrong as it only looks at StandardDraw
			IEnumerable<ModImGui> overridenGuids = guis.WhereMethodIsOverridden(g => g.StandardDraw);
            hook.arr = overridenGuids.Select(p => (int)p.Index).ToArray();
		}
	}

	static HookList AddHook<F>(Expression<Func<ImGuiInterface, F>> func) where F : Delegate
	{
        HookList hook = new HookList(func.ToOverrideQuery());

		hooks.Add(hook);

		return hook;
	}
}
