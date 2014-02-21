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
using System.Collections.Generic;

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
			var query = ParseQuery (connection);
			var mode = ParseMode (query);

			if (!CheckTransferMode (connection, mode))
				return;

			WriteSuccess (connection);
		}

		bool CheckTransferMode (Connection connection, TransferMode mode)
		{
			Console.WriteLine ("TRANSFER MODE: {0}", mode);

			switch (mode) {
			case TransferMode.Default:
			case TransferMode.ContentLength:
				if (!connection.Headers.ContainsKey ("Content-Length")) {
					WriteError (connection, "Missing Content-Length");
					return false;
				}

				if (connection.Headers.ContainsKey ("Transfer-Encoding")) {
					WriteError (connection, "Transfer-Encoding header not allowed");
					return false;
				}

				return ReadStaticBody (connection);

			case TransferMode.Chunked:
				if (connection.Headers.ContainsKey ("Content-Length")) {
					WriteError (connection, "Content-Length header not allowed");
					return false;
				}

				if (!connection.Headers.ContainsKey ("Transfer-Encoding")) {
					WriteError (connection, "Missing Transfer-Encoding header");
					return false;
				}

				var transferEncoding = connection.Headers ["Transfer-Encoding"];
				if (!string.Equals (transferEncoding, "chunked", StringComparison.InvariantCultureIgnoreCase)) {
					WriteError (connection, "Invalid Transfer-Encoding header: '{0}'", transferEncoding);
					return false;
				}

				return true;

			default:
				WriteError (connection, "Unknown TransferMode: '{0}'", mode);
				return false;
			}
		}

		bool ReadStaticBody (Connection connection)
		{
			var length = int.Parse (connection.Headers ["Content-Length"]);

			var buffer = new char [length];
			int ret = connection.RequestReader.Read (buffer, 0, length);

			if (ret != length) {
				WriteError (connection, "Expected {0} bytes, but got {1}.", length, ret);
				return false;
			}

			return true;
		}

		protected TransferMode ParseMode (IDictionary<string,string> query)
		{
			if (!query.ContainsKey ("mode"))
				return TransferMode.Default;

			switch (query ["mode"]) {
			case "chunked":
				return TransferMode.Chunked;
			case "length":
				return TransferMode.ContentLength;
			case "default":
				return TransferMode.Default;
			default:
				throw new InvalidOperationException ();
			}
		}

		protected static string GetModeString (TransferMode mode)
		{
			switch (mode) {
			case TransferMode.Chunked:
				return "chunked";
			case TransferMode.ContentLength:
				return "length";
			default:
				return "default";
			}
		}

		public HttpWebRequest CreateRequest (TransferMode mode, string body)
		{
			var request = (HttpWebRequest)HttpWebRequest.Create (Uri.AbsoluteUri + "?mode=" + GetModeString (mode));

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

