namespace Nitro
{
    public interface ICollector
    {
        bool CanCollectPowerup(IPowerup powerup);

        bool CollectorEnabled { get; }

        bool CollectOnContact { get; }

        bool CollectPowerup(IPowerup powerup);

        void Execute();
    }
}
