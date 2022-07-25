using Microsoft.Xna.Framework;
using System;

namespace ImGUI;

public struct TextureData
{
	public int frames;
	public IntPtr ptr;
	public Vector2 size;

	internal System.Numerics.Vector2 Transform(int tsize)
	{
		var Y = size.Y / frames;
		var X = size.X;
		if (X == Y)
		{
			return new(tsize, tsize);
		}
		if(Y < size.X)
		{
			return new(tsize, Y * tsize / X);
		}
		return new(X * tsize / Y, tsize);

	}

	internal System.Numerics.Vector2 Uv0(int frame)
	{
		var Y = size.Y / frames;
		return new(0, Y * (frame - 1) / size.Y);
	}

	internal System.Numerics.Vector2 Uv1(int frame)
	{
		var Y = size.Y / frames;
		return new(1, Y * frame / size.Y);
	}
}