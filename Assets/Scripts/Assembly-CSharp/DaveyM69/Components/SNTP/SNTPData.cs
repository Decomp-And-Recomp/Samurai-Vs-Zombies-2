using System;
using System.Collections.Generic;

namespace DaveyM69.Components.SNTP
{
	public class SNTPData
	{
		private const int LeapIndicatorLength = 4;

		private const byte LeapIndicatorMask = 192;

		private const byte LeapIndicatorOffset = 6;

		public const int MaximumLength = 68;

		public const int MinimumLength = 48;

		private const byte ModeComplementMask = 248;

		private const int ModeLength = 8;

		private const byte ModeMask = 7;

		private const int originateIndex = 24;

		private const int receiveIndex = 32;

		private const int referenceIdentifierOffset = 12;

		private const int referenceIndex = 16;

		private const int StratumLength = 16;

		public const long TicksPerSecond = 10000000L;

		private const int transmitIndex = 40;

		private const byte VersionNumberComplementMask = 199;

		private const int VersionNumberLength = 8;

		private const byte VersionNumberMask = 56;

		private const byte VersionNumberOffset = 3;

		private byte[] data;

		private static readonly DateTime Epoch = new DateTime(1900, 1, 1);

		private static readonly Dictionary<LeapIndicator, string> LeapIndicatorDictionary = new Dictionary<LeapIndicator, string>
		{
			{
				LeapIndicator.NoWarning,
				"No warning"
			},
			{
				LeapIndicator.LastMinute61Seconds,
				"Last minute has 61 seconds"
			},
			{
				LeapIndicator.LastMinute59Seconds,
				"Last minute has 59 seconds"
			},
			{
				LeapIndicator.Alarm,
				"Alarm condition (clock not synchronized)"
			}
		};

		private static readonly Dictionary<VersionNumber, string> VersionNumberDictionary = new Dictionary<VersionNumber, string>
		{
			{
				VersionNumber.Version3,
				"Version 3 (IPv4 only)"
			},
			{
				VersionNumber.Version4,
				"Version 4 (IPv4, IPv6 and OSI)"
			}
		};

		private static readonly Dictionary<Mode, string> ModeDictionary = new Dictionary<Mode, string>
		{
			{
				Mode.Reserved,
				"Reserved"
			},
			{
				Mode.SymmetricActive,
				"Symmetric active"
			},
			{
				Mode.SymmetricPassive,
				"Symmetric passive"
			},
			{
				Mode.Client,
				"Client"
			},
			{
				Mode.Server,
				"Server"
			},
			{
				Mode.Broadcast,
				"Broadcast"
			},
			{
				Mode.ReservedNTPControl,
				"Reserved for NTP control message"
			},
			{
				Mode.ReservedPrivate,
				"Reserved for private use"
			}
		};

		private static readonly Dictionary<Stratum, string> StratumDictionary = new Dictionary<Stratum, string>
		{
			{
				Stratum.Primary,
				"1, Primary reference (e.g. radio clock)"
			},
			{
				Stratum.Secondary,
				"2, Secondary reference (via NTP or SNTP)"
			},
			{
				Stratum.Secondary3,
				"3, Secondary reference (via NTP or SNTP)"
			},
			{
				Stratum.Secondary4,
				"4, Secondary reference (via NTP or SNTP)"
			},
			{
				Stratum.Secondary5,
				"5, Secondary reference (via NTP or SNTP)"
			},
			{
				Stratum.Secondary6,
				"6, Secondary reference (via NTP or SNTP)"
			},
			{
				Stratum.Secondary7,
				"7, Secondary reference (via NTP or SNTP)"
			},
			{
				Stratum.Secondary8,
				"8, Secondary reference (via NTP or SNTP)"
			},
			{
				Stratum.Secondary9,
				"9, Secondary reference (via NTP or SNTP)"
			},
			{
				Stratum.Secondary10,
				"10, Secondary reference (via NTP or SNTP)"
			},
			{
				Stratum.Secondary11,
				"11, Secondary reference (via NTP or SNTP)"
			},
			{
				Stratum.Secondary12,
				"12, Secondary reference (via NTP or SNTP)"
			},
			{
				Stratum.Secondary13,
				"13, Secondary reference (via NTP or SNTP)"
			},
			{
				Stratum.Secondary14,
				"14, Secondary reference (via NTP or SNTP)"
			},
			{
				Stratum.Secondary15,
				"15, Secondary reference (via NTP or SNTP)"
			},
			{
				Stratum.Unspecified,
				"Unspecified or unavailable"
			}
		};

		private static readonly Dictionary<ReferenceIdentifier, string> RefererenceIdentifierDictionary = new Dictionary<ReferenceIdentifier, string>
		{
			{
				DaveyM69.Components.SNTP.ReferenceIdentifier.ACTS,
				"NIST dialup modem service"
			},
			{
				DaveyM69.Components.SNTP.ReferenceIdentifier.CHU,
				"Ottawa (Canada) Radio 3330, 7335, 14670 kHz"
			},
			{
				DaveyM69.Components.SNTP.ReferenceIdentifier.DCF,
				"Mainflingen (Germany) Radio 77.5 kHz"
			},
			{
				DaveyM69.Components.SNTP.ReferenceIdentifier.GOES,
				"Geostationary Orbit Environment Satellite"
			},
			{
				DaveyM69.Components.SNTP.ReferenceIdentifier.GPS,
				"Global Positioning Service"
			},
			{
				DaveyM69.Components.SNTP.ReferenceIdentifier.LOCL,
				"Uncalibrated local clock used as a primary reference for a subnet without external means of synchronization"
			},
			{
				DaveyM69.Components.SNTP.ReferenceIdentifier.LORC,
				"LORAN-C radionavigation system"
			},
			{
				DaveyM69.Components.SNTP.ReferenceIdentifier.MSF,
				"Rugby (UK) Radio 60 kHz"
			},
			{
				DaveyM69.Components.SNTP.ReferenceIdentifier.OMEG,
				"OMEGA radionavigation system"
			},
			{
				DaveyM69.Components.SNTP.ReferenceIdentifier.PPS,
				"Atomic clock or other pulse-per-second source individually calibrated to national standards"
			},
			{
				DaveyM69.Components.SNTP.ReferenceIdentifier.PTB,
				"PTB (Germany) modem service"
			},
			{
				DaveyM69.Components.SNTP.ReferenceIdentifier.TDF,
				"Allouis (France) Radio 164 kHz"
			},
			{
				DaveyM69.Components.SNTP.ReferenceIdentifier.USNO,
				"U.S. Naval Observatory modem service"
			},
			{
				DaveyM69.Components.SNTP.ReferenceIdentifier.WWV,
				"Ft. Collins (US) Radio 2.5, 5, 10, 15, 20 MHz"
			},
			{
				DaveyM69.Components.SNTP.ReferenceIdentifier.WWVB,
				"Boulder (US) Radio 60 kHz"
			},
			{
				DaveyM69.Components.SNTP.ReferenceIdentifier.WWVH,
				"Kaui Hawaii (US) Radio 2.5, 5, 10, 15 MHz"
			}
		};

		public DateTime DestinationDateTime { get; internal set; }

		public LeapIndicator LeapIndicator
		{
			get
			{
				return (LeapIndicator)LeapIndicatorValue;
			}
		}

		public string LeapIndicatorText
		{
			get
			{
				string value;
				LeapIndicatorDictionary.TryGetValue(LeapIndicator, out value);
				return value;
			}
		}

		private byte LeapIndicatorValue
		{
			get
			{
				return (byte)((data[0] & 0xC0) >> 6);
			}
		}

		public int Length
		{
			get
			{
				return data.Length;
			}
		}

		public double LocalClockOffset
		{
			get
			{
				return (double)(ReceiveDateTime.Ticks - OriginateDateTime.Ticks + (TransmitDateTime.Ticks - DestinationDateTime.Ticks)) / 2.0 / 10000000.0;
			}
		}

		public Mode Mode
		{
			get
			{
				return (Mode)ModeValue;
			}
			private set
			{
				ModeValue = (byte)value;
			}
		}

		public string ModeText
		{
			get
			{
				string value;
				ModeDictionary.TryGetValue(Mode, out value);
				return value;
			}
		}

		private byte ModeValue
		{
			get
			{
				return (byte)(data[0] & 7u);
			}
			set
			{
				data[0] = (byte)((data[0] & 0xF8u) | value);
			}
		}

		public DateTime OriginateDateTime
		{
			get
			{
				return TimestampToDateTime(24);
			}
		}

		public double PollInterval
		{
			get
			{
				return Math.Pow(2.0, (sbyte)data[2]);
			}
		}

		public double Precision
		{
			get
			{
				return Math.Pow(2.0, (sbyte)data[3]);
			}
		}

		public DateTime ReceiveDateTime
		{
			get
			{
				return TimestampToDateTime(32);
			}
		}

		public DateTime ReferenceDateTime
		{
			get
			{
				return TimestampToDateTime(16);
			}
		}

		public string ReferenceIdentifier
		{
			get
			{
				string value = null;
				switch (Stratum)
				{
				case Stratum.Unspecified:
				case Stratum.Primary:
				{
					uint num = 0u;
					for (int i = 0; i <= 3; i++)
					{
						num = (num << 8) | data[12 + i];
					}
					if (!RefererenceIdentifierDictionary.TryGetValue((ReferenceIdentifier)num, out value))
					{
						value = string.Format("{0}{1}{2}{3}", (char)data[12], (char)data[13], (char)data[14], (char)data[15]);
					}
					break;
				}
				case Stratum.Secondary:
				case Stratum.Secondary3:
				case Stratum.Secondary4:
				case Stratum.Secondary5:
				case Stratum.Secondary6:
				case Stratum.Secondary7:
				case Stratum.Secondary8:
				case Stratum.Secondary9:
				case Stratum.Secondary10:
				case Stratum.Secondary11:
				case Stratum.Secondary12:
				case Stratum.Secondary13:
				case Stratum.Secondary14:
				case Stratum.Secondary15:
					switch (VersionNumber)
					{
					case VersionNumber.Version3:
						value = string.Format("{0}.{1}.{2}.{3}", data[12], data[13], data[14], data[15]);
						break;
					default:
						if (VersionNumber < VersionNumber.Version3)
						{
							value = string.Format("{0}.{1}.{2}.{3}", data[12], data[13], data[14], data[15]);
						}
						break;
					case VersionNumber.Version4:
						break;
					}
					break;
				}
				return value;
			}
		}

		public double RootDelay
		{
			get
			{
				return SecondsStampToSeconds(4);
			}
		}

		public double RootDispersion
		{
			get
			{
				return SecondsStampToSeconds(8);
			}
		}

		public double RoundTripDelay
		{
			get
			{
				return (double)(DestinationDateTime.Ticks - OriginateDateTime.Ticks - (ReceiveDateTime.Ticks - TransmitDateTime.Ticks)) / 10000000.0;
			}
		}

		public Stratum Stratum
		{
			get
			{
				return (Stratum)StratumValue;
			}
		}

		public string StratumText
		{
			get
			{
				string value;
				if (!StratumDictionary.TryGetValue(Stratum, out value))
				{
					return "Reserved";
				}
				return value;
			}
		}

		private byte StratumValue
		{
			get
			{
				return data[1];
			}
		}

		public DateTime TransmitDateTime
		{
			get
			{
				return TimestampToDateTime(40);
			}
			private set
			{
				DateTimeToTimestamp(value, 40);
			}
		}

		public VersionNumber VersionNumber
		{
			get
			{
				return (VersionNumber)VersionNumberValue;
			}
			private set
			{
				VersionNumberValue = (byte)value;
			}
		}

		public string VersionNumberText
		{
			get
			{
				string value;
				if (!VersionNumberDictionary.TryGetValue(VersionNumber, out value))
				{
					return "Unknown";
				}
				return value;
			}
		}

		private byte VersionNumberValue
		{
			get
			{
				return (byte)((data[0] & 0x38) >> 3);
			}
			set
			{
				data[0] = (byte)((data[0] & 0xC7u) | (uint)(value << 3));
			}
		}

		internal SNTPData(byte[] bytearray)
		{
			if (bytearray.Length >= 48 && bytearray.Length <= 68)
			{
				data = bytearray;
				return;
			}
			throw new ArgumentOutOfRangeException("Byte Array", string.Format("Byte array must have a length between {0} and {1}.", 48, 68));
		}

		internal SNTPData()
			: this(new byte[48])
		{
		}

		private void DateTimeToTimestamp(DateTime dateTime, int startIndex)
		{
			ulong ticks = (ulong)(dateTime - Epoch).Ticks;
			ulong num = ticks / 10000000;
			ulong num2 = ticks % 10000000 * 4294967296L / 10000000;
			for (int num3 = 3; num3 >= 0; num3--)
			{
				data[startIndex + num3] = (byte)num;
				num >>= 8;
			}
			for (int num4 = 7; num4 >= 4; num4--)
			{
				data[startIndex + num4] = (byte)num2;
				num2 >>= 8;
			}
		}

		private double SecondsStampToSeconds(int startIndex)
		{
			ulong num = 0uL;
			for (int i = 0; i <= 1; i++)
			{
				num = (num << 8) | data[startIndex + i];
			}
			ulong num2 = 0uL;
			for (int j = 2; j <= 3; j++)
			{
				num2 = (num2 << 8) | data[startIndex + j];
			}
			ulong num3 = num * 10000000 + num2 * 10000000 / 65536;
			return (double)num3 / 10000000.0;
		}

		private DateTime Timestamp32ToDateTime(int startIndex)
		{
			ulong num = 0uL;
			for (int i = 0; i <= 3; i++)
			{
				num = (num << 8) | data[startIndex + i];
			}
			ulong value = num * 10000000;
			return Epoch + TimeSpan.FromTicks((long)value);
		}

		private DateTime TimestampToDateTime(int startIndex)
		{
			ulong num = 0uL;
			for (int i = 0; i <= 3; i++)
			{
				num = (num << 8) | data[startIndex + i];
			}
			ulong num2 = 0uL;
			for (int j = 4; j <= 7; j++)
			{
				num2 = (num2 << 8) | data[startIndex + j];
			}
			ulong value = num * 10000000 + num2 * 10000000 / 4294967296L;
			return Epoch + TimeSpan.FromTicks((long)value);
		}

		internal static SNTPData GetClientRequestPacket(VersionNumber versionNumber)
		{
			SNTPData sNTPData = new SNTPData();
			sNTPData.Mode = Mode.Client;
			sNTPData.VersionNumber = versionNumber;
			sNTPData.TransmitDateTime = DateTime.Now.ToUniversalTime();
			return sNTPData;
		}

		public static implicit operator SNTPData(byte[] byteArray)
		{
			return new SNTPData(byteArray);
		}

		public static implicit operator byte[](SNTPData sntpPacket)
		{
			return sntpPacket.data;
		}
	}
}
