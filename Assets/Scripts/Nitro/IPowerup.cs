namespace Nitro
{
    public interface IPowerup
	{
		ICollector Collector { get; }

		void DoAction();

		void DoneUsingPowerup();

		void OnCollect(ICollector collector);
	}
}