namespace Gamespy.Matchmaking
{
	public enum NATType
	{
		no_nat = 0,
		firewall_only = 1,
		full_cone = 2,
		restricted_cone = 3,
		port_restricted_cone = 4,
		symmetric = 5,
		unknown = 6,
		NUM_NAT_TYPES = 7
	}
}
