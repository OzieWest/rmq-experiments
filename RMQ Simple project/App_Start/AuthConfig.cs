using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Web.WebPages.OAuth;
using RMQ_Simple_project.Models;

namespace RMQ_Simple_project
{
	public static class AuthConfig
	{
		public static void RegisterAuth()
		{
			//OAuthWebSecurity.RegisterMicrosoftClient(
			//    clientId: "",
			//    clientSecret: "");

			//OAuthWebSecurity.RegisterTwitterClient(
			//    consumerKey: "",
			//    consumerSecret: "");

			//OAuthWebSecurity.RegisterFacebookClient(
			//    appId: "",
			//    appSecret: "");

			OAuthWebSecurity.RegisterGoogleClient();
		}
	}
}
