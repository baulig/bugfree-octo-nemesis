//
// SimplePostHandler.cs
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

namespace Xamarin.WebTests.Server
{
	using Client;
	using Framework;

	public class SimplePostHandler : Handler
	{
		public SimplePostHandler (Listener listener)
			: base (listener, "/post/")
		{
		}

		public override void HandleRequest (Connection connection)
		{
			connection.ResponseWriter.WriteLine ("HTTP/1.1 200 OK");
			connection.ResponseWriter.WriteLine ("");
			connection.ResponseWriter.WriteLine ("Hello World!");
		}

		public HttpWebRequest CreateRequest (TransferMode mode, string body)
		{
			var request = (HttpWebRequest)HttpWebRequest.Create (Uri);
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
	}
}

