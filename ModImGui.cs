using ImGuiNET;
using Terraria.ModLoader;

namespace ImGUI;


public abstract class ModImGui : ModType
{
	public ushort Index { get; set; }

	public static ushort NextIndex;

	public sealed override void Load()
	{

	}

	protected sealed override void Register()
	{
		ModTypeLookup<ModImGui>.Register(this);
		Index = NextIndex++;
		ImGuiLoader.guis.Add(this);

	}

	public sealed override void SetupContent()
	{
		SetStaticDefaults();
	}

	public sealed override void Unload()
	{

	}

	/// <summary>
	/// Use ImGui.** statements that you want to render in the Debug window.
	/// </summary>
	public virtual void DebugGUI()
	{

	}

	/// <summary>
	/// Create own separate windows.Don't leave ImGui.** instructions on the air, make sure to create a window.
	/// </summary>
	public virtual void CustomGUI()
	{

	}

	/// <summary>
	/// Use it to draw things behind all ImGUI windows but above terraria, useful for primitives like hitboxes.
	/// </summary>
	public virtual void BackgroundDraw(ImDrawListPtr drawList)
	{

	}

	/// <summary>
	/// Use it to draw things on top of ImGUI windows.
	/// </summary>
	public virtual void ForeroundDraw(ImDrawListPtr drawList)
	{

	}
}