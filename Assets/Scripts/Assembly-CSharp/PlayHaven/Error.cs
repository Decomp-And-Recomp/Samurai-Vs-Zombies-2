namespace PlayHaven
{
	public class Error
	{
		public int code;

		public string description = string.Empty;

		public override string ToString()
		{
			return "code: " + code + ", description: " + description;
		}
	}
}
