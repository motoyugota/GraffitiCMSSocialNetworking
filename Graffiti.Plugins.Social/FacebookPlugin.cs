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
using System.Collections.Generic;
using System.Collections.Specialized;
using Facebook.Rest;
using Facebook.Schema;
using Facebook.Session;
using Graffiti.Core;

namespace Graffiti.Plugins.Social
{
	public class FacebookPlugin : SocialPostingPlugin
	{
		public string ApiKey { get; set; }
		public string AppSecret { get; set; }
		public string SessionKey { get; set; }
		public string PageIds { get; set; }

		private List<long> pageIdList;
		private List<long> PageIdList
		{
			get
			{
				if (this.pageIdList == null)
				{
					this.pageIdList = new List<long>();
					string[] ids = this.PageIds.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
					foreach (string id in ids)
					{
						this.pageIdList.Add(Convert.ToInt64(id));
					}
				}

				return this.pageIdList;
			}
		}

		public override string Name
		{
			get { return "Facebook Posting Plugin"; }
		}

		public override string Description
		{
			get { return "Automatically Post Update to Facebook when inserting a new Post"; }
		}

		public override void PostInserted(Post post)
		{
			// Send post to FaceBook
			try
			{
				ConnectSession session = new ConnectSession(ApiKey, AppSecret);
				session.SessionKey = SessionKey;

				if (session.IsConnected())
				{
					Macros macros = new Macros();
					Api api = new Api(session);

					string fullUrl = macros.FullUrl(post.Url);

					attachment attachment = new attachment();
					attachment.name = post.Title;
					attachment.href = fullUrl;
					attachment.description = post.Excerpt("", "", "", 200);

					action_link link = new action_link();
					link.href = fullUrl;
					link.text = "Read the full entry";
					List<action_link> links = new List<action_link>();
					links.Add(link);

					if (!String.IsNullOrEmpty(post.ImageUrl))
					{
						attachment_media_image image = new attachment_media_image();
						image.src = macros.FullUrl(post.ImageUrl);
						image.href = fullUrl;
						image.type = attachment_media_type.image;

						List<attachment_media> media = new List<attachment_media>();
						media.Add(image);

						attachment.media = media;
					}

					foreach (long pageId in this.PageIdList)
					{
						try
						{
							api.Stream.Publish("", attachment, links, "", pageId);
						}
						catch (Exception ex)
						{
							Log.Error("Facebook Plugin", "Failed to submit status to Facebook. {0}", ex.Message);
						}
					}
				}
				else
				{
					Log.Error("Facebook Plugin", "Facebook Session ID not valid");
				}
			}
			catch (Exception ex)
			{
				Log.Error("Facebook Plugin", "Failed to submit status to Facebook. {0}", ex.Message);
			}
		}

		protected override FormElementCollection AddFormElements()
		{
			FormElementCollection fec = new FormElementCollection();

			fec.Add(new TextFormElement("apiKey", "API Key", "API Key for your Facebook Application"));
			fec.Add(new TextFormElement("appSecret", "App Secret", "App Secret value for your Facebook Application"));
			fec.Add(new TextFormElement("sessionKey", "Session Key", "Offline Access Session Key for Facebook"));
			fec.Add(new TextAreaFormElement("pageIds", "Page IDs", "Numeric IDs of user and fan pages to post to (one per line).<br />To work, all IDs must grant permission to the Facebook Application to post to streams.", 5));

			return fec;
		}

		protected override System.Collections.Specialized.NameValueCollection DataAsNameValueCollection()
		{
			NameValueCollection nvc = new NameValueCollection();
			nvc["apiKey"] = this.ApiKey;
			nvc["appSecret"] = this.AppSecret;
			nvc["sessionKey"] = this.SessionKey;
			nvc["pageIds"] = this.PageIds;
			return nvc;
		}

		public override StatusType SetValues(System.Web.HttpContext context, NameValueCollection nvc)
		{
			this.ApiKey = nvc["apiKey"];
			this.AppSecret = nvc["appSecret"];
			this.SessionKey = nvc["sessionKey"];
			this.PageIds = nvc["pageIds"];

			return StatusType.Success;
		}
	}
}
