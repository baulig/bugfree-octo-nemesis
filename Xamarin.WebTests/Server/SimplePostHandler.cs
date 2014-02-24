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
using System.Text;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;

namespace Xamarin.WebTests.Server
{
	using Client;
	using Framework;

	public class SimplePostHandler : Handler
	{
		int readChunkSize = 4096;
		int readChunkMinDelay = 0;
		int readChunkMaxDelay = 0;

		public SimplePostHandler (Listener listener)
			: base (listener, "/post/")
		{
		}

		public override void HandleRequest (Connection connection)
		{
			var query = ParseQuery (connection);
			var mode = ParseMode (query);

			string value;
			if (query.TryGetValue ("readChunkSize", out value))
				readChunkSize = int.Parse (value);
			if (query.TryGetValue ("readChunkMinDelay", out value))
				readChunkMinDelay = int.Parse (value);
			if (query.TryGetValue ("readChunkMaxDelay", out value))
				readChunkMaxDelay = int.Parse (value);

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

				var body = ReadChunkedBody (connection);
				Console.WriteLine ("CHUNKED BODY: |{0}|", body);

				return true;

			default:
				WriteError (connection, "Unknown TransferMode: '{0}'", mode);
				return false;
			}
		}

		bool ReadStaticBody (Connection connection)
		{
			var length = int.Parse (connection.Headers ["Content-Length"]);

			string type;
			if (connection.Headers.TryGetValue ("Content-Type", out type))
				Console.WriteLine ("CONTENT-TYPE: {0} {1}", type);

			var random = new Random ();
			var delayRange = readChunkMaxDelay - readChunkMinDelay;

			var buffer = new char [length];
			int offset = 0;
			while (offset < length) {
				int delay = readChunkMinDelay + random.Next (delayRange);
				Thread.Sleep (delay);

				var size = Math.Min (length - offset, readChunkSize);
				int ret = connection.RequestReader.Read (buffer, offset, size);
				if (ret <= 0) {
					WriteError (connection, "Failed to read body.");
					return false;
				}

				offset += ret;
			}

			return true;
		}

		string ReadChunkedBody (Connection connection)
		{
			var body = new StringBuilder ();

			do {
				var header = connection.RequestReader.ReadLine ();
				var length = int.Parse (header, NumberStyles.HexNumber);
				if (length == 0)
					break;

				var buffer = new char [length];
				var ret = connection.RequestReader.Read (buffer, 0, length);
				if (ret != length)
					throw new InvalidOperationException ();

				var empty = connection.RequestReader.ReadLine ();
				if (!string.IsNullOrEmpty (empty))
					throw new InvalidOperationException ();

				body.Append (buffer);
			} while (true);

			return body.ToString ();
		}

		protected TransferMode ParseMode (IDictionary<string,string> query)
		{
			if (!query.ContainsKey ("mode"))
				return TransferMode.Default;

			switch (query ["mode"].ToLowerInvariant ()) {
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

		public Uri GetUri (TransferMode mode, int? readChunkSize = null, int? readChunkMinDelay = null, int? readChunkMaxDelay = null)
		{
			var query = new StringBuilder ();
			query.AppendFormat ("?mode={0}", GetModeString (mode));
			if (readChunkSize != null)
				query.AppendFormat ("&readChunkSize={0}", readChunkSize);
			if (readChunkMinDelay != null) {
				query.AppendFormat ("&readChunkMinDelay={0}", readChunkMinDelay);
				query.AppendFormat ("&readChunkMaxDelay={0}", readChunkMaxDelay ?? readChunkMinDelay);
			}
			return new Uri (Uri.AbsoluteUri + query.ToString ());
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

