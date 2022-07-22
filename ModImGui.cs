using Terraria.ModLoader;

namespace ImGUI;


public abstract class ModImGui : ModType, IIndexed
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

	}

	public sealed override void Unload()
	{
		
	}

	public virtual void DebugGUI()
	{
	}

	public virtual void TabGUI()
	{

	}

	public virtual void CustomGUI()
	{

	}
}