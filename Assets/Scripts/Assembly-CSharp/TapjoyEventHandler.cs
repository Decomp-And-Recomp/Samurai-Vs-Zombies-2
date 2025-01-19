using UnityEngine;

public class TapjoyEventHandler : MonoBehaviour
{
	public delegate void VideoStateHandler(bool videoEnabled);

	public const string HANDLER_OBJECT_NAME = "TapjoyEventHandlerObject";

	private static TapjoyEventHandler myself;

	public VideoStateHandler videoStateHandler;

	public static void Init()
	{
		if (myself == null)
		{
			GameObject gameObject = new GameObject("TapjoyEventHandlerObject");
			myself = gameObject.AddComponent<TapjoyEventHandler>();
			Object.DontDestroyOnLoad(gameObject);
		}
	}

	public static TapjoyEventHandler GetHandlerInstance()
	{
		return myself;
	}

	public void OnVideoStateChanged(string state)
	{
		if (videoStateHandler != null)
		{
			if (state.Equals("enabled"))
			{
				videoStateHandler(true);
			}
			else
			{
				videoStateHandler(false);
			}
		}
	}
}
