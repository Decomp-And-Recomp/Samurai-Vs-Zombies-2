public class FBBSession
{
	private readonly FBBUser _user;

	private readonly FBBPermissions _permissions;

	private readonly FBBSessionState _state;

	public FBBUser User
	{
		get
		{
			return _user;
		}
	}

	public FBBPermissions Permissions
	{
		get
		{
			return _permissions;
		}
	}

	public FBBSessionState State
	{
		get
		{
			return _state;
		}
	}

	public FBBSession(FBBUser user, FBBPermissions permissions, FBBSessionState state)
	{
		_user = user;
		_permissions = permissions;
		_state = state;
	}
}
