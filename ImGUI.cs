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
	public static ImGuiRenderer renderer;
	private Texture2D _xnaTexture;
	private IntPtr _imGuiTexture;

	public static ModKeybind DebugKey;
	public static ModKeybind WindowInfoKey;

	public static ImGUIConfig Config = ModContent.GetInstance<ImGUIConfig>();

	public override void Load()
	{
		DebugKey = KeybindLoader.RegisterKeybind(this, "Debug Window", Keys.F6);
		WindowInfoKey = KeybindLoader.RegisterKeybind(this, "General Info Window", Keys.F5);

		ConfigureNative();

		Main.RunOnMainThread(() =>
		{
			renderer = new ImGuiRenderer(Main.instance);
			renderer.RebuildFontAtlas();

			LoadContent();
			On.Terraria.Main.DoDraw += Main_DoDraw;
			log4net.Config.BasicConfigurator.Configure(new ImGuiAppender());
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

		// Redraw the mouse so that it is on top of ImGui
		Main.spriteBatch.Begin();
		Main.DrawCursor(Main.DrawThickCursor());
		Main.spriteBatch.End();
	}

	public override void PostSetupContent()
	{
		ImGuiLoader.UpdateHooks();
	}

	private void ImGuiLayout()
	{
		if(Config.DebugWindow && ImGui.Begin("Debug"))
		{
			ImGui.Text(string.Format("Application average {0:F3} ms/frame ({1:F1} FPS)", 1000f / ImGui.GetIO().Framerate, ImGui.GetIO().Framerate));
			ImGuiLoader.DebugGUI();
			ImGui.End();
		}
		// draw default window
		WindowInfo.GUI();
		// draw custom windows
		ImGuiLoader.CustomGUI();
	}

	public static string ImGuiPath => Path.Combine(Main.SavePath, "ImGui");

	public static string CimguiPath => Path.Combine(ImGuiPath, GetNativePath());

	private static string GetNativePath()
	{
		if (OperatingSystem.IsWindows())
			return "cimgui.dll";
		throw new NotImplementedException();
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
			return NativeLibrary.Load(CimguiPath);
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
		DebugKey = null;
		WindowInfoKey = null;
		On.Terraria.Main.DoDraw -= Main_DoDraw;
		renderer.UnbindTexture(_imGuiTexture);
	}
}