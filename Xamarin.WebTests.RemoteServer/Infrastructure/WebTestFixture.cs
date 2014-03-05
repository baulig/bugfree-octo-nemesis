//
// WebTestFixture.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2014 Xamarin Inc. (http://www.xamarin.com)
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
using System.Net;
using SD = System.Diagnostics;

namespace Xamarin.WebTests.RemoteServer.Infrastructure
{
	using Framework;
	using Client;

	public abstract class WebTestFixture
	{
		public static bool UseSSL (PuppyFlags flags)
		{
			return (flags & PuppyFlags.UseSSL) != 0;
		}

		public static bool UseProxy (PuppyFlags flags)
		{
			return (flags & PuppyFlags.UseProxy) != 0;
		}

		public static bool AutoRedirect (PuppyFlags flags)
		{
			return (flags & PuppyFlags.AutoRedirect) != 0;
		}

		public void Debug (string function, params object[] args)
		{
			var message = GetType ().Name + ":" + function + ": " + string.Join (" ", args);
			SD.Debug.WriteLine (message);
			Console.WriteLine (message);
		}

		protected static readonly PuppyFlags[] AllFlags = { PuppyFlags.None, PuppyFlags.UseSSL, PuppyFlags.UseProxy, PuppyFlags.UseSSL | PuppyFlags.UseProxy };
		protected static readonly PuppyFlags[] NoProxyFlags = { PuppyFlags.None, PuppyFlags.UseSSL };
		protected static readonly HttpStatusCode[] AllRedirectCodes = { HttpStatusCode.Moved, HttpStatusCode.Found, HttpStatusCode.SeeOther, HttpStatusCode.TemporaryRedirect };
		protected static readonly TransferMode[] AllTransferModes = { TransferMode.Default, TransferMode.ContentLength, TransferMode.Chunked };

		public static HttpWebRequest CreateWebRequest (string path, PuppyFlags flags, string method = "GET")
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

		static IWebProxy GetProxy (PuppyFlags flags)
		{
			var proxy = new WebProxy (UseSSL (flags) ? Settings.SquidAddressSSL : Settings.SquidAddress);
			if (!string.IsNullOrEmpty (Settings.SquidUser))
				proxy.Credentials = new NetworkCredential (Settings.SquidUser, Settings.SquidPass);
			return proxy;
		}

		public static HttpWebResponse GetResponse (string path, PuppyFlags flags, string method = "GET")
		{
			var request = CreateWebRequest (path, flags, method);
			request.Timeout = 2500;
			return GetResponse (request);
		}

		public static HttpWebResponse GetResponse (HttpWebRequest request)
		{
			return (HttpWebResponse)request.GetResponse ();
		}
	}
}

