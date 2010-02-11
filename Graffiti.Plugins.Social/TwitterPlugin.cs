/* ****************************************************************************
 *
 * Copyright (c) Nexus Technologies, LLC. All rights reserved.
 *
 * This software is subject to the Microsoft Public License (Ms-PL). 
 * A copy of the license can be found at:
 * 
 * http://graffitisocial.codeplex.com/license
 *
 * You must not remove this notice, or any other, from this software.
 *
 * ***************************************************************************/

using System;
using System.Collections.Specialized;
using Graffiti.Core;
using Twitterizer.Framework;

namespace Graffiti.Plugins.Social
{
	public class TwitterPlugin : SocialPostingPlugin
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string ApplicationName { get; set; }

		private const int maxLength = 140;

		public override string Name
		{
			get { return "Twitter Posting Plugin"; }
		}

		public override string Description
		{
			get { return "Automatically Post Update to Twitter when inserting a new Post"; }
		}

		public override void PostInserted(Post post)
		{
			// Send post to Twitter
			try
			{
				Twitter t = new Twitter();
				if (String.IsNullOrEmpty(this.ApplicationName))
				{
					t = new Twitter(this.Username, this.Password);
				}
				else
				{
					t = new Twitter(this.Username, this.Password, this.ApplicationName);
				}
				
				Macros macros = new Macros();

				string status = "Latest Post: " + post.Title;
				string url = MakeTinyUrl(macros.FullUrl(post.Url));

				int length = status.Length;
				int urlLength = url.Length;

				if (length + urlLength <= maxLength - 1)
				{
					status = status + " " + url;
				}
				else if (urlLength > maxLength)
				{
					throw new Exception("URL could not be shortened - Twitter Status Update could not be posted");
				}
				else
				{
					status = status.Substring(0, maxLength - urlLength - 2) + "… " + url;
				}

				t.Status.Update(status);
			}
			catch (Exception ex)
			{
				Log.Error("Twitter Plugin", "Failed to submit status to Twitter. {0}", ex.Message);
			}
		}

		protected override FormElementCollection AddFormElements()
		{
			FormElementCollection fec = new FormElementCollection();

			fec.Add(new TextFormElement("username", "Username", "Name of the Twitter user account"));
			fec.Add(new TextFormElement("password", "Password", "Twitter password"));
			fec.Add(new TextFormElement("applicationName", "Application Name", "Name to show on all Twitter messages as the source application.  Must first be registered at <a href=\"http://twitter.com/oauth_clients/new\">http://twitter.com/oauth_clients/new</a>.  If not entered, Twitterizer will be used as the source application."));

			return fec;
		}

		protected override System.Collections.Specialized.NameValueCollection DataAsNameValueCollection()
		{
			NameValueCollection nvc = new NameValueCollection();
			nvc["username"] = this.Username;
			nvc["password"] = this.Password;
			nvc["applicationName"] = this.ApplicationName;
			return nvc;
		}

		public override StatusType SetValues(System.Web.HttpContext context, NameValueCollection nvc)
		{
			this.Username = nvc["username"];
			this.Password = nvc["password"];
			this.ApplicationName = nvc["applicationName"];
			
			return StatusType.Success;
		}
	}
}
