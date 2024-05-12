using ImGuiNET;
using Terraria.ModLoader;

namespace ImGUI;

/// <summary>
/// Base class to put ImGui.** calls
/// </summary>
public abstract class ModImGui : ModType
{
	/// <summary>
	/// Indicates that this custom gui should be rendered even when the main imgui is hidden.
	/// </summary>
	public virtual bool AlwaysVisible => false;

	/// <summary>
	/// Indicates that this gui should be rendered even in the main menu.
	/// </summary>
	public virtual bool RenderInMainMenu => false;

	/// <summary>
	/// Indicates that this gui should be rendered even in the game is paused.
	/// </summary>
	public virtual bool RenderInPause => false;
	
	internal ushort Index { get; set; }

	static ushort _NextIndex;

	/// <inheritdoc/>
	protected sealed override void Register()
	{
		ModTypeLookup<ModImGui>.Register(this);
		Index = _NextIndex++;
		ImGuiLoader.guis.Add(this);	
	}

	/// <inheritdoc/>
	public sealed override bool IsLoadingEnabled(Mod mod)
	{
		return ImGUI.CanGui;
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
	/// same as CustomGUI but things are not hidden by hiding ImGUI
	/// </summary>
	public virtual void OverlayGUI()
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