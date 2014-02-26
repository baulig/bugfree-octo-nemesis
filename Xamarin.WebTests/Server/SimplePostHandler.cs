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
		public TransferMode Mode {
			get;
			private set;
		}

		int? readChunkSize;
		int? readChunkMinDelay, readChunkMaxDelay;
		bool? allowWriteBuffering;

		public int? ReadChunkSize {
			get {
				return readChunkSize;
			}
			set {
				WantToModify ();
				readChunkSize = value;
			}
		}

		public int? ReadChunkMinDelay {
			get {
				return readChunkMinDelay;
			}
			set {
				WantToModify ();
				readChunkMinDelay = value;
			}
		}

		public int? ReadChunkMaxDelay {
			get {
				return readChunkMaxDelay;
			}
			set {
				WantToModify ();
				readChunkMaxDelay = value;
			}
		}

		public bool? AllowWriteStreamBuffering {
			get {
				return allowWriteBuffering;
			}
			set {
				WantToModify ();
				allowWriteBuffering = value;
			}
		}

		public SimplePostHandler (Listener listener, TransferMode mode)
			: base (listener)
		{
		}

		protected override bool DoHandleRequest (Connection connection)
		{
			if (!connection.Method.Equals ("POST") && !connection.Method.Equals ("PUT")) {
				WriteError (connection, "Wrong method: {0}", connection.Method);
				return false;
			}

			if (!CheckTransferMode (connection))
				return false;

			WriteSuccess (connection);
			return true;
		}

		bool CheckTransferMode (Connection connection)
		{
			switch (Mode) {
			case TransferMode.Default:
				Console.WriteLine ("DEFAULT: {0}", connection.Headers.ContainsKey ("Content-Length"));
				return ReadStaticBody (connection);

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
				WriteError (connection, "Unknown TransferMode: '{0}'", Mode);
				return false;
			}
		}

		bool ReadStaticBody (Connection connection)
		{
			var length = int.Parse (connection.Headers ["Content-Length"]);

			string type;
			if (connection.Headers.TryGetValue ("Content-Type", out type))
				Console.WriteLine ("CONTENT-TYPE: {0}", type);

			var chunkSize = ReadChunkSize ?? length;
			var minDelay = ReadChunkMinDelay ?? 0;
			var maxDelay = ReadChunkMaxDelay ?? 0;

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

		public HttpWebRequest CreateRequest (string body)
		{
			var request = CreateRequest ();

			if (body != null)
				request.ContentType = "text/plain";
			request.Method = "POST";

			if (AllowWriteStreamBuffering != null)
				request.AllowWriteStreamBuffering = AllowWriteStreamBuffering.Value;

			switch (Mode) {
			case TransferMode.Chunked:
				request.SendChunked = true;
				break;
			case TransferMode.ContentLength:
				if (body == null)
					throw new InvalidOperationException ();
				request.ContentLength = body.Length;
				break;
			}

			if (body != null) {
				using (var writer = new StreamWriter (request.GetRequestStream ())) {
					writer.Write (body);
				}
			}

			return request;
		}
	}
}

