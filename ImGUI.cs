using ImGUI.Renderer;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using ImVec4 = System.Numerics.Vector4;

[assembly: InternalsVisibleTo("DevTools")]

namespace ImGUI;

/// <summary>
/// ImGUI mod, used to access de current ImGuiRenderer.
/// </summary>
public class ImGUI : Mod
{
	// renderer
	private static ImGuiRenderer renderer;

	/// <summary>
	/// The current renderer, use to bind textures to use in imgui. 
	/// </summary>
	public static ImGuiRenderer Renderer { get => renderer; private set => renderer = value; }


	// renderer state
	private static bool _imguiloaded;

	// config
	internal static ModKeybind ToggleImGUI;
	internal static ModKeybind DebugKey;
	internal static ImGUIConfig Config;

	// state
	public static bool Visible { get; internal set; } = true;

	// native lib
	private static IntPtr NativeLib;

	private static readonly string CimguiPath = Path.GetTempFileName();

	/// <summary>
	/// Can imgui be loaded?
	/// </summary>
	public static bool CanGui => !Main.dedServ && Main.netMode != NetmodeID.Server;

	/// <inheritdoc/>
	public override void Load()
	{
		if (!CanGui) return;
		// configs
		Config = ModContent.GetInstance<ImGUIConfig>();
		DebugKey = KeybindLoader.RegisterKeybind(this, "Debug Window", Keys.F6);
		ToggleImGUI = KeybindLoader.RegisterKeybind(this, "Toggle ImGUI", Keys.F5);

		ConfigureNative();

		// add logger appender to Terraria
		log4net.Config.BasicConfigurator.Configure(new ImGuiAppender());

		//  execute in main thread to RebuildFontAtlas, background threads cant build font atlas
		Main.RunOnMainThread(() =>
		{
			// create and configure the renderer
			renderer = new ImGuiRenderer(this);

			var io = ImGui.GetIO();
			io.Fonts.Clear();
			var fontBytes = GetFileBytes("extras/FONT.TTF");
			var pinnedArray = GCHandle.Alloc(fontBytes, GCHandleType.Pinned);
			var pointer = pinnedArray.AddrOfPinnedObject();
			var f = io.Fonts.AddFontFromMemoryTTF(pointer, fontBytes.Length, 20);

			renderer.RebuildFontAtlas();

			pinnedArray.Free();

			LoadContent();
			On.Terraria.Main.DoDraw += Main_DoDraw;

			// initial style, can be moved?
			UpdateStyle(Config.Style);
		});

	}

	private void Main_DoDraw(On.Terraria.Main.orig_DoDraw orig, Main self, GameTime gameTime)
	{
		// render all terraria
		orig(self, gameTime);
		if(!Visible) return;

		// Update current state in imgui
		renderer.BeforeLayout(gameTime);

		// Draw our UI
		ImGuiLayout();
		
		// Call AfterLayout now to finish up and draw all the things
		renderer.AfterLayout();
		if(Config.TerrariaMouse)
		{
			Main.spriteBatch.Begin();
			Main.DrawCursor(Main.DrawThickCursor());
			Main.spriteBatch.End();
		}
	}

	/// <inheritdoc/>
	public override void PostSetupContent()
	{
		if (!CanGui) return;
		// idk if this go here
		ImGuiLoader.UpdateHooks();
	}

	// always true
	const bool use_work_area = true;

	private void ImGuiLayout()
	{
		// confiugre main dock area
		DockSpace();

		// todo: why dont work in normal style? something is setting padding to 0
		var st = ImGui.GetStyle();
		st.WindowPadding = new System.Numerics.Vector2(10, 10);

		// draw custom windows
		ImGuiLoader.CustomGUI();

		//draw debug window
		DebugWindow();

		// draw raws
		ImGuiLoader.BackgroundDraw(ImGui.GetBackgroundDrawList());
		ImGuiLoader.ForeroundDraw(ImGui.GetForegroundDrawList());

		
		InputHelper.Hover = ImGui.IsAnyItemHovered() || ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow | ImGuiHoveredFlags.RootAndChildWindows | ImGuiHoveredFlags.RectOnly | ImGuiHoveredFlags.AllowWhenDisabled);

		var io = ImGui.GetIO();

		InputHelper.Text = ImGui.IsAnyItemFocused() || io.WantTextInput;


		if (!Config.TerrariaMouse)
			// show the mouse if is over a window
			Main.instance.IsMouseVisible = InputHelper.Hover;
	}

	private void DebugWindow()
	{
		// show the logs
		if(AppLog.show_app_log)
		{
			AppLog.Show();
		}
			
		if (Config.DebugWindow && ImGui.Begin("Debug", ref Config.DebugWindow, ImGuiWindowFlags.MenuBar))
		{
			if (ImGui.BeginMenuBar())
			{
				if (ImGui.BeginMenu("Options"))
				{
					ImGui.MenuItem("Log", null, ref AppLog.show_app_log);
					if (ImGui.MenuItem("Close"))
					{
						Config.DebugWindow = false;
					}
					ImGui.EndMenu();
				}
				ImGui.EndMenuBar();
			}
			// only whow fps on debug so that it is not empty
			ImGui.TextWrapped(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));
			ImGuiLoader.DebugGUI();
			ImGui.End();
		}
	}

	private static void DockSpace()
	{
		var viewport = ImGui.GetMainViewport();
		// dont allow docks in center and make background inivisible.
		var dockspace_flags = ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.NoDockingInCentralNode;
		// the window itself is cand be docked, obvius.
		var window_flags = ImGuiWindowFlags.NoDocking;
		ImGui.SetNextWindowPos(use_work_area ? viewport.WorkPos : viewport.Pos);
		ImGui.SetNextWindowSize(use_work_area ? viewport.WorkSize : viewport.Size);
		ImGui.SetNextWindowViewport(viewport.ID);
		// we want our window to by practicaly not interactable, only need to be a dock parent
		window_flags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
		window_flags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;
		window_flags |= ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration;

		ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, System.Numerics.Vector2.Zero);
		ImGui.Begin("DockSpace Main", window_flags);
		
		// create de dockspace
		var dockspace_id = ImGui.GetID("MainDockSpace");
		ImGui.DockSpace(dockspace_id, System.Numerics.Vector2.Zero, dockspace_flags);

		ImGui.End();
	}

	private static string GetNativePath()
	{
		if (OperatingSystem.IsWindows())
			return "cimgui.dll";
		else if (OperatingSystem.IsMacOS())
			return "libcimgui.dylib";
		else if (OperatingSystem.IsLinux())
			return "libcimgui.so";
		throw new PlatformNotSupportedException();
	}

	private void ConfigureNative()
	{
		// make ImGui resolve the native with a custom resolver
		byte[] nativeByte = GetFileBytes(Path.Combine("lib", GetNativePath()));
		File.WriteAllBytes(CimguiPath, nativeByte);
		NativeLibrary.SetDllImportResolver(typeof(ImGui).Assembly, NativeResolver);
	}

	private IntPtr NativeResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
	{
		if (libraryName == "cimgui")
		{
			if(NativeLib != IntPtr.Zero) return NativeLib;
			NativeLib =  NativeLibrary.Load(CimguiPath);
			return NativeLib;
		}

		return IntPtr.Zero;
	}

	private void LoadContent()
	{
		// enable docking and mark imgui as loaded
		ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
		_imguiloaded = true;
	}

	/// <inheritdoc/>
	public override void Unload()
	{
		if (!CanGui) return;
		// stop drawing imgui
		On.Terraria.Main.DoDraw -= Main_DoDraw;
		_imguiloaded = false;
		DebugKey = null;
		Config = null;

		// destroy renderer sources and Free native lib
		renderer.Unload();
		NativeLibrary.Free(NativeLib);
		if (FileExists(CimguiPath))
			File.Delete(CimguiPath);
	}

	internal static void UpdateStyle(ImGuiStyle style)
	{
		if (!_imguiloaded) return;
		switch (style)
		{
			case ImGuiStyle.Terraria:
				setTerrariaStyle();
				break;
			case ImGuiStyle.Classic:
				ImGui.StyleColorsClassic();
				break;
			case ImGuiStyle.Dark:
				ImGui.StyleColorsDark();
				break;
			case ImGuiStyle.Light:
				ImGui.StyleColorsLight();
				break;
		}
		var st = ImGui.GetStyle();
		st.FrameRounding = 9;
		st.TabBorderSize = 1;
		st.WindowRounding = 6;
		st.PopupRounding = 6;
		st.ScrollbarRounding = 6;
		st.GrabRounding = 6;
		st.ScrollbarSize = 15;
	}

	private static void setTerrariaStyle()
	{
		ImGui.StyleColorsClassic();
		var style = ImGui.GetStyle();
		style.Colors[(int)ImGuiCol.Text] = new ImVec4(0.90f, 0.90f, 0.90f, 1.00f);
		style.Colors[(int)ImGuiCol.TextDisabled] = new ImVec4(0.60f, 0.60f, 0.60f, 1.00f);
		style.Colors[(int)ImGuiCol.WindowBg] = new ImVec4(0.27f, 0.29f, 0.53f, 0.78f);
		style.Colors[(int)ImGuiCol.ChildBg] = new ImVec4(0.00f, 0.00f, 0.00f, 0.00f);
		style.Colors[(int)ImGuiCol.PopupBg] = new ImVec4(0.09f, 0.05f, 0.25f, 0.92f);
		style.Colors[(int)ImGuiCol.Border] = new ImVec4(0.00f, 0.00f, 0.00f, 0.50f);
		style.Colors[(int)ImGuiCol.BorderShadow] = new ImVec4(0.00f, 0.00f, 0.00f, 0.00f);
		style.Colors[(int)ImGuiCol.FrameBg] = new ImVec4(0.14f, 0.16f, 0.33f, 0.78f);
		style.Colors[(int)ImGuiCol.FrameBgHovered] = new ImVec4(0.47f, 0.47f, 0.69f, 0.40f);
		style.Colors[(int)ImGuiCol.FrameBgActive] = new ImVec4(0.42f, 0.41f, 0.64f, 0.69f);
		style.Colors[(int)ImGuiCol.TitleBg] = new ImVec4(0.27f, 0.27f, 0.54f, 0.83f);
		style.Colors[(int)ImGuiCol.TitleBgActive] = new ImVec4(0.32f, 0.32f, 0.63f, 0.87f);
		style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new ImVec4(0.40f, 0.40f, 0.80f, 0.20f);
		style.Colors[(int)ImGuiCol.MenuBarBg] = new ImVec4(0.40f, 0.40f, 0.55f, 0.80f);
		style.Colors[(int)ImGuiCol.ScrollbarBg] = new ImVec4(0.28f, 0.35f, 0.62f, 0.69f);
		style.Colors[(int)ImGuiCol.ScrollbarGrab] = new ImVec4(0.75f, 0.76f, 0.78f, 1.00f);
		style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new ImVec4(1.00f, 1.00f, 1.00f, 1.00f);
		style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new ImVec4(1.00f, 1.00f, 1.00f, 1.00f);
		style.Colors[(int)ImGuiCol.CheckMark] = new ImVec4(0.90f, 0.90f, 0.90f, 0.50f);
		style.Colors[(int)ImGuiCol.SliderGrab] = new ImVec4(1.00f, 1.00f, 1.00f, 0.30f);
		style.Colors[(int)ImGuiCol.SliderGrabActive] = new ImVec4(0.41f, 0.39f, 0.80f, 0.60f);
		style.Colors[(int)ImGuiCol.Button] = new ImVec4(0.35f, 0.40f, 0.61f, 0.62f);
		style.Colors[(int)ImGuiCol.ButtonHovered] = new ImVec4(0.40f, 0.48f, 0.71f, 0.79f);
		style.Colors[(int)ImGuiCol.ButtonActive] = new ImVec4(0.46f, 0.54f, 0.80f, 1.00f);
		style.Colors[(int)ImGuiCol.Header] = new ImVec4(0.40f, 0.40f, 0.90f, 0.45f);
		style.Colors[(int)ImGuiCol.HeaderHovered] = new ImVec4(0.45f, 0.45f, 0.90f, 0.80f);
		style.Colors[(int)ImGuiCol.HeaderActive] = new ImVec4(0.53f, 0.53f, 0.87f, 0.80f);
		style.Colors[(int)ImGuiCol.Separator] = new ImVec4(0.50f, 0.50f, 0.50f, 0.60f);
		style.Colors[(int)ImGuiCol.SeparatorHovered] = new ImVec4(0.60f, 0.60f, 0.70f, 1.00f);
		style.Colors[(int)ImGuiCol.SeparatorActive] = new ImVec4(0.70f, 0.70f, 0.90f, 1.00f);
		style.Colors[(int)ImGuiCol.ResizeGrip] = new ImVec4(1.00f, 1.00f, 1.00f, 0.10f);
		style.Colors[(int)ImGuiCol.ResizeGripHovered] = new ImVec4(0.78f, 0.82f, 1.00f, 0.60f);
		style.Colors[(int)ImGuiCol.ResizeGripActive] = new ImVec4(0.78f, 0.82f, 1.00f, 0.90f);
		style.Colors[(int)ImGuiCol.Tab] = new ImVec4(0.34f, 0.34f, 0.68f, 0.79f);
		style.Colors[(int)ImGuiCol.TabHovered] = new ImVec4(0.45f, 0.45f, 0.90f, 0.80f);
		style.Colors[(int)ImGuiCol.TabActive] = new ImVec4(0.40f, 0.40f, 0.73f, 0.84f);
		style.Colors[(int)ImGuiCol.TabUnfocused] = new ImVec4(0.28f, 0.28f, 0.57f, 0.82f);
		style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new ImVec4(0.35f, 0.35f, 0.65f, 0.84f);
		style.Colors[(int)ImGuiCol.DockingPreview] = new ImVec4(0.90f, 0.85f, 0.40f, 0.31f);
		style.Colors[(int)ImGuiCol.DockingEmptyBg] = new ImVec4(0.20f, 0.20f, 0.20f, 1.00f);
		style.Colors[(int)ImGuiCol.PlotLines] = new ImVec4(1.00f, 1.00f, 1.00f, 1.00f);
		style.Colors[(int)ImGuiCol.PlotLinesHovered] = new ImVec4(0.90f, 0.70f, 0.00f, 1.00f);
		style.Colors[(int)ImGuiCol.PlotHistogram] = new ImVec4(0.90f, 0.70f, 0.00f, 1.00f);
		style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new ImVec4(1.00f, 0.60f, 0.00f, 1.00f);
		style.Colors[(int)ImGuiCol.TableHeaderBg] = new ImVec4(0.27f, 0.27f, 0.38f, 1.00f);
		style.Colors[(int)ImGuiCol.TableBorderStrong] = new ImVec4(0.31f, 0.31f, 0.45f, 1.00f);
		style.Colors[(int)ImGuiCol.TableBorderLight] = new ImVec4(0.26f, 0.26f, 0.28f, 1.00f);
		style.Colors[(int)ImGuiCol.TableRowBg] = new ImVec4(0.00f, 0.00f, 0.00f, 0.00f);
		style.Colors[(int)ImGuiCol.TableRowBgAlt] = new ImVec4(1.00f, 1.00f, 1.00f, 0.07f);
		style.Colors[(int)ImGuiCol.TextSelectedBg] = new ImVec4(0.00f, 0.00f, 1.00f, 0.35f);
		style.Colors[(int)ImGuiCol.DragDropTarget] = new ImVec4(1.00f, 1.00f, 0.00f, 0.90f);
		style.Colors[(int)ImGuiCol.NavHighlight] = new ImVec4(0.45f, 0.45f, 0.90f, 0.80f);
		style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new ImVec4(1.00f, 1.00f, 1.00f, 0.70f);
		style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new ImVec4(0.80f, 0.80f, 0.80f, 0.20f);
		style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new ImVec4(0.20f, 0.20f, 0.20f, 0.35f);



	}
}