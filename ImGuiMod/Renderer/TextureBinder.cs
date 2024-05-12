using ImGuiMod.Data;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ImGuiMod.Renderer;

/// <summary>
/// Allow you to get the textures of common Terraria arrays
/// </summary>
public class TextureBinder
{
	class TextureBinderSystem : ModSystem
	{
		public override void PostSetupContent()
		{
			npcs = new(TextureAssets.Npc, t => Main.npcFrameCount[t]);
			proj = new(TextureAssets.Projectile, t => Main.projFrames[t]);
			item = new(TextureAssets.Item, t => Main.itemAnimations[t]?.FrameCount ?? 1);
			gore = new(TextureAssets.Gore, t => 1);
			buff = new(TextureAssets.Buff, t => 1);
			tile = new(TextureAssets.Tile, t => 1);
		}
	}

	/// <summary>
	/// <see cref="TextureAssets.Npc"/> binder.
	/// </summary>
	public static TextureBinder npcs;
	/// <summary>
	/// <see cref="TextureAssets.Projectile"/> binder.
	/// </summary>
	public static TextureBinder proj;
	/// <summary>
	/// <see cref="TextureAssets.Item"/> binder.
	/// </summary>
	public static TextureBinder item;
	/// <summary>
	/// <see cref="TextureAssets.Gore"/> binder.
	/// </summary>
	public static TextureBinder gore;
	/// <summary>
	/// <see cref="TextureAssets.Buff"/> binder.
	/// </summary>
	public static TextureBinder buff;
	/// <summary>
	/// <see cref="TextureAssets.Tile"/> binder.
	/// </summary>
	public static TextureBinder tile;

	readonly TextureData[] binds;

	readonly Func<int, int> framecount;

	readonly Asset<Texture2D>[] source;

	TextureBinder(Asset<Texture2D>[] npc, Func<int, int> framesResolver)
	{
		source = npc;
		binds = new TextureData[npc.Length];
		framecount = framesResolver;
	}

	/// <summary>
	/// Get a binded texture by type.
	/// </summary>
	public TextureData GetTextureData(int type) => binds[type];

	/// <summary>
	/// Bind a texture,if is already binded then unbind and bind again.
	/// </summary>
	public void BindTextureData(int type)
	{
		if (binds[type].ptr != IntPtr.Zero)
			ImGUIMod.Renderer.UnbindTexture(binds[type].ptr);
        Asset<Texture2D> asset = source[type];
		if (!asset.IsLoaded)
			Main.Assets.Request<Texture2D>(asset.Name, AssetRequestMode.ImmediateLoad);
        Texture2D texture = asset.Value;
		binds[type] = new()
		{
			ptr = ImGUIMod.Renderer.BindTexture(texture),
			size = texture.Size(),
			frames = framecount(type)
		};
	}

	/// <summary>
	/// Get or bind the texture.
	/// </summary>
	public TextureData this[int type]
	{
		get
		{
			if (binds[type].ptr != IntPtr.Zero)
				return binds[type];
			BindTextureData(type);
			return binds[type];
		}
	}
}
