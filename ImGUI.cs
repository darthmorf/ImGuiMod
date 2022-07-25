using ImGUI.Renderer;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Terraria;
using Terraria.ModLoader;

namespace ImGUI;

public class ImGUI : Mod
{
	private static bool _imguiloaded;
	public static ImGuiRenderer renderer;
	private Texture2D _xnaTexture;
	private IntPtr _imGuiTexture;

	public static ModKeybind DebugKey;
	public static ModKeybind WindowInfoKey;

	public static ImGUIConfig Config;

	private static IntPtr NativeLib;

	public override void Load()
	{
		Config = ModContent.GetInstance<ImGUIConfig>();
		DebugKey = KeybindLoader.RegisterKeybind(this, "Debug Window", Keys.F6);
		WindowInfoKey = KeybindLoader.RegisterKeybind(this, "General Info Window", Keys.F5);

		ConfigureNative();

		log4net.Config.BasicConfigurator.Configure(new ImGuiAppender());

		Main.RunOnMainThread(() =>
		{
			renderer = new ImGuiRenderer(Main.instance, this);
			renderer.RebuildFontAtlas();

			LoadContent();
			On.Terraria.Main.DoDraw += Main_DoDraw;
			UpdateStyle(Config.Style);
		});

	}

	private void Main_DoDraw(On.Terraria.Main.orig_DoDraw orig, Main self, GameTime gameTime)
	{
		orig(self, gameTime);

		renderer.BeforeLayout(gameTime);

		// Draw our UI
		ImGuiLayout();
		
		// Call AfterLayout now to finish up and draw all the things
		renderer.AfterLayout();	
	}

	public override void PostSetupContent()
	{
		ImGuiLoader.UpdateHooks();
	}

	const bool use_work_area = true;

	private void ImGuiLayout()
	{
		DockSpace();

		DebugWindow();


		// draw custom windows
		ImGuiLoader.CustomGUI();
		// draw raws
		ImGuiLoader.BackgroundDraw(ImGui.GetBackgroundDrawList());
		ImGuiLoader.ForeroundDraw(ImGui.GetForegroundDrawList());

		// show the mouse if is over a window
		Main.instance.IsMouseVisible = ImGui.IsAnyItemHovered()	|| ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow);
	}

	private void DebugWindow()
	{
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
			ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));
			ImGuiLoader.DebugGUI();
			ImGui.End();
		}
	}

	private static void DockSpace()
	{
		var viewport = ImGui.GetMainViewport();
		var dockspace_flags = ImGuiDockNodeFlags.PassthruCentralNode | ImGuiDockNodeFlags.NoDockingInCentralNode;
		var window_flags = ImGuiWindowFlags.NoDocking;
		ImGui.SetNextWindowPos(use_work_area ? viewport.WorkPos : viewport.Pos);
		ImGui.SetNextWindowSize(use_work_area ? viewport.WorkSize : viewport.Size);
		ImGui.SetNextWindowViewport(viewport.ID);
		window_flags |= ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove;
		window_flags |= ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoNavFocus;
		window_flags |= ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoDecoration;

		ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, System.Numerics.Vector2.Zero);
		ImGui.Begin("DockSpace Main", window_flags);

		var dockspace_id = ImGui.GetID("MainDockSpace");
		ImGui.DockSpace(dockspace_id, System.Numerics.Vector2.Zero, dockspace_flags);

		ImGui.End();
	}

	public static string ImGuiPath => Path.Combine(Main.SavePath, "ImGui");

	public static string CimguiPath => Path.Combine(ImGuiPath, GetNativePath());

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
		Directory.CreateDirectory(ImGuiPath);
		byte[] nativeByte = GetFileBytes(Path.Combine("lib", GetNativePath()));
		File.WriteAllBytes(CimguiPath, nativeByte);
		NativeLibrary.SetDllImportResolver(typeof(ImGui).Assembly, NativeResolver);
	}

	private IntPtr NativeResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
	{
		if (libraryName == "cimgui")
		{
			if(NativeLib != IntPtr.Zero)
			{
				return NativeLib;
			}
			NativeLib =  NativeLibrary.Load(CimguiPath);
			return NativeLib;
		}

		return IntPtr.Zero;
	}

	private void LoadContent()
	{
		_xnaTexture = CreateTexture(Main.instance.GraphicsDevice, 300, 150, pixel =>
		{
			var red = (pixel % 300) / 2;
			return new Color(red, 1, 1);
		});

		// Then, bind it to an ImGui-friendly pointer, that we can use during regular ImGui.** calls
		_imGuiTexture = renderer.BindTexture(_xnaTexture);

		ImGui.GetIO().ConfigFlags |= ImGuiConfigFlags.DockingEnable;
		_imguiloaded = true;
	}

	public static Texture2D CreateTexture(GraphicsDevice device, int width, int height, Func<int, Color> paint)
	{
		//initialize a texture
		var texture = new Texture2D(device, width, height);

		//the array holds the color for each pixel in the texture
		Color[] data = new Color[width * height];
		for (var pixel = 0; pixel < data.Length; pixel++)
		{
			//the function applies the color according to the specified pixel
			data[pixel] = paint(pixel);
		}

		//set the color
		texture.SetData(data);

		return texture;
	}

	public override void Unload()
	{
		_imguiloaded = false;
		DebugKey = null;
		WindowInfoKey = null;
		On.Terraria.Main.DoDraw -= Main_DoDraw;
		Config = null;

		renderer.UnbindTexture(_imGuiTexture);
		NativeLibrary.Free(NativeLib);
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