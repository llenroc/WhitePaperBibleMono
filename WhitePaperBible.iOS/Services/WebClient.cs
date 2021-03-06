using System;
using RestSharp;
using WhitePaperBible.Core.Services;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Net;
using WhitePaperBible.Core.Models;


namespace WhitePaperBible.iOS
{
	public class WebClient:IJSONWebClient
	{
		public WebClient ()
		{
		}

		private static List<string> PendingMethods = new List<string> ();

		#region IJSONWebClient implementation

		public event EventHandler RequestComplete = delegate {};
		public event EventHandler RequestError;


		public void OpenURL (string url, bool isPost, CookieContainer cookieJar=null)
		{

			var client = new RestClient ();
			client.CookieContainer = cookieJar;

			var request = new RestRequest (url, isPost ? Method.POST : Method.GET) { RequestFormat = DataFormat.Json };

			AddNetworkActivity (url);

			client.ExecuteAsync (request, response => {
				if(response.Cookies.Count > 0){
					if(response.Cookies[0].Name == "_whitepaperbible_session"){
						UserSessionCookie = new SessionCookie{ 
							Name = response.Cookies[0].Name,
							Value = response.Cookies[0].Value
						};
					}
				}

				ResponseText = response.Content;
				RemoveNetworkActivity (url);
				if (response.ResponseStatus == ResponseStatus.Error) {
					DispatchError ();
				} else {
					DispatchComplete ();
				}
			});
		}

		private void DispatchComplete ()
		{
			RequestComplete (this, EventArgs.Empty);
		}

		private void DispatchError ()
		{
			RequestError (this, EventArgs.Empty);
		}

		private static void AddNetworkActivity (string url)
		{
			PendingMethods.Add (url);
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true;
		}

		private static void RemoveNetworkActivity (string url)
		{
			PendingMethods.Remove (url);
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = PendingMethods.Count != 0;
		}

		public string ResponseText {
			get;
			private set;
		}

		public SessionCookie UserSessionCookie {
			get;
			private set;
		}

		#endregion
	}
}

