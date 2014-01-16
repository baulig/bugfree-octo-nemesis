//
// MainClass.cs
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
	using Client;

	public static class MainClass
	{
		public static void Run ()
		{
			RunTheTests ();
		}

		static void RunTheTests ()
		{
			var test = new WebRequests ();
			test.TestReuse (new ReuseTest (RequestFlags.None, TransferMode.ContentLength, 25, 5));
		}

		static void GetPuppySsl ()
		{
			var start = DateTime.Now;
			int port = 0;

			for (int i = 0; i < 1000; i++) {
				var puppy = GetPuppy.Get (RequestFlags.UseSSL, TransferMode.Chunked);
				if (puppy.RemotePort != port) {
					Console.WriteLine ("NEW PORT: #{0}: {1}", i, puppy);
					port = puppy.RemotePort;
					continue;
				}
				if ((i % 10) == 0)
					Console.WriteLine ("#{0}: {1}", i, puppy);
			}

			var end = DateTime.Now;
			Console.WriteLine ("Total time: {0}", end - start);
		}

		static void WildcardRun (string rootDomain)
		{
			for (int i = 0; i < 1000; i++) {
				var url = string.Format ("http://{0}.{1}/", i, rootDomain);
				var request = HttpWebRequest.Create (url) as HttpWebRequest;
				var response = request.GetResponse () as HttpWebResponse;
				Console.WriteLine ("TEST: {0} {1}", i, response.StatusCode);
				using (var stream = new StreamReader (response.GetResponseStream ())) {
					stream.ReadToEnd ();
				}
			}
		}
	}
}

