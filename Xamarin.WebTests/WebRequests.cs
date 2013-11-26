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

namespace Xamarin.WebTests
{
	public static class WebRequests
	{
		public static HttpWebRequest CreateWebRequest (Request request)
		{
			var proto = request.UseSSL ? "https" : "http";
			var path = Path.Combine (Settings.WebPrefix, request.Path);
			var uri = string.Format ("{0}://{1}{2}", proto, Settings.WebHost, path);
			Console.WriteLine (uri);

			var wr = (HttpWebRequest)HttpWebRequest.Create (uri);
			wr.Method = request.Method;
			if (request.UseProxy)
				wr.Proxy = GetProxy (request.UseSSL);
			return wr;
		}

		static IWebProxy GetProxy (bool ssl)
		{
			var proxy = new WebProxy (ssl ? Settings.SquidAddressSSL : Settings.SquidAddress);
			proxy.Credentials = new NetworkCredential (Settings.SquidUser, Settings.SquidPass);
			return proxy;
		}

		public static HttpWebResponse GetResponse (HttpWebRequest request)
		{
			return (HttpWebResponse)request.GetResponse ();
		}

		public static void RunWebRequest (Request request)
		{
			var wr = CreateWebRequest (request);
			var response = GetResponse (wr);
			Console.WriteLine ("RUN: {0} {1}", request, response.StatusCode);
		}
	}
}

