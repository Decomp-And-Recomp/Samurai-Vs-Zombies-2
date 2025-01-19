namespace PlayHaven
{
	public class Reward
	{
		public string receipt;

		public string name;

		public int quantity;

		public override string ToString()
		{
			return "name: " + name + ", quantity: " + quantity + ", receipt: " + receipt;
		}
	}
}
