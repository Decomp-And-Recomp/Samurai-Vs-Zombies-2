public interface UIHandlerComponent
{
	void Update(bool updateExpensiveVisuals);

	bool OnUIEvent(string eventID);

	void OnPause(bool pause);
}
