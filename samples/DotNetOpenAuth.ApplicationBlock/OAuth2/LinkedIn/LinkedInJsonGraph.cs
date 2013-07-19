namespace DotNetOpenAuth.ApplicationBlock.OAuth2.LinkedIn
{
	using System;
	using System.Globalization;
	using System.IO;
	using System.Runtime.Serialization;
	using System.Runtime.Serialization.Json;
	using System.Text;

	// https://developer.linkedin.com/apis
	// Note that for the xml version the datacontrat is different.
	[DataContract]
	public class LinkedInJsonGraph : IOAuth2Graph
	{
		private static readonly DataContractJsonSerializer JsonSerializer = new DataContractJsonSerializer(typeof(LinkedInJsonGraph));

		/// <summary>
		/// Gets or sets unique identifier token for this member
		/// </summary>
		[DataMember(Name = "id")]
		public string Id { get; set; }

		/// <summary>
		/// Gets or sets the formatted name of the user.
		/// LinkedIn has no specific name: the member's name formatted based on language
		/// This field might be omitted from some results or return a value of private, depending on the member's privacy settings
		/// </summary>
		[DataMember(Name = "formattedName")]
		public string Name { get; set; }

		/// <summary>
		/// Gets or sets the user's first name
		/// </summary>
		[DataMember(Name = "firstName")]
		public string FirstName { get; set; }

		/// <summary>
		/// Gets or sets the user's last name
		/// This field might be omitted from some results or return a value of private, depending on the member's privacy settings
		/// </summary>
		[DataMember(Name = "lastName")]
		public string LastName { get; set; }

		/// <summary>
		/// Gets or sets the member's headline (often "Job Title at Company")
		/// </summary>
		[DataMember(Name = "headline")]
		public string Headline { get; set; }

		/// <summary>
		/// Gets or sets the user's gender: always unknown with linkedin
		/// </summary>
		[DataMember(Name = "gender")]
		public string Gender { get; set; }

		/// <summary>
		/// Gets or sets the user's locale
		/// </summary>
		[DataMember(Name = "location")]
		public LinkedInLocation Location { get; set; }

		public string Locale {
			get { return new CultureInfo(this.Location.Country.Code).ToString(); }
		}

		[DataMember(Name = "languages")]
		public LinkedInIdName[] Languages { get; set; }

		/// <summary>
		/// Gets or sets the URL of the profile for the user on Facebook
		/// This field is only available when requested using a field selector in a profile or connections call.
		/// </summary>
		[DataMember(Name = "publicProfileUrl")]
		public Uri Link { get; set; }

		/// <summary>
		/// Gets or sets the timestamp, in milliseconds, when the member's profile was last edited
		/// </summary>
		[DataMember(Name = "lastModifiedTimestamp")]
		public string UpdatedTime { get; set; }

		/// <summary>
		/// Gets or sets the user's birthday
		/// Date string in linked in format
		/// </summary>
		[DataMember(Name = "dateOfBirth")]
		public string Birthday { get; set; }

		public DateTime? BirthdayDT
		{
			get
			{
				if (!string.IsNullOrEmpty(this.Birthday) && (this.Locale != null))
				{
					var ci = new CultureInfo(this.Locale.Replace('_', '-'));
					return DateTime.Parse(this.Birthday, ci);
				}

				return null;
			}
		}

		/// <summary>
		/// Gets or sets the primary email address of user
		/// </summary>
		[DataMember(Name = "emailAddress")]
		public string Email { get; set; }

		[DataMember(Name = "pictureUrl")]
		public Uri AvatarUrl { get; set; }

		/// <summary>
		/// Gets the gender, but LinkedIn does not support gender, hence always Unknown
		/// </summary>
		public HumanGender GenderEnum
		{
			get
			{
				return HumanGender.Unknown;
			}
		}

		public static LinkedInJsonGraph Deserialize(string json)
		{
			if (string.IsNullOrEmpty(json))
			{
				throw new ArgumentNullException("json");
			}

			return Deserialize(new MemoryStream(Encoding.UTF8.GetBytes(json)));
		}

		public static LinkedInJsonGraph Deserialize(Stream jsonStream)
		{
			if (jsonStream == null)
			{
				throw new ArgumentNullException("jsonStream");
			}
			return (LinkedInJsonGraph)JsonSerializer.ReadObject(jsonStream);
		}

		public static class Fields
		{
			public const string BasicProfile = "id,first_name,last_name,location:(country:(code)),picture-url,public-profile-url,formatted-name,headline";
			public const string FullProfile = "last-modified-timestamp,date-of-birth";
			public const string Email = "email-address";
			public const string ContactInfo = "phone-numbers";
		}

		[DataContract]
		public class LinkedInLocation
		{
			[DataMember(Name = "country")]
			public LinkedInCountry Country { get; set; }
		}

		[DataContract]
		public class LinkedInCountry
		{
			[DataMember(Name = "code")]
			public string Code { get; set; }
		}

		[DataContract]
		public class LinkedInIdName
		{
			[DataMember(Name = "id")]
			public string Id { get; set; }

			[DataMember(Name = "name")]
			public string Name { get; set; }
		}
		[DataContract(Name = "phoneNumber")]
		public class PhoneNumber
		{
			[DataMember(Name = "phoneType")]
			public string Id { get; set; }

			[DataMember(Name = "phoneNumber")]
			public string Name { get; set; }
		}
	}
}
