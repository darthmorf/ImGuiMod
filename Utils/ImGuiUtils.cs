using ImGuiNET;
using System;
using System.Numerics;

namespace ImGUI.Utils;

public class ImGuiUtils
{
	public static void Overlay(ref int corner, ref bool open, Action content)
	{
		var window_flags = ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoDocking | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNav;

		if (corner != -1)
		{
			const float PAD = 10.0f;
			var viewport = ImGui.GetMainViewport();
			var work_pos = viewport.WorkPos; // Use work area to avoid menu-bar/task-bar, if any!
			var work_size = viewport.WorkSize;
			Vector2 window_pos = default, window_pos_pivot = default;
			window_pos.X = ((corner & 1) == 1) ? (work_pos.X + work_size.X - PAD) : (work_pos.X + PAD);
			window_pos.Y = ((corner & 2) == 2) ? (work_pos.Y + work_size.Y - PAD) : (work_pos.Y + PAD);
			window_pos_pivot.X = ((corner & 1) == 1) ? 1.0f : 0.0f;
			window_pos_pivot.Y = ((corner & 2) == 2) ? 1.0f : 0.0f;
			ImGui.SetNextWindowPos(window_pos, ImGuiCond.FirstUseEver, window_pos_pivot);
			ImGui.SetNextWindowViewport(viewport.ID);
			window_flags |= ImGuiWindowFlags.NoMove;
		}
		ImGui.SetNextWindowBgAlpha(0.35f); // Transparent background

		if (ImGui.Begin("Actives overlay", ref open, window_flags))
		{
			
			content();

			if (ImGui.BeginPopupContextWindow())
			{
				if (ImGui.MenuItem("Custom", null, corner == -1)) corner = -1;
				if (ImGui.MenuItem("Top-left", null, corner == 0)) corner = 0;
				if (ImGui.MenuItem("Top-right", null, corner == 1)) corner = 1;
				if (ImGui.MenuItem("Bottom-left", null, corner == 2)) corner = 2;
				if (ImGui.MenuItem("Bottom-right", null, corner == 3)) corner = 3;
				if (open && ImGui.MenuItem("Close")) open = false;
				ImGui.EndPopup();
			}
		}
		ImGui.End();
	}

	public static void SimpleLayout<T>(ref bool open, ref T[] npc, string name, ref int selected, Func<T, bool> active, Func<T, string> display, Action<T> tabs, Action<T> buttons)
	{
		ImGui.SetNextWindowSize(new Vector2(500, 500), ImGuiCond.FirstUseEver);
		if (ImGui.Begin($"{name} Explorer", ref open, ImGuiWindowFlags.MenuBar))
		{
			if (ImGui.BeginMenuBar())
			{
				if (ImGui.BeginMenu("Options"))
				{
					if (ImGui.MenuItem("Close")) open = false;
					ImGui.EndMenu();
				}
				ImGui.EndMenuBar();
			}

			{
				ImGui.BeginChild("left pane", new Vector2(200, 0), true);
				for (int i = 0; i < npc.Length - 1; i++)
				{
					if (active(npc[i]))
						if (ImGui.Selectable($"{display(npc[i])}({i})", selected == i))
							selected = i;
				}
				ImGui.EndChild();
			}
			ImGui.SameLine();
			if(selected >= 0 && selected < npc.Length - 1 && active(npc[selected]))
			// Right
			{
				ImGui.BeginGroup();
				ImGui.BeginChild("item view", new Vector2(0, -ImGui.GetFrameHeightWithSpacing())); // Leave room for 1 line below us
				ImGui.Text($"{name}: {selected}");
				ImGui.Separator();
				if (ImGui.BeginTabBar("##Tabs", ImGuiTabBarFlags.None))
				{
					tabs(npc[selected]);
				}
				ImGui.EndChild();
				buttons(npc[selected]);
				ImGui.EndGroup();
			}
			else
			{
				ImGui.TextWrapped("Choose an item from the left panel");
			}
		}
		ImGui.End();
	}

	internal static void VectorWrapped(string v, Microsoft.Xna.Framework.Vector2 position, bool Length = false)
	{
		ImGui.TextWrapped($"{v}: ");
		ImGui.Indent();

		ImGui.TextWrapped("X: ");
		ImGui.SameLine();
		ImGui.TextWrapped(position.X.ToString());

		ImGui.TextWrapped("Y: ");
		ImGui.SameLine();
		ImGui.TextWrapped(position.Y.ToString());

		if (Length)
		{
			ImGui.TextWrapped("Length: ");
			ImGui.SameLine();
			ImGui.TextWrapped(position.Length().ToString());
		}

		ImGui.Unindent();
	}
}
