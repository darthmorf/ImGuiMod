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
	public System.Numerics.Vector2 Transform(int tsize)
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

	/// <summary>
	/// Get the top lef uv of <paramref name="frame"/> for the image.
	/// </summary>
	/// <param name="frame">Frame number.</param>
	/// <returns>The uv required to display this frame.</returns>
	public System.Numerics.Vector2 Uv0(int frame)
	{
		var Y = size.Y / frames;
		return new(0, Y * (frame - 1) / size.Y);
	}

	/// <summary>
	/// Get the bottom right uv of <paramref name="frame"/> for the image.
	/// </summary>
	/// <param name="frame">Frame number.</param>
	/// <returns>The uv required to display this frame.</returns>
	public System.Numerics.Vector2 Uv1(int frame)
	{
		var Y = size.Y / frames;
		return new(1, Y * frame / size.Y);
	}
}