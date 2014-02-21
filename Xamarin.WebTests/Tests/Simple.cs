//
// Simple.cs
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

namespace Xamarin.WebTests
{
	using Framework;
	using Client;

	[TestFixture]
	public class Simple : WebTestFixture
	{
		[Category("Simple")]
		[TestCaseSource("AllFlags")]
		public void TestSimpleGet (RequestFlags flags)
		{
			var response = GetResponse ("www/index.html", flags);
			try {
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode);
			} finally {
				response.Close ();
			}
		}

		[Category("Work")]
		[TestCaseSource("SimpleTests")]
		public void TestGet (SimpleTest test)
		{
			GetPuppy.Get (test.Flags, test.TransferMode);
			Assert.Pass ();
		}

		[Category("Work")]
		[TestCaseSource("SimpleTests")]
		public void TestPost (SimpleTest test)
		{
			var text = "Hello World";
			var request = PostPuppy.CreateRequest (test.Flags, test.TransferMode, text);

			var response = (HttpWebResponse)request.GetResponse ();
			try {
				Assert.AreEqual (HttpStatusCode.OK, response.StatusCode, "#1");
				var puppy = PostPuppy.Read (response);

				if (test.TransferMode != TransferMode.Chunked) {
					Assert.AreEqual (text.Length, puppy.ContentLength, "#2a");
					Assert.AreEqual (text, puppy.SimpleBody, "#3a");
					Assert.IsNull (puppy.FullBody, "#4b");
				} else {
					Assert.AreEqual (-1, puppy.ContentLength, "#2b");
					Assert.IsNull (puppy.SimpleBody, "#3b");
					Assert.AreEqual (text, puppy.FullBody, "#4");
				}
			} finally {
				response.Close ();
			}
		}

		[Category("Simple")]
		[TestCaseSource("AllFlags")]
		public void TestNoWriteStreamBuffering (RequestFlags flags)
		{
			var req = GetPuppy.CreateRequest (flags, TransferMode.Default);

			req.Method = "POST";
			req.KeepAlive = false;
			req.AllowWriteStreamBuffering = false;

			var ar = req.BeginGetRequestStream (null, null);
			var rs = req.EndGetRequestStream (ar);
			rs.Close ();
		}

		[Category("Work")]
		[Test]
		// Bug6737
		// This test is supposed to fail prior to .NET 4.0
		public void Post_EmptyRequestStream ()
		{
			var wr = HttpWebRequest.Create ("http://google.com");
			wr.Method = "POST";
			wr.GetRequestStream ();

			var gr = wr.BeginGetResponse (delegate { }, null);
			Assert.AreEqual (true, gr.AsyncWaitHandle.WaitOne (5000), "#1");
		}

		public static IEnumerable SimpleTests ()
		{
			foreach (var flags in NoProxyFlags)
				foreach (var mode in AllTransferModes)
					yield return new SimpleTest (flags, mode);
		}
	}
}

