public interface IInputHandler
{
	void HandleInput(InputCrawl inputCrawl, out InputRouter.InputResponse response);
}
