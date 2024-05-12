using Microsoft.Xna.Framework;
using System;

namespace ImGUI.Data;

/// <summary>
/// Saves the pointer and the dimensions of a texture, depending on the type of texture also the frames value is valid
/// </summary>
public struct TextureData
{
	/// <summary>
	/// Number of frames of the VERTICAL image.
	/// </summary>
	public int frames;
	/// <summary>
	/// Pointer to de binded image.
	/// </summary>
	public IntPtr ptr;
	/// <summary>
	/// Dimensions of the image.
	/// </summary>
	public Vector2 size;

	/// <summary>
	/// Utility method to display square images, only works with vertical sprite sheets.
	/// </summary>
	/// <param name="tsize">the size of the image.</param>
	/// <returns>The scaled values to fit the square image.</returns>
	public Vector2 Transform(float tsize)
	{
		float Y = size.Y / frames;
        float X = size.X;
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

	/// <summary>
	/// Get the top lef uv of <paramref name="frame"/> for the image.
	/// </summary>
	/// <param name="frame">Frame number.</param>
	/// <returns>The uv required to display this frame.</returns>
	public Vector2 Uv0(int frame)
	{
        float Y = size.Y / frames;
		return new(0, Y * (frame - 1) / size.Y);
	}

	/// <summary>
	/// Get the bottom right uv of <paramref name="frame"/> for the image.
	/// </summary>
	/// <param name="frame">Frame number.</param>
	/// <returns>The uv required to display this frame.</returns>
	public Vector2 Uv1(int frame)
	{
		float Y = size.Y / frames;
		return new(1, Y * frame / size.Y);
	}

	internal Vector2 Transform(float tsize, int verticalFrames = 1, int horizontalFrames = 1)
	{
		float Y = size.Y / verticalFrames;
		float X = size.X / horizontalFrames;
		if (X == Y)
		{
			return new(tsize, tsize);
		}
		if (Y < size.X)
		{
			return new(tsize, Y * tsize / X);
		}
		return new(X * tsize / Y, tsize);
	}

	internal Vector2 Uv0(int verticalFrames, int horizontalFrames, int frameX, int frameY)
	{
		float Y = size.Y / verticalFrames;
        float X = size.X / horizontalFrames;

		float top = Y * (frameY - 1);
		float left = X * (frameX - 1);

		return new(left / size.X, top / size.Y);
	}

	internal Vector2 Uv1(int verticalFrames, int horizontalFrames, int frameX, int frameY)
	{
		float Y = size.Y / verticalFrames;
		float X = size.X / horizontalFrames;

		float bottom = Y * frameY;
		float right = X * frameX;

		return new(right / size.X, bottom / size.Y);
	}
}