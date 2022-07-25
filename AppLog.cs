using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace ImGUI;

internal class AppLog
{
	internal static bool show_app_log;
	private static bool AutoScrollLogs = true;
	internal static List<string> Logs = new();

	internal static void Show()
	{
		if (ImGui.Begin("tMod Logs", ref show_app_log))
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
			if (copy)
				ImGui.LogToClipboard();

			ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, Vector2.Zero);

			foreach (var log in Logs.ToArray())
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
}