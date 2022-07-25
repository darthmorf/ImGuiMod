using ImGUI.Renderer;
using ImGUI.Utils;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace ImGUI;

internal class WindowInfo
{
	private static bool show_app_npc_explorer;
	internal static List<string> Logs = new();
	private static bool AutoScrollLogs = true;
	private static bool show_app_actives_overlay;
	static int activesoverlaycorner = -1;
	private static int selected_npc_explorer = -1;
	private static bool show_app_log;
	private static int npc_texture_frame = 1;
	private static bool animate_npc_texture;
	private static int frame_timer;
	private static bool show_app_proj_explorer;
	private static int selected_proj_explorer;
	private static bool animate_proj_texture;
	private static int proj_texture_frame = 1;

	internal static void GUI()
	{
		if (show_app_log)
			ShowAppLog();
		if (show_app_actives_overlay)
			ShowAppActivesOverlay();
		if (show_app_npc_explorer)
			ShowAppNPCExplorer();
		if (show_app_proj_explorer)
			ShowAppProjExplorer();

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

		ShowWindowInfo();

		ImGui.End();
	}

	private static void ShowAppProjExplorer()
	{
		SimpleLayout(ref show_app_proj_explorer, ref Main.projectile, "Projectile", ref selected_proj_explorer,
		n => n.active,
		n => n.Name,
		n =>
		{
			if (ImGui.BeginTabItem("AI"))
			{
				ImGui.TextWrapped("aiStyle: ");
				ImGui.SameLine();
				ImGui.TextWrapped(n.aiStyle.ToString());

				ImGui.TextWrapped("ai: ");
				ImGui.Indent();

				for (int i = 0; i < n.ai.Length; i++)
				{
					ImGui.TextWrapped($"ai[{i}]: ");
					ImGui.SameLine();
					ImGui.TextWrapped(n.ai[i].ToString());
				}

				ImGui.Unindent();

				ImGui.EndTabItem();
			}
			if (ImGui.BeginTabItem("Texture"))
			{
				var texture = TextureBinder.proj[n.type];

				ImGui.Image(texture.ptr, texture.Transform(100), texture.Uv0(proj_texture_frame), texture.Uv1(proj_texture_frame));
				ImGui.Separator();
				ImGui.SliderInt("Frame", ref proj_texture_frame, 1, texture.frames);
				ImGui.Checkbox("Animate", ref animate_proj_texture);
				if (animate_proj_texture)
				{
					frame_timer++;
					if (frame_timer > 5)
					{
						frame_timer = 0;
						proj_texture_frame++;
						if (proj_texture_frame > texture.frames)
						{
							proj_texture_frame = 1;
						}
					}
				}



				ImGui.EndTabItem();
			}
		},
		n =>
		{
			if (ImGui.Button("Teleport to"))
			{
				Main.player[Main.myPlayer].position = n.Center - new Microsoft.Xna.Framework.Vector2(0, Main.player[Main.myPlayer].Size.Y);
			}
			ImGui.SameLine();
			if (ImGui.Button("Disable"))
			{
				n.active = false;
			}
		});
	}

	private static void ShowAppNPCExplorer()
	{
		SimpleLayout(ref show_app_npc_explorer, ref Main.npc, "NPC", ref selected_npc_explorer, 
		n => n.active,
		n=> n.GivenOrTypeName,
		n=>
		{
			if (ImGui.BeginTabItem("Description"))
			{
				ImGui.TextWrapped("Fullname: ");
				ImGui.SameLine();
				ImGui.TextWrapped(n.FullName);

				ImGui.TextWrapped("GivenName: ");
				ImGui.SameLine();
				ImGui.TextWrapped(n.GivenName);

				ImGui.TextWrapped("type: ");
				ImGui.SameLine();
				ImGui.TextWrapped(n.type.ToString());

				ImGui.TextWrapped("netId: ");
				ImGui.SameLine();
				ImGui.TextWrapped(n.netID.ToString());

				ImGui.TextWrapped("life: ");
				ImGui.SameLine();
				ImGui.TextWrapped(n.life + "/" + n.lifeMax);

				VectorWrapped("position", n.position);

				VectorWrapped("velocity", n.velocity, true);

				ImGui.EndTabItem();
			}
			if (ImGui.BeginTabItem("AI"))
			{
				ImGui.TextWrapped("aiStyle: ");
				ImGui.SameLine();
				ImGui.TextWrapped(n.aiStyle.ToString());

				ImGui.TextWrapped("aiAction: ");
				ImGui.SameLine();
				ImGui.TextWrapped(n.aiAction.ToString());

				ImGui.TextWrapped("ai: ");
				ImGui.Indent();

				for (int i = 0; i < n.ai.Length; i++)
				{
					ImGui.TextWrapped($"ai[{i}]: ");
					ImGui.SameLine();
					ImGui.TextWrapped(n.ai[i].ToString());
				}

				ImGui.Unindent();

				ImGui.TextWrapped("target: ");
				ImGui.SameLine();

				if (n.HasValidTarget)
				{
					ImGui.TextWrapped(n.TranslatedTargetIndex + "(" + (n.HasPlayerTarget ? "Player" : "NPC") + ")");
				}
				else
				{
					ImGui.TextWrapped(n.target.ToString());
				}

				ImGui.EndTabItem();
			}
			if(ImGui.BeginTabItem("Texture"))
			{
				var texture = TextureBinder.npcs[n.type];

				ImGui.Image(texture.ptr, texture.Transform(100), texture.Uv0(npc_texture_frame), texture.Uv1(npc_texture_frame));
				ImGui.Separator();
				ImGui.SliderInt("Frame", ref npc_texture_frame, 1, texture.frames);
				ImGui.Checkbox("Animate", ref animate_npc_texture);
				if (animate_npc_texture)
				{
					frame_timer++;
					if(frame_timer > 5)
					{
						frame_timer = 0;
						npc_texture_frame++;
						if(npc_texture_frame > texture.frames)
						{
							npc_texture_frame = 1;
						}
					}
				}

				ImGui.EndTabItem();
			}
			ImGui.EndTabBar();
			var dr = ImGui.GetBackgroundDrawList();
			var init = n.position - Main.screenPosition;
			var end = (n.position + n.Size) - Main.screenPosition;
			dr.AddRect(new Vector2(init.X, init.Y), new Vector2(end.X, end.Y), 0xffffffff);
		},
		n=>
		{
			if (n.type != NPCID.TargetDummy)
			{
				if (ImGui.Button("Teleport to you"))
				{
					n.position = Main.player[Main.myPlayer].Center - new Microsoft.Xna.Framework.Vector2(0, n.Size.Y);
				}
				ImGui.SameLine();
			}
			if (ImGui.Button("Teleport to"))
			{
				Main.player[Main.myPlayer].position = n.Center - new Microsoft.Xna.Framework.Vector2(0, Main.player[Main.myPlayer].Size.Y);
			}
			ImGui.SameLine();
			if (ImGui.Button("Disable")) {
				n.active = false;
			}
		});
	}

	private static void ShowAppActivesOverlay()
	{
		Overlay(ref activesoverlaycorner, ref show_app_actives_overlay, () =>
		{
			ImGui.BulletText($"Players: {Main.player.SkipLast(1).Count(c => c.active)}/{Main.maxNetPlayers}");
			ImGui.BulletText($"NPCS: {Main.npc.SkipLast(1).Count(c => c.active)}/{Main.npc.Length - 1}");
			ImGui.BulletText($"Projs: {Main.projectile.SkipLast(1).Count(c => c.active)}/{Main.projectile.Length - 1}");
			ImGui.BulletText($"Dust: {Main.dust.SkipLast(1).Count(c => c.active)}/{Main.dust.Length - 1}");
			ImGui.BulletText($"Gore: {Main.gore.SkipLast(1).Count(c => c.active)}/{Main.gore.Length - 1}");
			ImGui.BulletText($"Items: {Main.item.SkipLast(1).Count(c => c.active)}/{Main.item.Length - 1}");
			
		});
	}

	private static void ShowWindowInfo()
	{

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
		if (ImGui.BeginMenu("Tools"))
		{
			ImGui.MenuItem("NPC Explorer", null, ref show_app_npc_explorer);
			ImGui.MenuItem("Projectile Explorer", null, ref show_app_proj_explorer);
			ImGui.EndMenu();
		}
	}

	private static void MenuBarInfo()
	{
		if (ImGui.BeginMenu("Info"))
		{
			ImGui.MenuItem("Log", null, ref show_app_log);
			ImGui.MenuItem("Actives overlay", null, ref show_app_actives_overlay);
			ImGui.EndMenu();
		}
	}
}
