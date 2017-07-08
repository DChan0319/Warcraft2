using Warcraft.App;
using Warcraft.Player.Capabilities;

namespace Warcraft.Player
{
	/// <summary>
	/// An asset command, containing information about the action and target.
	/// </summary>
	public struct AssetCommand
	{
		public AssetAction Action;
		public AssetCapabilityType Capability;
		public PlayerAsset Target;
		public ActivatedPlayerCapability ActivatedCapability;
	}
}