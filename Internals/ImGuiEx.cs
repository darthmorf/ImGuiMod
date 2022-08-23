using ImGUI.Data;
using ImGUI.Internals;
using System;
using System.Text;
using Terraria;

namespace ImGuiNET;

#pragma warning disable CS1591
public static class ImGuiEx
{
	public unsafe static ImGuiListClipperPtr ListClipper()
	{
		return new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper_ImGuiListClipper());
	}

	public unsafe static bool BeginTabItem(string label, ImGuiTabItemFlags flags)
	{
		int num = 0;
		byte* ptr;
		if (label != null)
		{
			num = Encoding.UTF8.GetByteCount(label);

			if (num <= Util.StackAllocationSizeLimit)
			{
				var stac = stackalloc byte[num + 1];
				ptr = stac;
			}
			else
			{
				ptr = Util.Allocate(num + 1);
			}
			int utf = Util.GetUtf8(label, ptr, num);
			ptr[utf] = 0;
		}
		else
		{
			ptr = null;
		}

		byte* p_open2 = null;
		byte num2 = ImGuiNative.igBeginTabItem(ptr, p_open2, flags);
		if (num > Util.StackAllocationSizeLimit)
		{
			Util.Free(ptr);
		}
		return num2 != 0;
	}

	public static void ImageFrame(IntPtr texture, float size, int verticalFrames = 1, int horizontalFrames = 1, int frameX = 1, int frameY = 1)
	{
		var tex = ImGUI.ImGUI.Renderer._loadedTextures[texture];
		var data = new TextureData
		{
			ptr = texture,
			size = tex.Size(),
			frames = 1
		};
		ImageFrame(data, verticalFrames, horizontalFrames, frameX, frameY);
	}

	public static void ImageFrame(TextureData data, float size, int verticalFrames = 1, int horizontalFrames = 1, int frameX = 1, int frameY = 1)
	{
		var tsize = data.Transform(size, verticalFrames, horizontalFrames);
		var uv0 = data.Uv0(verticalFrames, horizontalFrames, frameX, frameY);
		var uv1 = data.Uv1(verticalFrames, horizontalFrames, frameX, frameY);
		ImGui.Image(data.ptr, tsize, uv0, uv1);
	}

	public static void ImageFrame(IntPtr texture, ImVec2 size, int verticalFrames = 1, int horizontalFrames = 1, int frameX = 1, int frameY = 1)
	{
		var tex = ImGUI.ImGUI.Renderer._loadedTextures[texture];
		var data = new TextureData
		{
			ptr = texture,
			size = tex.Size(),
			frames = 1
		};
		ImageFrame(data, verticalFrames, horizontalFrames, frameX, frameY);
	}

	public static void ImageFrame(TextureData data, ImVec2 size, int verticalFrames = 1, int horizontalFrames = 1, int frameX = 1, int frameY = 1)
	{
		var uv0 = data.Uv0(verticalFrames, horizontalFrames, frameX, frameY);
		var uv1 = data.Uv1(verticalFrames, horizontalFrames, frameX, frameY);
		ImGui.Image(data.ptr, size, uv0, uv1);
	}

	public static void SetNextWindowTitleAnimated()
	{
		ImGuiIlEdit.NextWindowAnimatedTitle = true;
	}

	public static void SetNextWindowId(string id)
	{
		ImGuiIlEdit.NextWindowID = id;
	}

}
#pragma warning restore CS1591