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
		public class QueryData {
			public TransferMode Mode {
				get;
				private set;
			}

			public int? ReadChunkSize {
				get;
				private set;
			}

			public int? ReadChunkMinDelay {
				get;
				private set;
			}

			public int? ReadChunkMaxDelay {
				get;
				private set;
			}

			static TransferMode ParseMode (IDictionary<string,string> query)
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

			public QueryData (SimplePostHandler handler, Connection connection)
			{
				var query = handler.ParseQuery (connection);

				Mode = ParseMode (query);

				string value;
				if (query.TryGetValue ("readChunkSize", out value))
					ReadChunkSize = int.Parse (value);
				else
					ReadChunkSize = 4096;

				if (query.TryGetValue ("readChunkMinDelay", out value))
					ReadChunkMinDelay = int.Parse (value);
				if (query.TryGetValue ("readChunkMaxDelay", out value))
					ReadChunkMaxDelay = int.Parse (value);
			}

			public QueryData (TransferMode mode, int? readChunkSize = null, int? readChunkMinDelay = null, int? readChunkMaxDelay = null)
			{
				Mode = mode;
				ReadChunkSize = readChunkSize;
				ReadChunkMinDelay = readChunkMinDelay;
				ReadChunkMaxDelay = readChunkMaxDelay;
			}

			public string GetQueryString ()
			{
				var query = new StringBuilder ();
				query.AppendFormat ("?mode={0}", GetModeString (Mode));
				if (ReadChunkSize != null)
					query.AppendFormat ("&readChunkSize={0}", ReadChunkSize);
				if (ReadChunkMinDelay != null) {
					query.AppendFormat ("&readChunkMinDelay={0}", ReadChunkMinDelay);
					query.AppendFormat ("&readChunkMaxDelay={0}", ReadChunkMaxDelay ?? ReadChunkMinDelay);
				}
				return query.ToString ();
			}
		}

		public SimplePostHandler (Listener listener)
			: base (listener, "/post/")
		{
		}

		public override void HandleRequest (Connection connection)
		{
			if (!connection.Method.Equals ("POST") && !connection.Method.Equals ("PUT")) {
				WriteError (connection, "Wrong method: {0}", connection.Method);
				return;
			}

			var query = new QueryData (this, connection);

			if (!CheckTransferMode (connection, query))
				return;

			WriteSuccess (connection);
		}

		bool CheckTransferMode (Connection connection, QueryData query)
		{
			switch (query.Mode) {
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

				return ReadStaticBody (connection, query);

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
				WriteError (connection, "Unknown TransferMode: '{0}'", query.Mode);
				return false;
			}
		}

		bool ReadStaticBody (Connection connection, QueryData query)
		{
			var length = int.Parse (connection.Headers ["Content-Length"]);

			string type;
			if (connection.Headers.TryGetValue ("Content-Type", out type))
				Console.WriteLine ("CONTENT-TYPE: {0} {1}", type);

			var chunkSize = query.ReadChunkSize ?? 4096;
			var minDelay = query.ReadChunkMinDelay ?? 0;
			var maxDelay = query.ReadChunkMaxDelay ?? 0;

			var random = new Random ();
			var delayRange = maxDelay - minDelay;

			var buffer = new char [length];
			int offset = 0;
			while (offset < length) {
				int delay = minDelay + random.Next (delayRange);
				Console.WriteLine ("READ STATIC BODY: {0} {1} {2}", offset, length, delay);
				Thread.Sleep (delay);

				var size = Math.Min (length - offset, chunkSize);
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

		public Uri GetUri (QueryData query)
		{
			return new Uri (Uri.AbsoluteUri + query.GetQueryString ());
		}

		public HttpWebRequest CreateRequest (TransferMode mode, string body)
		{
			return CreateRequest (new QueryData (mode), body);
		}

		public HttpWebRequest CreateRequest (QueryData query, string body)
		{
			var request = (HttpWebRequest)HttpWebRequest.Create (GetUri (query));

			request.ContentType = "text/plain";
			request.Method = "POST";

			switch (query.Mode) {
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

