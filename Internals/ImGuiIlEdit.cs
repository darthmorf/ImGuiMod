using ImGuiNET;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Drawing;
using System.Reflection;
using Terraria.ModLoader;
using Terraria.ModLoader.Core;

namespace ImGUI.Internals;

internal class ImGuiIlEdit
{
	public static string CurrentModGui;

	public static string NextWindowID;

	public static bool NextWindowAnimatedTitle;

	private static MethodInfo begin1;

	public delegate bool hook_Begin1(orig_Begin1 orig, string label);

	public delegate bool orig_Begin1(string label);

	private static MethodInfo begin2;

	public delegate bool hook_Begin2(orig_Begin1 orig, string label, ref bool open);

	public delegate bool orig_Begin2(string label, ref bool open);

	private static MethodInfo begin3;

	public delegate bool hook_Begin3(orig_Begin1 orig, string label, ImGuiWindowFlags flags);

	public delegate bool orig_Begin3(string label, ImGuiWindowFlags flags);

	private static MethodInfo begin4;

	public delegate bool hook_Begin4(orig_Begin1 orig, string label, ref bool open, ImGuiWindowFlags flags);

	public delegate bool orig_Begin4(string label, ref bool open, ImGuiWindowFlags flags);

	internal static void Apply()
	{
		var imgui = typeof(ImGui);

		begin1 = imgui.GetMethod("Begin", new Type[] { typeof(string) });
		MonoModHooks.Add(begin1, Begin1);

		begin2 = imgui.GetMethod("Begin", new Type[] { typeof(string), typeof(bool).MakeByRefType() });
        MonoModHooks.Add(begin2, Begin2);

		begin3 = imgui.GetMethod("Begin", new Type[] { typeof(string), typeof(ImGuiWindowFlags) });
        MonoModHooks.Add(begin3, Begin3);

		begin4 = imgui.GetMethod("Begin", new Type[] { typeof(string), typeof(bool).MakeByRefType(), typeof(ImGuiWindowFlags) });
		MonoModHooks.Add(begin4, Begin4);

	}

	public static bool Begin1(orig_Begin1 orig, string label)
	{
		ChangeLabel(ref label);
		return orig(label);
	}

	public static bool Begin2(orig_Begin2 orig, string label, ref bool open)
	{
		ChangeLabel(ref label);
		return orig(label, ref open);
	}

	public static bool Begin3(orig_Begin3 orig, string label, ImGuiWindowFlags flags)
	{
		ChangeLabel(ref label);
		return orig(label, flags);
	}

	public static bool Begin4(orig_Begin4 orig, string label, ref bool open, ImGuiWindowFlags flags)
	{
		ChangeLabel(ref label);
		return orig(label, ref open, flags);
	}

	private static void ChangeLabel(ref string label)
	{
		var str = NextWindowAnimatedTitle ? "###" : "##";
		var modname = string.IsNullOrWhiteSpace(CurrentModGui) ? "ImGUI" : CurrentModGui;

		if (!string.IsNullOrWhiteSpace(NextWindowID))
			modname = NextWindowID;

		if (NextWindowAnimatedTitle)
			label += str + label + modname;
		else
			label += str + modname;

		NextWindowAnimatedTitle = false;
		NextWindowID = null;
	}

	internal static void Revert()
	{
		// Hooks are now automatically unloaded when the mod is unloaded
	}
}