using Glu.Plugins.ASocial;

public class Facebook
{
	public static void Initialize(string permissions)
	{
	}

	public static bool IsInitialized()
	{
		return true;
	}

	public static bool IsBeingModified()
	{
		return false;
	}

	public static bool SetPermissions(string permissions)
	{
		return false;
	}

	public static void Login(bool showInterface)
	{
	}

	public static void ValidateSession()
	{
	}

	public static bool IsLoggedIn()
	{
		return Glu.Plugins.ASocial.Facebook.IsLoggedIn();
	}

	public static void Close()
	{
	}

	public static void Logout()
	{
	}

	public static void ReadFriends()
	{
	}

	public static void PublishScore(int score)
	{
	}

	public static void PostMessage(string message, string place, string tags)
	{
	}

	public static void PostLink(string title, string caption, string description, string url, string imageUrl, string place, string tags)
	{
	}

	public static void SendRequest(string message, string toUser, string title)
	{
	}

	public static void FeedDialog(string name, string caption, string description, string toUser, string url, string imageUrl)
	{
	}

	public static void ShowInviteFriendsDialog()
	{
	}
}
