using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;

namespace ImGuiNET;

public unsafe static class ImGuiExNative
{
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	public static extern void igItemSize_Rect(Vector2[] size, float text_baseline_y);

	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool igItemAdd(Vector2[] bb, uint id, Vector2[] _bb = null, int extra_flags = 0);
}
