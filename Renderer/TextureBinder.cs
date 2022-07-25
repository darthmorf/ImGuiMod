using ImGUI.Data;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace ImGUI.Renderer;

public class TextureBinder
{
	private class TextureBinderSystem : ModSystem
	{
		public override void PostSetupContent()
		{
			npcs = new(TextureAssets.Npc, t => Main.npcFrameCount[t]);
			proj = new(TextureAssets.Projectile, t => Main.projFrames[t]);
			item = new(TextureAssets.Item, t => Main.itemAnimations[t]?.FrameCount ?? 1);
			gore = new(TextureAssets.Gore, t => 1);
			buff = new(TextureAssets.Buff, t => 1);
		}
	}

	public static TextureBinder npcs;
	public static TextureBinder proj;
	public static TextureBinder item;
	public static TextureBinder gore;
	public static TextureBinder buff;

	private TextureData[] binds;
	private Func<int, int> framecount;
	private Asset<Texture2D>[] source;

	private TextureBinder(Asset<Texture2D>[] npc, Func<int, int> framesResolver)
	{
		source = npc;
		binds = new TextureData[npc.Length];
		framecount = framesResolver;
	}

	public TextureData GetTextureData(int type) => binds[type];

	public void BindTextureData(int type)
	{
		if (binds[type].ptr != IntPtr.Zero)
			ImGUI.renderer.UnbindTexture(binds[type].ptr);
		var texture = source[type].Value;
		binds[type] = new TextureData
		{
			ptr = ImGUI.renderer.BindTexture(texture),
			size = texture.Size(),
			frames = framecount(type)
		};
	}

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
