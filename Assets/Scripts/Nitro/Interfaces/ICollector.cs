namespace Nitro
{
    /// <summary>
    /// Base interface for all collectors
    /// </summary>
    public interface ICollector
    {
        /// <summary>
        /// Checks if a powerup can be collected by this collector
        /// </summary>
        /// <param name="powerup">The powerup to check</param>
        /// <returns>Returns true of the powerup can be collected</returns>
        bool CanCollectPowerup(IPowerup powerup);

        /// <summary>
        /// Collects a powerup
        /// </summary>
        /// <param name="powerup">The powerup to collect</param>
        /// <returns>Returns true if the powerup has been collected</returns>
        bool CollectPowerup(IPowerup powerup);

        /// <summary>
        /// Executes the powerups that have been collected
        /// </summary>
        void Execute();
    }
}
