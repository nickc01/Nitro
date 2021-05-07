public interface IPlayerDetector
{
	/// <summary>
	/// Called when the object collides with a car
	/// </summary>
	/// <param name="player"></param>
	void OnPlayerTouch(Player player);
	
	/// <summary>
	/// Called either when the car stops colliding with the object, or a new object collides with the car
	/// </summary>
	/// <param name="player"></param>
	void OnPlayerUnTouch(Player player);
}