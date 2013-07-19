namespace DotNetOpenAuth.ApplicationBlock.OAuth2.LinkedIn
{
	using System;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.OAuth2;

	public class LinkedInClient : WebServerClient
	{
		public static string ProviderName = "LinkedIn";

		private static readonly AuthorizationServerDescription LinkedInDescription = new AuthorizationServerDescription
		{
			TokenEndpoint = new Uri("https://www.linkedin.com/uas/oauth2/accessToken"),
			AuthorizationEndpoint = new Uri("https://www.linkedin.com/uas/oauth2/authorization"),
			ProtocolVersion = ProtocolVersion.V20
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="LinkedInClient"/> class.
		/// </summary>
		public LinkedInClient() : base(LinkedInDescription) { }

		public async Task<IOAuth2Graph> GetGraphAsync(IAuthorizationState authState, string[] fields = null, CancellationToken cancellationToken = default(CancellationToken)) {
			if ((authState != null) && (authState.AccessToken != null)) {
				var httpClient = new HttpClient(this.CreateAuthorizingHandler(authState));
				var fieldsStr = string.Empty;
				if ((fields == null) || (fields.Length == 0))
				{
					fieldsStr = LinkedInJsonGraph.Fields.BasicProfile;
				}
				else
				{
					fieldsStr = string.Join(",", fields);
				}
				using (var response = await httpClient.GetAsync(string.Format("https://api.linkedin.com/v1/people/~:({0})", fieldsStr), cancellationToken))
				{
					response.EnsureSuccessStatusCode();
					using (var responseStream = await response.Content.ReadAsStreamAsync()) {
						var linkedInGraph = LinkedInJsonGraph.Deserialize(responseStream);
						return linkedInGraph;
					}
				}
			}

			return null;
		}

		internal new DelegatingHandler CreateAuthorizingHandler(IAuthorizationState authorization, HttpMessageHandler innerHandler = null)
		{
			if (authorization == null)
			{
				throw new Exception("Authorization");
			}
			return new LinkedInTokenHttpMessageHandler(this, authorization, innerHandler ?? new HttpClientHandler());
		}

		/// <summary>
		/// Scopes defined by the LinkedIn.
		/// </summary>
		/// <remarks>
		/// See https://developer.linkedin.com/documents/authentication
		/// </remarks>
		public static class Scopes
		{
			/// <summary>
			/// Your Profile Overview, Name, photo, headline, and current positions
			/// GET /people/~
			/// </summary>
			public const string BasicProfile = "r_basicprofile";

			/// <summary>
			/// Your Full Profile, Full profile including experience, education, skills, and recommendations
			/// </summary>
			public const string Fullprofile = "r_fullprofile";

			/// <summary>
			/// Your Email Address, The primary email address you use for your LinkedIn account
			/// </summary>
			public const string Email = "r_emailaddress";

			/// <summary>
			/// Your 1st and 2nd degree connections
			/// </summary>
			public const string Network = "r_network";

			/// <summary>
			/// Address, phone number, and bound accounts
			/// </summary>
			public const string ContactInfo = "r_contactinfo";

			/// <summary>
			/// Retrieve and post updates to LinkedIn as you
			/// </summary>
			public const string NetworkUpdates = "rw_nus";

			/// <summary>
			/// Retrieve and post group discussions as you
			/// </summary>
			public const string GroupDiscussions = "rw_groups";

			/// <summary>
			/// Send messages and invitations to connect as you
			/// </summary>
			public const string InvitationsMessages = "w_messages";
		}
	}
}
