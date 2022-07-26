using ImGuiNET;
using Terraria.ModLoader;

namespace ImGUI;

/// <summary>
/// Base class to put ImGui.** calls
/// </summary>
public abstract class ModImGui : ModType
{
	internal ushort Index { get; set; }

	private static ushort NextIndex;

	/// <inheritdoc/>
	protected sealed override void Register()
	{
		ModTypeLookup<ModImGui>.Register(this);
		Index = NextIndex++;
		ImGuiLoader.guis.Add(this);

	}

	/// <inheritdoc/>
	public sealed override void SetupContent()
	{
		SetStaticDefaults();
	}

	/// <summary>
	/// Use ImGui.** statements that you want to render in the Debug window.
	/// </summary>
	public virtual void DebugGUI()
	{

	}

	/// <summary>
	/// Create own separate windows. Don't leave ImGui.** instructions on the air, make sure to create a window.
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