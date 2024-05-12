using ImGuiNET;
using Terraria.ModLoader;

namespace ImGuiMod;

/// <summary>
/// Base class to put ImGui.** calls
/// </summary>
public abstract class ImGuiInterface : ModType
{
    /// <summary>
    /// Style to use when rendering windows in this interface
    /// </summary>
    public virtual ImGuiStyle Style => ImGuiStyle.Terraria;

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