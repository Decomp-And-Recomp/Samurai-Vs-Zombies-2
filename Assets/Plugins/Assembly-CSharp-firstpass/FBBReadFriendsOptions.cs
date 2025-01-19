using System;

[Flags]
public enum FBBReadFriendsOptions : uint
{
	None = 0u,
	All = 1u,
	NonPlayers = 2u,
	Players = 4u
}
