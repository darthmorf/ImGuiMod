using System.Runtime.InteropServices;
using System.Text;
using System;

namespace ImGUI.Internals;

public class Util
{
	public const int StackAllocationSizeLimit = 2048;

	internal unsafe static byte* Allocate(int byteCount)
	{
		return (byte*)Marshal.AllocHGlobal(byteCount);
	}

	internal unsafe static void Free(byte* ptr)
	{
		Marshal.FreeHGlobal((IntPtr)ptr);
	}

	internal unsafe static int GetUtf8(string s, byte* utf8Bytes, int utf8ByteCount)
	{
		fixed (char* ptr = s)
		{
			char* chars = ptr;
			return Encoding.UTF8.GetBytes(chars, s.Length, utf8Bytes, utf8ByteCount);
		}
	}
}
