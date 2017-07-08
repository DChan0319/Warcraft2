using Newtonsoft.Json;
using Warcraft.GameModel;

namespace Warcraft.Player.Capabilities
{
	public abstract class ActivatedPlayerCapability
	{
		/// <summary>
		/// The user of the capability.
		/// </summary>
		public PlayerAsset Actor { get; set; }

		/// <summary>
		/// The <see cref="PlayerData"/> of the <see cref="Actor"/>.
		/// </summary>
		public PlayerData PlayerData { get; set; }

		/// <summary>
		/// The target of the capability.
		/// </summary>
		public PlayerAsset Target { get; set; }

		[JsonConstructor]
		public ActivatedPlayerCapability() { }

		public ActivatedPlayerCapability(PlayerAsset actor, PlayerData playerData, PlayerAsset target)
		{
			Actor = actor;
			PlayerData = playerData;
			Target = target;
		}

		public abstract int PercentComplete(int max);
		public abstract bool IncrementStep();
		public abstract void Cancel();
	}
}