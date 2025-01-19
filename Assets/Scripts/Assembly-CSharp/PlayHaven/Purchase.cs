namespace PlayHaven
{
	public class Purchase
	{
		public string productIdentifier;

		public int quantity;

		public string receipt;

		public override string ToString()
		{
			return "productIdentifier: " + productIdentifier + ", quantity: " + quantity + ", receipt: " + receipt;
		}
	}
}
