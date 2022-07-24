//using Terraria.ModLoader;

using Terraria.ModLoader;

namespace ImGUI;

public struct ImTabInfo
{
	string name;

}

public abstract class ModImGui : ModType, IIndexed
{
	public ushort Index { get; set; }

	public static ushort NextIndex;

	public sealed override void Load()
	{

	}

	private ImTabInfo? TabInfo;

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
	/// Render a tab for your mod in the main ImGUI window, must set TabInfo in SetStaticDefaults.
	/// </summary>
	private void TabGUI()
	{

	}

	/// <summary>
	/// Create own separate windows.Don't leave ImGui.** instructions on the air, make sure to create a window
	/// </summary>
	public virtual void CustomGUI()
	{

	}
}