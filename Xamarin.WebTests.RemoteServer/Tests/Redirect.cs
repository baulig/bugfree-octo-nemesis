//
// Redirect.cs
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
using System.IO;
using System.Net;
using System.Collections;
using NUnit.Framework;

namespace Xamarin.WebTests.RemoteServer.Tests
{
	using Infrastructure;
	using Framework;
	using Client;

	[TestFixture]
	public class Redirect : WebTestFixture
	{
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
			var response = GetResponse (path, test.Flags | PuppyFlags.AutoRedirect);
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

		[Category ("Work")]
		[TestCaseSource ("RedirectTests")]
		public void PostRedirect (RedirectTest test)
		{
			var prefix = string.Format ("redirects/same-server/{0}", (int)test.Code);
			var puppy = PostPuppy.CreateRequest (PuppyFlags.AutoRedirect, TransferMode.Chunked, prefix, "Hello Chunked World!");
			Console.WriteLine ("PUPPY: {0}", puppy);

			HttpWebResponse response;
			try {
				response = (HttpWebResponse)puppy.GetResponse ();
				Console.WriteLine ("GOT RESPONSE: {0}", response.StatusCode);
			} catch (WebException ex) {
				response = (HttpWebResponse)ex.Response;
				Console.WriteLine ("GOT ERROR RESPONSE: {0}", response.StatusCode);
			}

			using (var reader = new StreamReader (response.GetResponseStream ())) {
				var body = reader.ReadToEnd ();
				Console.WriteLine ("GOT RESPONSE BODY: {0}", body);
			}

			response.Close ();
		}

		public static IEnumerable RedirectTests ()
		{
			foreach (var flags in AllFlags)
				foreach (var code in AllRedirectCodes)
					yield return new RedirectTest { Flags = flags, Code = code };
		}

		public static IEnumerable Broken ()
		{
			yield return new RedirectTest { Flags = PuppyFlags.UseProxy, Code = HttpStatusCode.Redirect };
		}

		public class RedirectTest
		{
			public PuppyFlags Flags {
				get;
				set;
			}

			public HttpStatusCode Code {
				get; set;
			}

			public override string ToString ()
			{
				return string.Format ("[RedirectTest: Flags={0}, Code={1}]", Flags, Code);
			}
		}


	}
}

