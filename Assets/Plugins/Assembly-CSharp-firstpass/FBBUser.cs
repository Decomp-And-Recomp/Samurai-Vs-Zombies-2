using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class FBBUser
{
	private string _id;

	private string _username;

	private string _name;

	private string _firstName;

	private string _middleName;

	private string _lastName;

	private FBBGender? _gender;

	private string _locale;

	private string _link;

	private TimeZone _timezone;

	private string _currency;

	private bool? _installed;

	private string _email;

	private DateTime? _birthday;

	private string _location;

	public string ID
	{
		get
		{
			return _id;
		}
	}

	public string Username
	{
		get
		{
			return _username;
		}
	}

	public string Name
	{
		get
		{
			return _name;
		}
	}

	public string FirstName
	{
		get
		{
			return _firstName;
		}
	}

	public string MiddleName
	{
		get
		{
			return _middleName;
		}
	}

	public string LastName
	{
		get
		{
			return _lastName;
		}
	}

	public FBBGender? Gender
	{
		get
		{
			return _gender;
		}
	}

	public string Locale
	{
		get
		{
			return _locale;
		}
	}

	public string Link
	{
		get
		{
			return _link;
		}
	}

	public TimeZone Timezone
	{
		get
		{
			return _timezone;
		}
	}

	public string Currency
	{
		get
		{
			return _currency;
		}
	}

	public string Email
	{
		get
		{
			return _email;
		}
	}

	public DateTime? Birthday
	{
		get
		{
			return _birthday;
		}
	}

	public int? Age
	{
		get
		{
			DateTime now = DateTime.Now;
			int num = now.Year - _birthday.Value.Year;
			return (now.Month >= _birthday.Value.Month && (now.Month != _birthday.Value.Month || now.Day >= _birthday.Value.Day)) ? num : (num - 1);
		}
	}

	public string Location
	{
		get
		{
			return _location;
		}
	}

	public bool? Installed
	{
		get
		{
			return _installed;
		}
	}

	public bool IsValid
	{
		get
		{
			return !string.IsNullOrEmpty(_id);
		}
	}

	private FBBUser()
	{
	}

	public FBBUser(string id)
	{
		_id = id;
	}

	public FBBUser(string id, string name)
		: this(id)
	{
		_id = id;
		int num = name.IndexOf(' ');
		if (num > 0 && name.Length > 2)
		{
			_firstName = name.Substring(0, num);
			_lastName = name.Substring(num + 1, name.Length - _firstName.Length - 1);
		}
		else
		{
			_firstName = name;
			_lastName = string.Empty;
		}
	}

	public FBBUser(string id, string firstName, string lastName, bool installed)
		: this(id, firstName + " " + lastName)
	{
		_installed = installed;
	}

	public FBBUser(string id, string firstName, string lastName, FBBGender? gender, string locale, TimeZone timezone, string currency, bool? installed, string email, DateTime birthday)
	{
		_id = id;
		_firstName = firstName;
		_lastName = lastName;
		_gender = gender;
		_locale = locale;
		_timezone = timezone;
		_currency = currency;
		_installed = installed;
		_email = email;
		_birthday = birthday;
	}

	public static FBBUser Deserialize(string data)
	{
		Debug.Log("Deserialize data:");
		Debug.Log(data);
		if (data == null)
		{
			return null;
		}
		FBBUser fBBUser = new FBBUser();
		DeserializeField(out fBBUser._id, "id", data);
		DeserializeField(out fBBUser._username, "username", data);
		DeserializeField(out fBBUser._name, "name", data);
		DeserializeField(out fBBUser._firstName, "first_name", data);
		DeserializeField(out fBBUser._middleName, "middle_name", data);
		DeserializeField(out fBBUser._lastName, "last_name", data);
		string field = string.Empty;
		DeserializeField(out field, "gender", data);
		if (field != null)
		{
			fBBUser._gender = ((!(field == "male")) ? FBBGender.Female : FBBGender.Male);
		}
		else
		{
			fBBUser._gender = null;
		}
		DeserializeField(out fBBUser._locale, "locale", data);
		DeserializeField(out fBBUser._link, "link", data);
		DeserializeField(out fBBUser._currency, "currency", data);
		DeserializeField(out fBBUser._email, "email", data);
		string field2;
		DeserializeField(out field2, "birthday", data);
		DateTime result;
		if (field2 != null && DateTime.TryParse(field2, out result))
		{
			fBBUser._birthday = result;
			Debug.Log("(Calculated age is " + fBBUser.Age + ")");
		}
		else
		{
			fBBUser._birthday = null;
		}
		DeserializeField(out fBBUser._location, "location", data);
		string field3;
		DeserializeField(out field3, "installed", data);
		bool result2;
		if (bool.TryParse(field3, out result2))
		{
			fBBUser._installed = result2;
		}
		else
		{
			fBBUser._installed = null;
		}
		return fBBUser;
	}

	private static void DeserializeField(out string field, string fieldName, string data)
	{
		string pattern = string.Format("\"{0}\"\\:\"([^\"]+)\"", fieldName);
		Regex regex = new Regex(pattern);
		Match match = regex.Match(data);
		if (match.Success)
		{
			field = match.Groups[1].Value;
		}
		else
		{
			field = null;
		}
		Debug.Log(string.Format("Field '{0}' set to '{1}'", fieldName, field ?? "[null]"));
	}
}
