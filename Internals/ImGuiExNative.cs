global using ImVec2 = System.Numerics.Vector2;
using System.Runtime.InteropServices;

namespace ImGuiNET;

public unsafe static class ImGuiExNative
{
	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	public static extern void igItemSize_Rect(ImVec2[] size, float text_baseline_y);

	[DllImport("cimgui", CallingConvention = CallingConvention.Cdecl)]
	public static extern bool igItemAdd(ImVec2[] bb, uint id, ImVec2[] _bb = null, int extra_flags = 0);
}
