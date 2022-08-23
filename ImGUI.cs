using ImGUI.Internals;
using ImGUI.Renderer;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

[assembly: InternalsVisibleTo("DevTools")]

namespace ImGUI;

/// <summary>
/// ImGUI mod, used to access de current ImGuiRenderer.
/// </summary>
public class ImGUI : Mod
{

	/// <summary>
	/// The current renderer, use to bind textures to use in imgui. 
	/// </summary>
	public static ImGuiRenderer Renderer { get; private set; }


	// renderer state
	static bool _Imguiloaded;

	// config
	internal static ModKeybind ToggleImGui;
	internal static ModKeybind DebugKey;
	internal static ImGUIConfig Config;

	// state
	/// <summary>
	/// Get of currently ImGUI is visible
	/// </summary>
	public static bool Visible { get; internal set; } = true;

	// native lib
	static IntPtr _NativeLib;
	static readonly string CimguiPath = Path.GetTempFileName();

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
		ToggleImGui = KeybindLoader.RegisterKeybind(this, "Toggle ImGUI", Keys.F5);
		
		ConfigureNative();

		// add logger appender to Terraria
		log4net.Config.BasicConfigurator.Configure(new ImGuiAppender());

		ImGuiIlEdit.Apply();
		
		var check = new AutoResetEvent(false);

		//  execute in main thread to RebuildFontAtlas, background threads cant build font atlas
		Main.RunOnMainThread(() =>
		{
			// create and configure the renderer
			Renderer = new(this);

			var io = ImGui.GetIO();
			io.Fonts.Clear();
			var fontBytes = GetFileBytes("extras/FONT.TTF");
			var pinnedArray = GCHandle.Alloc(fontBytes, GCHandleType.Pinned);
			var pointer = pinnedArray.AddrOfPinnedObject();
			io.Fonts.AddFontFromMemoryTTF(pointer, fontBytes.Length, 20);
			
			Renderer.RebuildFontAtlas();

			pinnedArray.Free();

			LoadContent();

			check.Set();

			// initial style, can be moved?
			UpdateStyle(Config.Style);
		});
		check.WaitOne();
		check.Close();
	}

	internal static void Main_DoDraw(On.Terraria.Main.orig_DoDraw orig, Main self, GameTime gameTime)
	{
		// render all terraria
		orig(self, gameTime);

		// Update current state in imgui
		Renderer.BeforeLayout(gameTime);

		// Draw our UI
		ImGuiLayout();
		
		// Call AfterLayout now to finish up and draw all the things
		Renderer.AfterLayout();
		if (!Config.TerrariaMouse || Main.gameMenu)
			return;
		Main.spriteBatch.Begin();
		Main.DrawCursor(Main.DrawThickCursor());
		Main.spriteBatch.End();
	}

	/// <inheritdoc/>
	public sealed override void PostSetupContent()
	{
		if (!CanGui) return;
		
		// idk if this go here
		ImGuiLoader.UpdateHooks();
	}

	// always true
	const bool UseWorkArea = true;

	static void ImGuiLayout()
	{
		// confiugre main dock area
		DockSpace();

		// todo: why dont work in normal style? something is setting padding to 0
		var st = ImGui.GetStyle();
		st.WindowPadding = new(10, 10);

		// draw custom windows
		ImGuiLoader.CustomGUI();
		
		// draw always visible windows
		ImGuiLoader.OverlayGUI();

		//draw debug window
		if (Visible)
		{
			if(Config.ShowMetricsWindow)
				ImGui.ShowMetricsWindow(ref Config.ShowMetricsWindow); 
			DebugWindow(); 
		}

		// draw raws
		ImGuiLoader.BackgroundDraw(ImGui.GetBackgroundDrawList());
		ImGuiLoader.ForeroundDraw(ImGui.GetForegroundDrawList());

		
		InputHelper.Hover = ImGui.IsAnyItemHovered() || ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow | ImGuiHoveredFlags.RootAndChildWindows | ImGuiHoveredFlags.RectOnly | ImGuiHoveredFlags.AllowWhenDisabled);

		var io = ImGui.GetIO();

		InputHelper.Text = ImGui.IsAnyItemFocused() || io.WantTextInput;


		if (!Config.TerrariaMouse || Main.gameMenu)
			// show the mouse if is over a window
			Main.instance.IsMouseVisible = InputHelper.Hover;
	}

	static void DebugWindow()
	{
		if (Main.gameMenu && !ImGuiLoader.RenderDebugInMainMenu) return;
		if (Main.gamePaused && !ImGuiLoader.RenderDebugInPause) return;
		// show the logs
		if(AppLog.ShowAppLog)
		{
			AppLog.Show();
		}

		if (!Config.DebugWindow || !ImGui.Begin("Debug", ref Config.DebugWindow, ImGuiWindowFlags.MenuBar))
			return;
		
		if (ImGui.BeginMenuBar())
		{
			if (ImGui.BeginMenu("Options"))
			{
				ImGui.MenuItem("Log", null, ref AppLog.ShowAppLog);
					
				if (ImGui.MenuItem("Close"))
				{
					Config.DebugWindow = false;
				}
				ImGui.EndMenu();
			}
			ImGui.EndMenuBar();
		}
		// only show fps on debug so that it is not empty
		ImGui.TextWrapped($"Application average {1000f / ImGui.GetIO().Framerate:F3} ms/frame ({ImGui.GetIO().Framerate:F1} FPS)");
		ImGuiLoader.DebugGUI();
		ImGui.End();
	}

	private static unsafe void Image(IntPtr ptr, System.Numerics.Vector2 size, System.Numerics.Vector2 uv0, System.Numerics.Vector2 uv1)
	{
		var start = ImGui.GetCursorScreenPos();
		var end = start + size;
		var bb = new ImVec2[] { start, end };

		ImGuiExNative.igItemSize_Rect(bb, 0);

		if (!ImGuiExNative.igItemAdd(bb, 0))
			return;

		ImGui.GetWindowDrawList().AddImageRounded(ptr, start, end, uv0, uv1, 0xffffffff, 100);

	}

	static void DockSpace()
	{
		var viewport = ImGui.GetMainViewport();
		// dont allow docks in center and make background inivisible.
		var dockspace_flags = ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.NoDockingInCentralNode;
		// the window itself is cand be docked, obvius.
		var windowFlags = ImGuiWindowFlags.NoDocking;
		ImGui.SetNextWindowPos(UseWorkArea ? viewport.WorkPos : viewport.Pos);
		ImGui.SetNextWindowSize(UseWorkArea ? viewport.WorkSize : viewport.Size);
		ImGui.SetNextWindowViewport(viewport.ID);
		// we want our window to by practicaly not interactable, only need to be a dock parent
		windowFlags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
		windowFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;
		windowFlags |= ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration;

		ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, System.Numerics.Vector2.Zero);
		ImGui.Begin("DockSpace Main", windowFlags);
		
		// create de dockspace
		var dockspaceId = ImGui.GetID("MainDockSpace");
		ImGui.DockSpace(dockspaceId, System.Numerics.Vector2.Zero, dockspace_flags);

		ImGui.End();
	}

	static string GetNativePath()
	{
		if (OperatingSystem.IsWindows())
			return "cimgui.dll";
		if (OperatingSystem.IsMacOS())
			return "libcimgui.dylib";
		if (OperatingSystem.IsLinux())
			return "libcimgui.so";
		throw new PlatformNotSupportedException();
	}

	void ConfigureNative()
	{
		// make ImGui resolve the native with a custom resolver
		var nativeByte = GetFileBytes(Path.Combine("lib", GetNativePath()));
		File.WriteAllBytes(CimguiPath, nativeByte);
		NativeLibrary.SetDllImportResolver(typeof(ImGui).Assembly, NativeResolver);
		NativeLibrary.SetDllImportResolver(typeof(ImGUI).Assembly, NativeResolver);
	}

	static IntPtr NativeResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
	{
		if (libraryName != "cimgui")
			return IntPtr.Zero;
		if(_NativeLib != IntPtr.Zero) return _NativeLib;
		_NativeLib =  NativeLibrary.Load(CimguiPath);
		return _NativeLib;

	}

	static void LoadContent()
	{
		// enable docking and mark imgui as loaded
		ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
		_Imguiloaded = true;

		var modloader = typeof(ModLoader);
		var onload = modloader.GetField("OnSuccessfulLoad", BindingFlags.NonPublic | BindingFlags.Static);
		var current = (Action)onload.GetValue(null);
		Action a = () => {
			if(current == null)
				Main.menuMode = 0;
			On.Terraria.Main.DoDraw += ImGUI.Main_DoDraw;
		};

		if (current == null)
			onload.SetValue(null, a);
		else
			onload.SetValue(null, Delegate.Combine(current, a));
	}

	/// <inheritdoc/>
	public override void Unload()
	{
		if (!CanGui) return;
		// stop drawing imgui
		On.Terraria.Main.DoDraw -= Main_DoDraw;
		// reverts
		ImGuiIlEdit.Revert();
		_Imguiloaded = false;
		DebugKey = null;
		Config = null;

		// destroy renderer sources and Free native lib
		Renderer.Unload();
		NativeLibrary.Free(_NativeLib);
		if (FileExists(CimguiPath))
			File.Delete(CimguiPath);
	}

	internal static void UpdateStyle(ImGuiStyle style)
	{
		if (!_Imguiloaded) return;
		switch (style)
		{
			case ImGuiStyle.Terraria:
				SetTerrariaStyle();
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

	static void SetTerrariaStyle()
	{
		var style = ImGui.GetStyle();
		style.Colors[(int)ImGuiCol.Text] = new(0.90f, 0.90f, 0.90f, 1.00f);
		style.Colors[(int)ImGuiCol.TextDisabled] = new(0.60f, 0.60f, 0.60f, 1.00f);
		style.Colors[(int)ImGuiCol.WindowBg] = new(0.27f, 0.29f, 0.53f, 0.78f);
		style.Colors[(int)ImGuiCol.ChildBg] = new(0.00f, 0.00f, 0.00f, 0.00f);
		style.Colors[(int)ImGuiCol.PopupBg] = new(0.09f, 0.05f, 0.25f, 0.92f);
		style.Colors[(int)ImGuiCol.Border] = new(0.00f, 0.00f, 0.00f, 0.50f);
		style.Colors[(int)ImGuiCol.BorderShadow] = new(0.00f, 0.00f, 0.00f, 0.00f);
		style.Colors[(int)ImGuiCol.FrameBg] = new(0.14f, 0.16f, 0.33f, 0.78f);
		style.Colors[(int)ImGuiCol.FrameBgHovered] = new(0.47f, 0.47f, 0.69f, 0.40f);
		style.Colors[(int)ImGuiCol.FrameBgActive] = new(0.42f, 0.41f, 0.64f, 0.69f);
		style.Colors[(int)ImGuiCol.TitleBg] = new(0.27f, 0.27f, 0.54f, 0.83f);
		style.Colors[(int)ImGuiCol.TitleBgActive] = new(0.32f, 0.32f, 0.63f, 0.87f);
		style.Colors[(int)ImGuiCol.TitleBgCollapsed] = new(0.40f, 0.40f, 0.80f, 0.20f);
		style.Colors[(int)ImGuiCol.MenuBarBg] = new(0.40f, 0.40f, 0.55f, 0.80f);
		style.Colors[(int)ImGuiCol.ScrollbarBg] = new(0.28f, 0.35f, 0.62f, 0.69f);
		style.Colors[(int)ImGuiCol.ScrollbarGrab] = new(0.75f, 0.76f, 0.78f, 1.00f);
		style.Colors[(int)ImGuiCol.ScrollbarGrabHovered] = new(1.00f, 1.00f, 1.00f, 1.00f);
		style.Colors[(int)ImGuiCol.ScrollbarGrabActive] = new(1.00f, 1.00f, 1.00f, 1.00f);
		style.Colors[(int)ImGuiCol.CheckMark] = new(0.90f, 0.90f, 0.90f, 0.50f);
		style.Colors[(int)ImGuiCol.SliderGrab] = new(1.00f, 1.00f, 1.00f, 0.30f);
		style.Colors[(int)ImGuiCol.SliderGrabActive] = new(0.41f, 0.39f, 0.80f, 0.60f);
		style.Colors[(int)ImGuiCol.Button] = new(0.35f, 0.40f, 0.61f, 0.62f);
		style.Colors[(int)ImGuiCol.ButtonHovered] = new(0.40f, 0.48f, 0.71f, 0.79f);
		style.Colors[(int)ImGuiCol.ButtonActive] = new(0.46f, 0.54f, 0.80f, 1.00f);
		style.Colors[(int)ImGuiCol.Header] = new(0.40f, 0.40f, 0.90f, 0.45f);
		style.Colors[(int)ImGuiCol.HeaderHovered] = new(0.45f, 0.45f, 0.90f, 0.80f);
		style.Colors[(int)ImGuiCol.HeaderActive] = new(0.53f, 0.53f, 0.87f, 0.80f);
		style.Colors[(int)ImGuiCol.Separator] = new(0.50f, 0.50f, 0.50f, 0.60f);
		style.Colors[(int)ImGuiCol.SeparatorHovered] = new(0.60f, 0.60f, 0.70f, 1.00f);
		style.Colors[(int)ImGuiCol.SeparatorActive] = new(0.70f, 0.70f, 0.90f, 1.00f);
		style.Colors[(int)ImGuiCol.ResizeGrip] = new(1.00f, 1.00f, 1.00f, 0.10f);
		style.Colors[(int)ImGuiCol.ResizeGripHovered] = new(0.78f, 0.82f, 1.00f, 0.60f);
		style.Colors[(int)ImGuiCol.ResizeGripActive] = new(0.78f, 0.82f, 1.00f, 0.90f);
		style.Colors[(int)ImGuiCol.Tab] = new(0.34f, 0.34f, 0.68f, 0.79f);
		style.Colors[(int)ImGuiCol.TabHovered] = new(0.45f, 0.45f, 0.90f, 0.80f);
		style.Colors[(int)ImGuiCol.TabActive] = new(0.40f, 0.40f, 0.73f, 0.84f);
		style.Colors[(int)ImGuiCol.TabUnfocused] = new(0.28f, 0.28f, 0.57f, 0.82f);
		style.Colors[(int)ImGuiCol.TabUnfocusedActive] = new(0.35f, 0.35f, 0.65f, 0.84f);
		style.Colors[(int)ImGuiCol.DockingPreview] = new(0.90f, 0.85f, 0.40f, 0.31f);
		style.Colors[(int)ImGuiCol.DockingEmptyBg] = new(0.20f, 0.20f, 0.20f, 1.00f);
		style.Colors[(int)ImGuiCol.PlotLines] = new(1.00f, 1.00f, 1.00f, 1.00f);
		style.Colors[(int)ImGuiCol.PlotLinesHovered] = new(0.90f, 0.70f, 0.00f, 1.00f);
		style.Colors[(int)ImGuiCol.PlotHistogram] = new(0.90f, 0.70f, 0.00f, 1.00f);
		style.Colors[(int)ImGuiCol.PlotHistogramHovered] = new(1.00f, 0.60f, 0.00f, 1.00f);
		style.Colors[(int)ImGuiCol.TableHeaderBg] = new(0.27f, 0.27f, 0.38f, 1.00f);
		style.Colors[(int)ImGuiCol.TableBorderStrong] = new(0.31f, 0.31f, 0.45f, 1.00f);
		style.Colors[(int)ImGuiCol.TableBorderLight] = new(0.26f, 0.26f, 0.28f, 1.00f);
		style.Colors[(int)ImGuiCol.TableRowBg] = new(0.00f, 0.00f, 0.00f, 0.00f);
		style.Colors[(int)ImGuiCol.TableRowBgAlt] = new(1.00f, 1.00f, 1.00f, 0.07f);
		style.Colors[(int)ImGuiCol.TextSelectedBg] = new(0.00f, 0.00f, 1.00f, 0.35f);
		style.Colors[(int)ImGuiCol.DragDropTarget] = new(1.00f, 1.00f, 0.00f, 0.90f);
		style.Colors[(int)ImGuiCol.NavHighlight] = new(0.45f, 0.45f, 0.90f, 0.80f);
		style.Colors[(int)ImGuiCol.NavWindowingHighlight] = new(1.00f, 1.00f, 1.00f, 0.70f);
		style.Colors[(int)ImGuiCol.NavWindowingDimBg] = new(0.80f, 0.80f, 0.80f, 0.20f);
		style.Colors[(int)ImGuiCol.ModalWindowDimBg] = new(0.20f, 0.20f, 0.20f, 0.35f);
	}
}