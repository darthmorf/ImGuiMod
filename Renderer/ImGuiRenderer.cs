using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ImGuiNET;
using Terraria.ModLoader;
using Terraria;

namespace ImGUI.Renderer;

/// <summary>
/// ImGui renderer for use with XNA-likes (FNA and MonoGame)
/// </summary>
public class ImGuiRenderer
{
	Game _game;

	// Graphics
	readonly GraphicsDevice _graphicsDevice;

	BasicEffect _effect;

	readonly RasterizerState _rasterizerState;

	byte[] _vertexData;

	VertexBuffer _vertexBuffer;

	int _vertexBufferSize;

	byte[] _indexData;

	IndexBuffer _indexBuffer;

	int _indexBufferSize;

	// Textures
	readonly Dictionary<IntPtr, Texture2D> _loadedTextures;

	int _textureId = 0;

	IntPtr? _fontTextureId;

	// Input
	int _scrollWheelValue;

	readonly List<int> _keys = new();

	readonly Mod Mod;

	readonly IntPtr context;

	/// <summary>
	/// Create a new renderer.
	/// </summary>
	public ImGuiRenderer(Mod mod)
	{
		Mod = mod;
		context = ImGui.CreateContext();
		ImGui.SetCurrentContext(context);
		_game = Main.instance;
		_graphicsDevice = _game.GraphicsDevice;

		_loadedTextures = new();

		_rasterizerState = new()
		{
			CullMode = CullMode.None,
			DepthBias = 0,
			FillMode = FillMode.Solid,
			MultiSampleAntiAlias = false,
			ScissorTestEnable = true,
			SlopeScaleDepthBias = 0
		};

		SetupInput();
	}

	#region ImGuiRenderer

	/// <summary>
	/// Creates a texture and loads the font data from ImGui. Should be called when the <see cref="GraphicsDevice" /> is initialized but before any rendering is done
	/// </summary>
	public virtual unsafe void RebuildFontAtlas()
	{
		// Get font texture from ImGui
		var io = ImGui.GetIO();
		io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out var width, out var height, out var bytesPerPixel);

		// Copy the data to a managed array
		var pixels = new byte[width * height * bytesPerPixel];
		unsafe { Marshal.Copy(new(pixelData), pixels, 0, pixels.Length); }

		// Create and register the texture as an XNA texture
		var tex2d = new Texture2D(_graphicsDevice, width, height, false, SurfaceFormat.Color);
		tex2d.SetData(pixels);

		// Should a texture already have been build previously, unbind it first so it can be deallocated
		if (_fontTextureId.HasValue) UnbindTexture(_fontTextureId.Value);

		// Bind the new texture to an ImGui-friendly id
		_fontTextureId = BindTexture(tex2d);

		// Let ImGui know where to find the texture
		io.Fonts.SetTexID(_fontTextureId.Value);
		io.Fonts.ClearTexData(); // Clears CPU side texture data
	}

	/// <summary>
	/// Creates a pointer to a texture, which can be passed through ImGui calls such as <see cref="ImGui.Image(IntPtr, System.Numerics.Vector2)" />. That pointer is then used by ImGui to let us know what texture to draw
	/// </summary>
	public virtual IntPtr BindTexture(Texture2D texture)
	{
		var id = new IntPtr(_textureId++);

		_loadedTextures.Add(id, texture);

		return id;
	}

	/// <summary>
	/// Removes a previously created texture pointer, releasing its reference and allowing it to be deallocated
	/// </summary>
	public virtual void UnbindTexture(IntPtr textureId)
	{
		_loadedTextures.Remove(textureId);
	}

	/// <summary>
	/// Sets up ImGui for a new frame, should be called at frame start
	/// </summary>
	public virtual void BeforeLayout(GameTime gameTime)
	{
		ImGui.GetIO().DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

		UpdateInput();

		ImGui.NewFrame();
	}

	/// <summary>
	/// Asks ImGui for the generated geometry data and sends it to the graphics pipeline, should be called after the UI is drawn using ImGui.** calls
	/// </summary>
	public virtual void AfterLayout()
	{
		ImGui.Render();

		unsafe { RenderDrawData(ImGui.GetDrawData()); }
	}

	#endregion ImGuiRenderer

	#region Setup & Update

	/// <summary>
	/// Maps ImGui keys to XNA keys. We use this later on to tell ImGui what keys were pressed
	/// </summary>
	protected virtual void SetupInput()
	{
		var io = ImGui.GetIO();

		_keys.Add(io.KeyMap[(int)ImGuiKey.Tab] = (int)Keys.Tab);
		_keys.Add(io.KeyMap[(int)ImGuiKey.LeftArrow] = (int)Keys.Left);
		_keys.Add(io.KeyMap[(int)ImGuiKey.RightArrow] = (int)Keys.Right);
		_keys.Add(io.KeyMap[(int)ImGuiKey.UpArrow] = (int)Keys.Up);
		_keys.Add(io.KeyMap[(int)ImGuiKey.DownArrow] = (int)Keys.Down);
		_keys.Add(io.KeyMap[(int)ImGuiKey.PageUp] = (int)Keys.PageUp);
		_keys.Add(io.KeyMap[(int)ImGuiKey.PageDown] = (int)Keys.PageDown);
		_keys.Add(io.KeyMap[(int)ImGuiKey.Home] = (int)Keys.Home);
		_keys.Add(io.KeyMap[(int)ImGuiKey.End] = (int)Keys.End);
		_keys.Add(io.KeyMap[(int)ImGuiKey.Delete] = (int)Keys.Delete);
		_keys.Add(io.KeyMap[(int)ImGuiKey.Backspace] = (int)Keys.Back);
		_keys.Add(io.KeyMap[(int)ImGuiKey.Enter] = (int)Keys.Enter);
		_keys.Add(io.KeyMap[(int)ImGuiKey.Escape] = (int)Keys.Escape);
		_keys.Add(io.KeyMap[(int)ImGuiKey.Space] = (int)Keys.Space);
		_keys.Add(io.KeyMap[(int)ImGuiKey.A] = (int)Keys.A);
		_keys.Add(io.KeyMap[(int)ImGuiKey.C] = (int)Keys.C);
		_keys.Add(io.KeyMap[(int)ImGuiKey.V] = (int)Keys.V);
		_keys.Add(io.KeyMap[(int)ImGuiKey.X] = (int)Keys.X);
		_keys.Add(io.KeyMap[(int)ImGuiKey.Y] = (int)Keys.Y);
		_keys.Add(io.KeyMap[(int)ImGuiKey.Z] = (int)Keys.Z);

		TextInputEXT.TextInput += sendTextuImput;

		ImGui.GetIO().Fonts.AddFontDefault();
	}

	void sendTextuImput(char c)
	{
		if (c == '\t') return;
		ImGui.GetIO().AddInputCharacter(c);

	}

	/// <summary>
	/// Finalize ImGui.
	/// </summary>
	public void Unload()
	{
		TextInputEXT.TextInput -= sendTextuImput;
		// todo: having custom font now crash here, why?
		//ImGui.DestroyContext(context);
		_loadedTextures.Clear();
		_textureId = 1;
	}

	/// <summary>
	/// Updates the <see cref="Effect" /> to the current matrices and texture
	/// </summary>
	protected virtual Effect UpdateEffect(Texture2D texture)
	{
		_effect ??= new(_graphicsDevice);
		
		var io = ImGui.GetIO();

		_effect.World = Matrix.Identity;
		_effect.View = Matrix.Identity;
		_effect.Projection = Matrix.CreateOrthographicOffCenter(0f, io.DisplaySize.X, io.DisplaySize.Y, 0f, -1f, 1f);
		_effect.TextureEnabled = true;
		_effect.Texture = texture;
		_effect.VertexColorEnabled = true;

		return _effect;
	}

	/// <summary>
	/// Sends XNA input state to ImGui
	/// </summary>
	protected virtual void UpdateInput()
	{
		var io = ImGui.GetIO();

		var mouse = Mouse.GetState();
		var keyboard = Keyboard.GetState();

		for (var i = 0; i < _keys.Count; i++)
		{
			io.KeysDown[_keys[i]] = keyboard.IsKeyDown((Keys)_keys[i]);
		}

		io.KeyShift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
		io.KeyCtrl = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
		io.KeyAlt = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
		io.KeySuper = keyboard.IsKeyDown(Keys.LeftWindows) || keyboard.IsKeyDown(Keys.RightWindows);

		io.DisplaySize = new(_graphicsDevice.PresentationParameters.BackBufferWidth, _graphicsDevice.PresentationParameters.BackBufferHeight);
		io.DisplayFramebufferScale = new(1f, 1f);

		io.MousePos = new(mouse.X, mouse.Y);

		io.MouseDown[0] = mouse.LeftButton == ButtonState.Pressed;
		io.MouseDown[1] = mouse.RightButton == ButtonState.Pressed;
		io.MouseDown[2] = mouse.MiddleButton == ButtonState.Pressed;

		var scrollDelta = mouse.ScrollWheelValue - _scrollWheelValue;
		io.MouseWheel = scrollDelta > 0 ? 1 : scrollDelta < 0 ? -1 : 0;
		_scrollWheelValue = mouse.ScrollWheelValue;
	}

	#endregion Setup & Update

	#region Internals

	/// <summary>
	/// Gets the geometry as set up by ImGui and sends it to the graphics device
	/// </summary>
	void RenderDrawData(ImDrawDataPtr drawData)
	{
		// Setup render state: alpha-blending enabled, no face culling, no depth testing, scissor enabled, vertex/texcoord/color pointers
		var lastViewport = _graphicsDevice.Viewport;
		var lastScissorBox = _graphicsDevice.ScissorRectangle;

		_graphicsDevice.BlendFactor = Color.White;
		_graphicsDevice.BlendState = BlendState.NonPremultiplied;
		_graphicsDevice.RasterizerState = _rasterizerState;
		_graphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

		// Handle cases of screen coordinates != from framebuffer coordinates (e.g. retina displays)
		drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

		// Setup projection
		_graphicsDevice.Viewport = new(0, 0, _graphicsDevice.PresentationParameters.BackBufferWidth, _graphicsDevice.PresentationParameters.BackBufferHeight);

		UpdateBuffers(drawData);

		RenderCommandLists(drawData);

		// Restore modified state
		_graphicsDevice.Viewport = lastViewport;
		_graphicsDevice.ScissorRectangle = lastScissorBox;
	}

	unsafe void UpdateBuffers(ImDrawDataPtr drawData)
	{
		if (drawData.TotalVtxCount == 0)
		{
			return;
		}

		// Expand buffers if we need more room
		if (drawData.TotalVtxCount > _vertexBufferSize)
		{
			_vertexBuffer?.Dispose();

			_vertexBufferSize = (int)(drawData.TotalVtxCount * 1.5f);
			_vertexBuffer = new(_graphicsDevice, DrawVertDeclaration.Declaration, _vertexBufferSize, BufferUsage.None);
			_vertexData = new byte[_vertexBufferSize * DrawVertDeclaration.Size];
			Mod.Logger.Info($"Resize vertex buffer to {_vertexBufferSize}");
		}

		if (drawData.TotalIdxCount > _indexBufferSize)
		{
			_indexBuffer?.Dispose();

			_indexBufferSize = (int)(drawData.TotalIdxCount * 1.5f);
			_indexBuffer = new(_graphicsDevice, IndexElementSize.SixteenBits, _indexBufferSize, BufferUsage.None);
			_indexData = new byte[_indexBufferSize * sizeof(ushort)];
			Mod.Logger.Info($"Resize index buffer to {_indexBufferSize}");
		}

		// Copy ImGui's vertices and indices to a set of managed byte arrays
		var vtxOffset = 0;
		var idxOffset = 0;

		for (var n = 0; n < drawData.CmdListsCount; n++)
		{
			var cmdList = drawData.CmdListsRange[n];

			fixed (void* vtxDstPtr = &_vertexData[vtxOffset * DrawVertDeclaration.Size])
			fixed (void* idxDstPtr = &_indexData[idxOffset * sizeof(ushort)])
			{
				Buffer.MemoryCopy((void*)cmdList.VtxBuffer.Data, vtxDstPtr, _vertexData.Length, cmdList.VtxBuffer.Size * DrawVertDeclaration.Size);
				Buffer.MemoryCopy((void*)cmdList.IdxBuffer.Data, idxDstPtr, _indexData.Length, cmdList.IdxBuffer.Size * sizeof(ushort));
			}

			vtxOffset += cmdList.VtxBuffer.Size;
			idxOffset += cmdList.IdxBuffer.Size;
		}

		// Copy the managed byte arrays to the gpu vertex- and index buffers
		_vertexBuffer.SetData(_vertexData, 0, drawData.TotalVtxCount * DrawVertDeclaration.Size);
		_indexBuffer.SetData(_indexData, 0, drawData.TotalIdxCount * sizeof(ushort));
	}

	unsafe void RenderCommandLists(ImDrawDataPtr drawData)
	{
		_graphicsDevice.SetVertexBuffer(_vertexBuffer);
		_graphicsDevice.Indices = _indexBuffer;

		var vtxOffset = 0;
		var idxOffset = 0;

		for (var n = 0; n < drawData.CmdListsCount; n++)
		{
			var cmdList = drawData.CmdListsRange[n];

			for (var cmdi = 0; cmdi < cmdList.CmdBuffer.Size; cmdi++)
			{
				var drawCmd = cmdList.CmdBuffer[cmdi];

				if (drawCmd.ElemCount == 0)
				{
					continue;
				}

				if (!_loadedTextures.ContainsKey(drawCmd.TextureId))
				{
					throw new InvalidOperationException($"Could not find a texture with id '{drawCmd.TextureId}', please check your bindings");
				}

				_graphicsDevice.ScissorRectangle = new(
					(int)drawCmd.ClipRect.X,
					(int)drawCmd.ClipRect.Y,
					(int)(drawCmd.ClipRect.Z - drawCmd.ClipRect.X),
					(int)(drawCmd.ClipRect.W - drawCmd.ClipRect.Y)
				);

				var effect = UpdateEffect(_loadedTextures[drawCmd.TextureId]);

				foreach (var pass in effect.CurrentTechnique.Passes)
				{
					pass.Apply();

#pragma warning disable CS0618 // // FNA does not expose an alternative method.
					_graphicsDevice.DrawIndexedPrimitives(
						primitiveType: PrimitiveType.TriangleList,
						baseVertex: (int)drawCmd.VtxOffset + vtxOffset,
						minVertexIndex: 0,
						numVertices: cmdList.VtxBuffer.Size,
						startIndex: (int)drawCmd.IdxOffset + idxOffset,
						primitiveCount: (int)drawCmd.ElemCount / 3
					);
#pragma warning restore CS0618
				}
			}

			vtxOffset += cmdList.VtxBuffer.Size;
			idxOffset += cmdList.IdxBuffer.Size;
		}
	}

	#endregion Internals
}
