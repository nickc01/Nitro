namespace Nitro
{
	/// <summary>
	/// The base class for all powerups
	/// </summary>
    public interface IPowerup
	{
		/// <summary>
		/// If this powerup has been collected, this is the collector that collected it
		/// </summary>
		ICollector Collector { get; }

		/// <summary>
		/// The main action of the powerup
		/// </summary>
		void DoAction();

		/// <summary>
		/// Used to mark when a powerup is done executing
		/// </summary>
		void DoneUsingPowerup();

		/// <summary>
		/// Called when the powerup has been collected by a collector
		/// </summary>
		/// <param name="collector">The collector that collected the powerup</param>
		void OnCollect(ICollector collector);
	}
}