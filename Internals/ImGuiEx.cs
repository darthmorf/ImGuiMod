using ImGUI.Internals;
using System.Text;

namespace ImGuiNET;

public static class ImGuiEx
{
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

	public static void SetNextWindowTitleAnimated()
	{
		ImGuiIlEdit.NextWindowAnimatedTitle = true;
	}

	public static void SetNextWindowId(string id)
	{
		ImGuiIlEdit.NextWindowID = id;
	}

}
