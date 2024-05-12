using ImGuiMod.Internals;
using ImGuiMod.Renderer;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;

namespace ImGuiMod;

/// <summary>
/// ImGUI mod, used to access de current ImGuiRenderer.
/// </summary>
public class ImGUIMod : Mod
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
		ToggleImGui = KeybindLoader.RegisterKeybind(this, "Toggle ImGui", Keys.F5);
		
		ConfigureNative();

		// add logger appender to Terraria
		log4net.Config.BasicConfigurator.Configure(new ImGuiAppender());

		ImGuiIlEdit.Apply();

        AutoResetEvent check = new AutoResetEvent(false);

		//  execute in main thread to RebuildFontAtlas, background threads cant build font atlas
		Main.RunOnMainThread(() =>
		{
			// create and configure the renderer
			Renderer = new(this);

            ImGuiIOPtr io = ImGui.GetIO();
            byte[] fontBytes = GetFileBytes("extras/FONT.TTF");
            GCHandle pinnedArray = GCHandle.Alloc(fontBytes, GCHandleType.Pinned);
			nint pointer = pinnedArray.AddrOfPinnedObject();
            ImFontPtr terrariaFont = io.Fonts.AddFontFromMemoryTTF(pointer, fontBytes.Length, 20);
			unsafe
			{
				io.NativePtr->FontDefault = terrariaFont.NativePtr;
			}
			Renderer.RebuildFontAtlas();

			pinnedArray.Free();

			LoadContent();

			check.Set();
		});
		check.WaitOne();
		check.Close();
	}

	internal static void Main_DoDraw(Terraria.On_Main.orig_DoDraw orig, Main self, GameTime gameTime)
	{
		// Update current state in imgui
		Renderer.BeforeLayout(gameTime);

		// Draw our UI
		DrawImGui();

		// render all terraria
		orig(self, gameTime);

		// draw raws
		ImGuiLoader.BackgroundDraw(ImGui.GetBackgroundDrawList());
		ImGuiLoader.ForegroundDraw(ImGui.GetForegroundDrawList());

		// Call AfterLayout now to finish up and draw all the things
		Renderer.AfterLayout();
		if (!Config.TerrariaMouse)
			return;

		//PlayerInput.SetZoom_Unscaled();
		PlayerInput.SetZoom_UI();
		Main.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null, Main.UIScaleMatrix);
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

	static void DrawImGui()
	{
		// configure main dock area
		CreateMainDockableArea();

		// draw custom windows
		ImGuiLoader.StandardDraw();

		//  TODO - Per window hover logic
        InputHelper.ImGuiHasHover = ImGui.IsAnyItemHovered() || ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow | ImGuiHoveredFlags.RootAndChildWindows | ImGuiHoveredFlags.AllowWhenBlockedByActiveItem | ImGuiHoveredFlags.AllowWhenBlockedByPopup);

        ImGuiIOPtr io = ImGui.GetIO();
		InputHelper.Text = ImGui.IsAnyItemFocused() || io.WantTextInput;

		if (!Config.TerrariaMouse)
			// show the mouse if is over a window
			Main.instance.IsMouseVisible = InputHelper.ImGuiHasHover;
	}

	private static unsafe void Image(IntPtr ptr, Vector2 size, Vector2 uv0, Vector2 uv1)
	{
        Vector2 start = ImGui.GetCursorScreenPos();
        Vector2 end = start + size;
        Vector2[] bb = new Vector2[] { start, end };

		ImGuiExNative.igItemSize_Rect(bb, 0);

		if (!ImGuiExNative.igItemAdd(bb, 0))
			return;

		ImGui.GetWindowDrawList().AddImageRounded(ptr, start, end, uv0, uv1, 0xffffffff, 100);

	}

	static void CreateMainDockableArea()
	{
        ImGuiViewportPtr viewport = ImGui.GetMainViewport();
        // dont allow docks in center and make background invisible.
        ImGuiDockNodeFlags dockspace_flags = ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.NoDockingInCentralNode;
        // the window itself is cand be docked
        ImGuiWindowFlags windowFlags = ImGuiWindowFlags.NoDocking;
		ImGui.SetNextWindowPos(UseWorkArea ? viewport.WorkPos : viewport.Pos);
		ImGui.SetNextWindowSize(UseWorkArea ? viewport.WorkSize : viewport.Size);
		ImGui.SetNextWindowViewport(viewport.ID);
		// we want our window to by practicaly not interactable, only need to be a dock parent
		windowFlags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
		windowFlags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;
		windowFlags |= ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration;

		ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
		ImGui.Begin("DockSpace Main", windowFlags);

        // create the dockspace
        uint dockspaceId = ImGui.GetID("MainDockSpace");
		ImGui.DockSpace(dockspaceId, Vector2.Zero, dockspace_flags);

		ImGui.End();
		ImGui.PopStyleVar();
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
        byte[] nativeByte = GetFileBytes(Path.Combine("lib", GetNativePath()));
		File.WriteAllBytes(CimguiPath, nativeByte);
		NativeLibrary.SetDllImportResolver(typeof(ImGui).Assembly, NativeResolver);
		NativeLibrary.SetDllImportResolver(typeof(ImGUIMod).Assembly, NativeResolver);
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

        Type modloader = typeof(ModLoader);
        FieldInfo onload = modloader.GetField("OnSuccessfulLoad", BindingFlags.NonPublic | BindingFlags.Static);
        Action current = (Action)onload.GetValue(null);
		Action a = () => {
			if(current == null)
				Main.menuMode = 0;
            Terraria.On_Main.DoDraw += ImGUIMod.Main_DoDraw;
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
		Terraria.On_Main.DoDraw -= Main_DoDraw;
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

	internal static void SetWindowStyle(ImGuiStyle style)
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

        ImGuiStylePtr st = ImGui.GetStyle();
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
        ImGuiStylePtr style = ImGui.GetStyle();
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
		style.Colors[(int)ImGuiCol.Button] = new(0.41f, 0.41f, 0.93f, 0.62f);
		style.Colors[(int)ImGuiCol.ButtonHovered] = new(0.41f, 0.41f, 0.93f, 0.80f);
		style.Colors[(int)ImGuiCol.ButtonActive] = new(0.43f, 0.43f, 0.93f, 1.00f);
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
		style.Colors[(int)ImGuiCol.TabActive] = new(0.41f, 0.41f, 0.93f, 0.87f);
		style.Colors[(int)ImGuiCol.TabUnfocused] = new(0.23f, 0.23f, 0.52f, 0.82f);
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