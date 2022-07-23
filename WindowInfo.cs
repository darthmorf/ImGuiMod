using ImGuiNET;
using System.Collections.Generic;
using System.Numerics;

namespace ImGUI;

internal class WindowInfo
{
	private static bool show_app_log;
	internal static List<string> Logs = new();
	private static bool AutoScrollLogs = true;

	internal static void GUI()
	{
		if (show_app_log)
			ShowAppLog();

		if (!ImGUI.Config.InfoWindow) return;

		if (!ImGui.Begin("Info Window", ImGuiWindowFlags.MenuBar))
		{
			ImGui.End();
			return;
		}
		
		if (ImGui.BeginMenuBar())
		{
			MenuBarInfo();
			MenuBarTools();
			MenuBarOptions();
			MenuBarMods();

			ImGui.EndMenuBar();
		}

		ImGui.End();
	}

	private static void ShowAppLog()
	{
		if(ImGui.Begin("tMod Logs", ref show_app_log))
		{
			// Options menu
			if (ImGui.BeginPopup("LogOptions"))
			{
				ImGui.Checkbox("Auto-scroll", ref AutoScrollLogs);
				ImGui.EndPopup();
			}

			if (ImGui.Button("Options"))
				ImGui.OpenPopup("LogOptions");

			ImGui.SameLine();
			bool clear = ImGui.Button("Clear");
			ImGui.SameLine();
			bool copy = ImGui.Button("Copy");

			ImGui.Separator();
			ImGui.BeginChild("scrolling", Vector2.Zero, false, ImGuiWindowFlags.HorizontalScrollbar);

			if (clear)
				Logs.Clear();
			if(copy)
				ImGui.LogToClipboard();

			ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

			foreach (var log in Logs)
			{
				ImGui.TextUnformatted(log);
			}

			ImGui.PopStyleVar();

			if (AutoScrollLogs && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
				ImGui.SetScrollHereY(1.0f);

			ImGui.EndChild();
			ImGui.End();
		}
	}

	private static void MenuBarMods()
	{
		
	}

	private static void MenuBarOptions()
	{
	}

	private static void MenuBarTools()
	{
	}

	private static void MenuBarInfo()
	{
		if (ImGui.BeginMenu("Info"))
		{
			ImGui.MenuItem("Log", null, ref show_app_log);
			ImGui.EndMenu();
		}
	}
}