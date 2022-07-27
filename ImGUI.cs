using ImGUI.Renderer;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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
	// renderer
	private static ImGuiRenderer renderer;

	/// <summary>
	/// The current renderer, use to bind textures to use in imgui. 
	/// </summary>
	public static ImGuiRenderer Renderer { get => renderer; private set => renderer = value; }


	// renderer state
	private static bool _imguiloaded;

	// config
	internal static ModKeybind DebugKey;
	internal static ImGUIConfig Config;

	// native lib
	private static IntPtr NativeLib;

	private static string CimguiPath = Path.GetTempFileName();

	public static bool CanGui => !Main.dedServ && Main.netMode != NetmodeID.Server;

	/// <inheritdoc/>
	public override void Load()
	{
		if (!CanGui) return;
		// configs
		Config = ModContent.GetInstance<ImGUIConfig>();
		DebugKey = KeybindLoader.RegisterKeybind(this, "Debug Window", Keys.F6);

		ConfigureNative();

		// add logger appender to Terraria
		log4net.Config.BasicConfigurator.Configure(new ImGuiAppender());

		//  execute in main thread to RebuildFontAtlas, background threads cant build font atlas
		Main.RunOnMainThread(() =>
		{
			// create and configure the renderer
			renderer = new ImGuiRenderer(this);
			renderer.RebuildFontAtlas();

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

		// Update current state in imgui
		renderer.BeforeLayout(gameTime);

		// Draw our UI
		ImGuiLayout();
		
		// Call AfterLayout now to finish up and draw all the things
		renderer.AfterLayout();	
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

		// draw custom windows
		ImGuiLoader.CustomGUI();

		//draw debug window
		DebugWindow();

		// draw raws
		ImGuiLoader.BackgroundDraw(ImGui.GetBackgroundDrawList());
		ImGuiLoader.ForeroundDraw(ImGui.GetForegroundDrawList());

		// todo: add the ability to choose terraria mouse?
		// show the mouse if is over a window
		Main.instance.IsMouseVisible = ImGui.IsAnyItemHovered()	|| ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow);
	}

	private void DebugWindow()
	{
		// show the logs
		if(AppLog.show_app_log)
		{
			AppLog.Show();
		}
			
		if (Config.DebugWindow && ImGui.Begin("Debug", ImGuiWindowFlags.MenuBar))
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
	}
}