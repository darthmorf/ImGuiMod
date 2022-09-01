using ImGuiNET;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;

namespace ImGUI;

public static class AppLog
{
	internal static bool ShowAppLog;

	static bool _AutoScrollLogs = true;
	internal static readonly List<string> Logs = new();

	public static  ReadOnlyCollection<string> TmodLogs => Logs.AsReadOnly();
	
	internal static void Show()
	{
		if (!ImGui.Begin("tMod Logs", ref ShowAppLog))
		{
			ImGui.End();
			return;
		}
		
		// Options menu
		if (ImGui.BeginPopup("LogOptions"))
		{
			ImGui.Checkbox("Auto-scroll", ref _AutoScrollLogs);
			ImGui.EndPopup();
		}

		if (ImGui.Button("Options"))
			ImGui.OpenPopup("LogOptions");

		ImGui.SameLine();
		var clear = ImGui.Button("Clear");
		ImGui.SameLine();
		var copy = ImGui.Button("Copy");

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

		if (_AutoScrollLogs && ImGui.GetScrollY() >= ImGui.GetScrollMaxY())
			ImGui.SetScrollHereY(1.0f);

		ImGui.EndChild();
		ImGui.End();
	}
}