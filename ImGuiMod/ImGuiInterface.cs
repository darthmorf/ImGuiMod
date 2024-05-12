using ImGuiNET;
using Terraria.ModLoader;

namespace ImGuiMod;

/// <summary>
/// Base class to put ImGui.** calls
/// </summary>
public abstract class ImGuiInterface : ModType
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
		ModTypeLookup<ImGuiInterface>.Register(this);
		Index = _NextIndex++;
		ImGuiLoader.guis.Add(this);	
	}

	/// <inheritdoc/>
	public sealed override bool IsLoadingEnabled(Mod mod)
	{
		return ImGUIMod.CanGui;
	}

	/// <inheritdoc/>
	public sealed override void SetupContent()
	{
		SetStaticDefaults();
	}

    /// <summary>
    /// Used to draw standard ImGui elements. Most of your UI logic should go here.
    /// </summary>
    public virtual void StandardDraw()
    {

    }

    /// <summary>
    /// Used to draw elements behind all ImGUI windows but above Terraria, useful for primitives like hitboxes.
    /// </summary>
    public virtual void BackgroundDraw(ImDrawListPtr drawList)
    {

    }

    /// <summary>
    /// Used to draw elements in front of all standard ImGui elements
    /// </summary>
    public virtual void ForegroundDraw(ImDrawListPtr drawList)
    {

    }
}