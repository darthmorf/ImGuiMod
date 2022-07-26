using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Terraria.ModLoader.Core;

namespace ImGUI;

/// <summary>
/// Hooks for <see cref="ModImGui"/> instances.
/// </summary>
public static class ImGuiLoader
{
	internal static readonly List<ModImGui> guis = new();
	private static readonly List<HookList> hooks = new();

	private class HookList
	{
		public int[] arr = new int[0];
		public readonly MethodInfo method;

		public HookList(MethodInfo method)
		{
			this.method = method;
		}
	}

	private static HookList HookForeroundDraw = AddHook<Action<ImDrawListPtr>>(p => p.ForeroundDraw);

	/// <summary>
	/// Call <see cref="ModImGui.ForeroundDraw(ImDrawListPtr)"/> hook.
	/// </summary>
	public static void ForeroundDraw(ImDrawListPtr drawList)
	{
		foreach (var gui in HookForeroundDraw.arr)
		{
			guis[gui].ForeroundDraw(drawList);
		}
	}

	private static HookList HookBackgroundDraw = AddHook<Action<ImDrawListPtr>>(p => p.BackgroundDraw);

	/// <summary>
	/// Call <see cref="ModImGui.BackgroundDraw(ImDrawListPtr)"/> hook.
	/// </summary>
	public static void BackgroundDraw(ImDrawListPtr drawList)
	{
		foreach (var gui in HookBackgroundDraw.arr)
		{
			guis[gui].BackgroundDraw(drawList);
		}
	}

	private static HookList HookDebugGUI = AddHook<Action>(p => p.DebugGUI);

	/// <summary>
	/// Call <see cref="ModImGui.DebugGUI()"/> hook.
	/// </summary>
	public static void DebugGUI()
	{
		foreach (var gui in HookDebugGUI.arr)
		{
			guis[gui].DebugGUI();
		}
	}

	private static HookList HookCustomGUI = AddHook<Action>(p => p.CustomGUI);

	/// <summary>
	/// Call <see cref="ModImGui.CustomGUI()"/> hook.
	/// </summary>
	public static void CustomGUI()
	{
		foreach (var gui in HookCustomGUI.arr)
		{
			guis[gui].CustomGUI();
		}
	}

	internal static void UpdateHooks()
	{
		foreach (var hook in hooks)
		{
			hook.arr = guis.WhereMethodIsOverridden(hook.method).Select(p => (int)p.Index).ToArray();
		}
	}

	private static HookList AddHook<F>(Expression<Func<ModImGui, F>> func) where F : Delegate
	{
		var hook = new HookList(func.ToMethodInfo());

		hooks.Add(hook);

		return hook;
	}
}
