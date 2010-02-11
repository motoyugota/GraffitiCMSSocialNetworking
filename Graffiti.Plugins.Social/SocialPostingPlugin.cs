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
using System.IO;
using System.Net;
using Graffiti.Core;

namespace Graffiti.Plugins.Social
{
	public abstract class SocialPostingPlugin : GraffitiEvent
	{
		public override bool IsEditable
		{
			get { return true; }
		}

		public override void Init(GraffitiApplication ga)
		{
			ga.AfterInsert += new DataObjectEventHandler(ga_AfterInsert);
			ga.BeforeUpdate += new DataObjectEventHandler(ga_BeforeUpdate);
			ga.AfterUpdate += new DataObjectEventHandler(ga_AfterUpdate);
		}

		void ga_AfterUpdate(DataBuddyBase dataObject, EventArgs e)
		{
			Post p = dataObject as Post;

			if (p != null && p.PostStatus == PostStatus.Publish)
			{
				Post oldPost = GraffitiContext.Current["postBeingUpdated"] as Post;
				if (oldPost != null && oldPost.PostStatus != PostStatus.Publish)
				{
					PostInserted(p);
				}
			}
		}

		void ga_BeforeUpdate(DataBuddyBase dataObject, EventArgs e)
		{
			Post p = dataObject as Post;
			if (p != null)
			{
				Post oldPost = new Post(p.Id);
				if (p.PostStatus != oldPost.PostStatus)
				{
					GraffitiContext.Current["postBeingUpdated"] = oldPost;
				}
			}
		}

		private void ga_AfterInsert(DataBuddyBase dataObject, EventArgs e)
		{
			//Only handle posts
			Post p = dataObject as Post;

			//We only only want post which are non-uncategorized and are not published in the future.
			if (p != null && p.PostStatus == PostStatus.Publish && (!SiteSettings.Get().FilterUncategorizedPostsFromLists || p.CategoryId != CategoryController.UnCategorizedId))
			{
				PostInserted(p);
			}
		}

		public abstract void PostInserted(Post post);

		protected bool ConvertStringToBool(string checkValue)
		{
			if (string.IsNullOrEmpty(checkValue))
				return false;
			else if (checkValue == "checked" || checkValue == "on")
				return true;
			else
				return bool.Parse(checkValue);
		}

		protected string MakeTinyUrl(string Url)
		{
			try
			{
				if (Url.Length <= 30)
				{
					return Url;
				}
				if (!Url.ToLower().StartsWith("http") && !Url.ToLower().StartsWith("ftp"))
				{
					Url = "http://" + Url;
				}
				var request = WebRequest.Create("http://tinyurl.com/api-create.php?url=" + Url);
				var res = request.GetResponse();
				string text;
				using (var reader = new StreamReader(res.GetResponseStream()))
				{
					text = reader.ReadToEnd();
				}
				return text;
			}
			catch (Exception)
			{
				return Url;
			}
		}
	}
}
