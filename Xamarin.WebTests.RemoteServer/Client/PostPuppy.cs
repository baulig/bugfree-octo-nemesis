//
// PostPuppy.cs
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

namespace Xamarin.WebTests.RemoteServer.Client
{
	using Infrastructure;
	using Framework;

	public sealed class PostPuppy : AbstractPuppy
	{
		PostPuppy ()
		{
			ContentLength = -1;
		}

		public int ContentLength {
			get;
			private set;
		}

		public string SimpleBody {
			get;
			private set;
		}

		public string FullBody {
			get;
			private set;
		}

		public static PostPuppy Read (HttpWebResponse response)
		{
			if (response.StatusCode != HttpStatusCode.OK)
				throw new InvalidOperationException ();

			var puppy = new PostPuppy ();
			puppy.ReadResponse (response);
			return puppy;
		}

		public static HttpWebRequest CreateRequest (RequestFlags flags, TransferMode mode, string body)
		{
			var request = WebTestFixture.CreateWebRequest ("www/cgi-bin/post-puppy.pl?mode=" + GetModeString (mode), flags);
			request.ContentType = "text/plain";
			request.Method = "POST";

			switch (mode) {
			case TransferMode.Chunked:
				request.SendChunked = true;
				break;
			case TransferMode.ContentLength:
				request.ContentLength = body.Length;
				break;
			}

			using (var writer = new StreamWriter (request.GetRequestStream ())) {
				writer.Write (body);
			}

			return request;
		}

		protected override void ProcessResponseHeader (string key, string value)
		{
			switch (key) {
			case "LENGTH":
				ContentLength = string.IsNullOrEmpty (value) ? -1 : int.Parse (value);
				break;
			case "BODY":
				SimpleBody = string.IsNullOrEmpty (value) ? null : value;
				break;
			default:
				base.ProcessResponseHeader (key, value);
				break;
			}
		}

		protected override void ReadResponse (StreamReader reader)
		{
			base.ReadResponse (reader);
			if (!reader.EndOfStream)
				FullBody = reader.ReadToEnd ();
		}

		public override string ToString ()
		{
			return string.Format ("[PostPuppy: Method={0}, Path={1}, RemoteAddress={2}, RemotePort={3}]", Method, Path, RemoteAddress, RemotePort);
		}
	}
}

