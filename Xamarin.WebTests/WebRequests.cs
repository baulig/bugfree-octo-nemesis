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
using SD = System.Diagnostics;
using NUnit.Framework;

namespace Xamarin.WebTests
{
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

		public static void Debug (string message, params object[] args)
		{
			SD.Debug.WriteLine (message + ": " + string.Join (" ", args));
		}

		public static HttpWebRequest CreateWebRequest (string path, RequestFlags flags, string method = "GET")
		{
			var proto = UseSSL (flags) ? "https" : "http";
			var uri = string.Format ("{0}://{1}{2}{3}", proto, Settings.WebHost, Settings.WebPrefix, path);
			Debug ("CreateWebRequest", uri, flags);

			var wr = (HttpWebRequest)HttpWebRequest.Create (uri);
			wr.Method = method;
			if (UseProxy (flags))
				wr.Proxy = GetProxy (flags);
			return wr;
		}

		static IWebProxy GetProxy (RequestFlags flags)
		{
			var proxy = new WebProxy (UseSSL (flags) ? Settings.SquidAddressSSL : Settings.SquidAddress);
			proxy.Credentials = new NetworkCredential (Settings.SquidUser, Settings.SquidPass);
			return proxy;
		}

		public static HttpWebResponse GetResponse (string path, RequestFlags flags, string method = "GET")
		{
			return GetResponse (CreateWebRequest (path, flags, method));
		}

		public static HttpWebResponse GetResponse (HttpWebRequest request)
		{
			return (HttpWebResponse)request.GetResponse ();
		}

		[TestCase(RequestFlags.None, ExpectedResult = 200)]
		[TestCase(RequestFlags.UseSSL, ExpectedResult = 200)]
		[TestCase(RequestFlags.UseProxy, ExpectedResult = 200)]
		[TestCase(RequestFlags.UseSSL | RequestFlags.UseProxy, ExpectedResult = 200)]
		public int TestGet (RequestFlags flags)
		{
			var response = GetResponse ("www/index.html", flags);
			return (int)response.StatusCode;
		}
	}
}

