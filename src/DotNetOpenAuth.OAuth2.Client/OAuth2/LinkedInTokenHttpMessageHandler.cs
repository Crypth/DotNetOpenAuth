//-----------------------------------------------------------------------
// <copyright file="LinkedInTokenHttpMessageHandler.cs" company="No">
//     Added by Samuel Zayas. Do whatever you want with it.
// </copyright>
//-----------------------------------------------------------------------
namespace DotNetOpenAuth.OAuth2
{
	using System;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using DotNetOpenAuth.Messaging;
	using Validation;

	/// <summary>
	/// Adds the access token and x-li-format headers to the outgoing linked in api call.
	/// Has to be public, since we're overloading 
	/// </summary>
	public class LinkedInTokenHttpMessageHandler : DelegatingHandler
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="LinkedInTokenHttpMessageHandler" /> class.
		/// </summary>
		/// <param name="bearerToken">The bearer token.</param>
		/// <param name="innerHandler">The inner handler.</param>
		public LinkedInTokenHttpMessageHandler(string bearerToken, HttpMessageHandler innerHandler)
			: base(innerHandler)
		{
			Requires.NotNullOrEmpty(bearerToken, "bearerToken");
			this.BearerToken = bearerToken;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="LinkedInTokenHttpMessageHandler" /> class.
		/// </summary>
		/// <param name="client">The client associated with the authorization.</param>
		/// <param name="authorization">The authorization.</param>
		/// <param name="innerHandler">The inner handler.</param>
		/// <param name="format">Header to be added to x-li-format, xml or json</param>
		public LinkedInTokenHttpMessageHandler(ClientBase client, IAuthorizationState authorization, HttpMessageHandler innerHandler, string format = "json")
			: base(innerHandler)
		{
			Requires.NotNull(client, "client");
			Requires.NotNull(authorization, "authorization");
			Requires.That(!string.IsNullOrEmpty(authorization.AccessToken), "authorization.AccessToken", "AccessToken must be non-empty");
			this.ReturnFormat = format;
			this.Client = client;
			this.Authorization = authorization;
		}

		/// <summary>
		/// Gets the type of return format
		/// </summary>
		/// <value>The x-li-format entered in constructor.</value>
		internal string ReturnFormat { get; private set; }

		/// <summary>
		/// Gets the bearer token.
		/// </summary>
		/// <value>
		/// The bearer token.
		/// </value>
		internal string BearerToken { get; private set; }

		/// <summary>
		/// Gets the authorization.
		/// </summary>
		internal IAuthorizationState Authorization { get; private set; }

		/// <summary>
		/// Gets the OAuth 2 client associated with the <see cref="Authorization"/>.
		/// </summary>
		internal ClientBase Client { get; private set; }

		/// <summary>
		/// Sends an HTTP request to the inner handler to send to the server as an asynchronous operation.
		/// </summary>
		/// <param name="request">The HTTP request message to send to the server.</param>
		/// <param name="cancellationToken">A cancellation token to cancel operation.</param>
		/// <returns>
		/// Returns <see cref="T:System.Threading.Tasks.Task`1" />. The task object representing the asynchronous operation.
		/// </returns>
		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			var bearerToken = this.BearerToken;
			if (bearerToken == null)
			{
				ErrorUtilities.VerifyProtocol(!this.Authorization.AccessTokenExpirationUtc.HasValue || this.Authorization.AccessTokenExpirationUtc >= DateTime.UtcNow || this.Authorization.RefreshToken != null, "AuthorizationExpired");
				if (this.Authorization.AccessTokenExpirationUtc.HasValue && this.Authorization.AccessTokenExpirationUtc.Value < DateTime.UtcNow)
				{
					ErrorUtilities.VerifyProtocol(this.Authorization.RefreshToken != null, "AccessTokenRefreshFailed");
					await this.Client.RefreshAuthorizationAsync(this.Authorization, cancellationToken: cancellationToken);
				}
				bearerToken = this.Authorization.AccessToken;
			}

			var newUri = request.RequestUri.OriginalString;
			newUri += (request.RequestUri.OriginalString.Contains("?") ? "&" : "?") + "oauth2_access_token=" + bearerToken;
			request.RequestUri = new Uri(newUri);
			Requires.NotNullOrEmpty(this.ReturnFormat, "ReturnFormat");
			request.Headers.Add("x-li-format", this.ReturnFormat);

			return await base.SendAsync(request, cancellationToken);
		}
	}
}