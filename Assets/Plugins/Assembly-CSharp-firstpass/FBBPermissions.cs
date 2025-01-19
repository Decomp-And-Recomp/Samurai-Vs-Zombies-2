public class FBBPermissions
{
	private readonly string _permissions;

	public FBBPermissions(string permissions)
	{
		_permissions = permissions;
	}

	public override string ToString()
	{
		return _permissions;
	}
}
