namespace OAuthClient
{
	using System;
	using System.Collections.Generic;
	using System.Configuration;
	using System.Net;
	using System.Web;
	using System.Web.UI;

	using DotNetOpenAuth.ApplicationBlock;
	using DotNetOpenAuth.ApplicationBlock.OAuth2.LinkedIn;
	using DotNetOpenAuth.Messaging;
	using DotNetOpenAuth.OAuth2;

	public partial class LinkedIn : System.Web.UI.Page
	{
		public static readonly LinkedInClient LinkedInClient = new LinkedInClient
		{
			ClientIdentifier = ConfigurationManager.AppSettings["linkedInApiKey"],
			ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(ConfigurationManager.AppSettings["linkedInSecretKey"]),
		};

		protected void Page_Load(object sender, EventArgs e)
		{
			var scopes = new HashSet<string>()
				{
					LinkedInClient.Scopes.Email,
					LinkedInClient.Scopes.BasicProfile,
					LinkedInClient.Scopes.Fullprofile
					////LinkedInClient.Scopes.ContactInfo,
					////LinkedInClient.Scopes.GroupDiscussions,
					////LinkedInClient.Scopes.InvitationsMessages,
					////LinkedInClient.Scopes.Network,
					////LinkedInClient.Scopes.NetworkUpdates
				};

			this.RegisterAsyncTask(
				new PageAsyncTask(
					async ct =>
					{
						IAuthorizationState authorization = await LinkedInClient.ProcessUserAuthorizationAsync(new HttpRequestWrapper(Request), ct);
						if (authorization == null)
						{
							// Kick off authorization request
							var request = await LinkedInClient.PrepareRequestUserAuthorizationAsync(cancellationToken: ct, scopes: scopes);
							await request.SendAsync(new HttpContextWrapper(Context), ct);
							this.Context.Response.End();
						}
						else
						{
							IOAuth2Graph oauth2Graph = await LinkedInClient.GetGraphAsync(authorization, new[] { LinkedInJsonGraph.Fields.BasicProfile, LinkedInJsonGraph.Fields.Email }, ct);
							//// IOAuth2Graph oauth2Graph2 = await LinkedInClient.GetGraphAsync(authorization, new[] { LinkedInJsonGraph.Fields.BasicProfile, LinkedInJsonGraph.Fields.FullProfile, LinkedInJsonGraph.Fields.Email, LinkedInJsonGraph.Fields.ContactInfo });

							this.nameLabel.Text = HttpUtility.HtmlEncode(oauth2Graph.FirstName) + " " + oauth2Graph.Email + " " + ((LinkedInJsonGraph)oauth2Graph).Headline;
						}
					}));
		}
	}
}