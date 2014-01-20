//
// WebRequests.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://www.xamarin.com)
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using SD = System.Diagnostics;
#if NUNIT
using NUnit.Framework;
#endif

namespace Xamarin.WebTests
{
	using Client;

	[TestFixture]
	public class WebRequests
	{
		public static bool UseSSL (RequestFlags flags)
		{
			return (flags & RequestFlags.UseSSL) != 0;
		}

		public static bool UseProxy (RequestFlags flags)
		{
			return (flags & RequestFlags.UseProxy) != 0;
		}

		public static bool AutoRedirect (RequestFlags flags)
		{
			return (flags & RequestFlags.AutoRedirect) != 0;
		}

		public static void Debug (string function, params object[] args)
		{
			var message = function + ": " + string.Join (" ", args);
			SD.Debug.WriteLine (message);
			Console.WriteLine (message);
		}

		public static HttpWebRequest CreateWebRequest (string path, RequestFlags flags, string method = "GET")
		{
			var proto = UseSSL (flags) ? "https" : "http";
			var uri = string.Format ("{0}://{1}{2}{3}", proto, Settings.WebHost, Settings.WebPrefix, path);
			// Debug ("CreateWebRequest", uri, flags);

			var wr = (HttpWebRequest)HttpWebRequest.Create (uri);
			wr.Method = method;
			wr.AllowAutoRedirect = AutoRedirect (flags);
			if (UseProxy (flags))
				wr.Proxy = GetProxy (flags);
			return wr;
		}

		static IWebProxy GetProxy (RequestFlags flags)
		{
			var proxy = new WebProxy (UseSSL (flags) ? Settings.SquidAddressSSL : Settings.SquidAddress);
			if (!string.IsNullOrEmpty (Settings.SquidUser))
				proxy.Credentials = new NetworkCredential (Settings.SquidUser, Settings.SquidPass);
			return proxy;
		}

		public static HttpWebResponse GetResponse (string path, RequestFlags flags, string method = "GET")
		{
			var request = CreateWebRequest (path, flags, method);
			request.Timeout = 2500;
			return GetResponse (request);
		}

		public static HttpWebResponse GetResponse (HttpWebRequest request)
		{
			return (HttpWebResponse)request.GetResponse ();
		}

		[Category("Simple")]
		[TestCaseSource("AllFlags")]
		public void TestGet (RequestFlags flags)
		{
			var response = GetResponse ("www/index.html", flags);
			try {
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode);
			} finally {
				response.Close ();
			}
		}

		[Category("Redirect")]
		[TestCaseSource ("RedirectTests")]
		public void TestRedirect (RedirectTest test)
		{
			var path = string.Format ("redirects/same-server/{0}/index.html", (int)test.Code);
			var response = GetResponse (path, test.Flags);
			try {
				Assert.AreEqual (test.Code, response.StatusCode, "#1");
			} finally {
				response.Close ();
			}
		}

		[Category("FollowRedirect")]
		[TestCaseSource ("RedirectTests")]
		public void FollowRedirect (RedirectTest test)
		{
			var path = string.Format ("redirects/same-server/{0}/index.html", (int)test.Code);
			var response = GetResponse (path, test.Flags | RequestFlags.AutoRedirect);
			try {
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#1");
			} finally {
				response.Close ();
			}
		}

		[Category("Test")]
		[TestCaseSource ("Broken")]
		public void NotWorking (RedirectTest test)
		{
			var path = string.Format ("redirects/same-server/{0}/index.html", (int)test.Code);
			var response = GetResponse (path, test.Flags);
			try {
				Assert.AreEqual (test.Code, response.StatusCode, "#1");
			} finally {
				response.Close ();
			}
		}

		[Category("Reuse")]
		[TestCaseSource ("ReuseTests")]
		public void TestReuse (ReuseTest test)
		{
			int port = -1;
			int connections = 0;
			Debug ("TestReuse", test);
			for (int i = 0; i < test.Count; i++) {
				var puppy = GetPuppy.Get (test.Flags, test.TransferMode);
				if (puppy.RemotePort == port)
					continue;
				if (i > 0)
					Debug ("TestReuse - NEW CONNECTION", i, port, puppy);
				connections++;
				port = puppy.RemotePort;
				if (connections > test.Limit)
					break;
			}

#if NUNIT
			Assert.That (connections, Is.EqualTo (1), "#1");
#else
			if (connections != 1)
				throw new InvalidOperationException ();
#endif
		}

		[Category("Connections")]
		[TestCaseSource ("ReuseTests")]
		public void TestConnectionCount (ReuseTest test)
		{
			var puppy = GetPuppy.Get (test.Flags, test.TransferMode);
			var sp = ServicePointManager.FindServicePoint (puppy.Uri);
			Assert.That (sp, Is.Not.Null, "#1");
			Assert.That (sp.CurrentConnections, Is.EqualTo (1), "#2");

			sp.CloseConnectionGroup (string.Empty);
			Assert.That (sp.CurrentConnections, Is.EqualTo (0), "#3");
		}

		[Category("Connections")]
		[TestCaseSource ("ReuseTests")]
		public void TestConnectionIdleTime (ReuseTest test)
		{
			var puppy = GetPuppy.Get (test.Flags, test.TransferMode);
			var sp = ServicePointManager.FindServicePoint (puppy.Uri);
			sp.MaxIdleTime = 2500;

			Assert.That (sp, Is.Not.Null, "#1");
			Assert.That (sp.CurrentConnections, Is.EqualTo (1), "#2");

			Thread.Sleep (2000);

			Assert.That (sp.CurrentConnections, Is.EqualTo (1), "#3");

			Thread.Sleep (600);

			Assert.That (sp.CurrentConnections, Is.EqualTo (0), "#4");
		}

		public static IEnumerable ReuseTests ()
		{
			foreach (var flags in NoProxyFlags)
				foreach (var mode in AllTransferModes)
					yield return new ReuseTest (flags, mode, 250);
		}

		static readonly RequestFlags[] AllFlags = { RequestFlags.None, RequestFlags.UseSSL, RequestFlags.UseProxy, RequestFlags.UseSSL | RequestFlags.UseProxy };
		static readonly RequestFlags[] NoProxyFlags = { RequestFlags.None, RequestFlags.UseSSL };
		static readonly HttpStatusCode[] AllRedirectCodes = { HttpStatusCode.Moved, HttpStatusCode.Found, HttpStatusCode.SeeOther, HttpStatusCode.TemporaryRedirect };
		static readonly TransferMode[] AllTransferModes = { TransferMode.Default, TransferMode.ContentLength, TransferMode.Chunked };

		public static IEnumerable RedirectTests ()
		{
			foreach (var flags in AllFlags)
				foreach (var code in AllRedirectCodes)
					yield return new RedirectTest { Flags = flags, Code = code };
		}

		public static IEnumerable Broken ()
		{
			yield return new RedirectTest { Flags = RequestFlags.UseProxy, Code = HttpStatusCode.Redirect };
		}
	}
}

