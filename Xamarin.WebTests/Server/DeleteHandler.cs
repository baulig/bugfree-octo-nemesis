//
// DeleteHandler.cs
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

	public class DeleteHandler : Handler
	{
		public class QueryData {
			public bool HasBody {
				get; set;
			}

			public QueryData (DeleteHandler handler, Connection connection)
			{
				var query = handler.ParseQuery (connection);

				string value;
				if (query.TryGetValue ("hasBody", out value))
					HasBody = bool.Parse (value);
			}

			public QueryData (bool hasBody)
			{
				HasBody = hasBody;
			}

			public string GetQueryString ()
			{
				return HasBody ? "?hasBody=true" : string.Empty;
			}
		}

		public DeleteHandler (Listener listener)
			: base (listener, "/delete/")
		{
		}

		public override void HandleRequest (Connection connection)
		{
			if (!connection.Method.Equals ("DELETE")) {
				WriteError (connection, "Wrong method: {0}", connection.Method);
				return;
			}

			var query = new QueryData (this, connection);

			if (CheckRequest (connection, query))
				WriteSuccess (connection);
		}

		bool CheckRequest (Connection connection, QueryData query)
		{
			string value;
			if (query.HasBody) {
				if (!connection.Headers.TryGetValue ("Content-Length", out value)) {
					WriteError (connection, "Missing Content-Length");
					return false;
				}

				int length = int.Parse (value);
				return ReadStaticBody (connection, length);
			} else {
				if (connection.Headers.ContainsKey ("Content-Length")) {
					WriteError (connection, "Content-Length not allowed");
					return false;
				}

				return true;
			}
		}

		bool ReadStaticBody (Connection connection, int length)
		{
			var buffer = new char [length];
			int offset = 0;
			while (offset < length) {
				var size = Math.Min (length - offset, 4096);
				int ret = connection.RequestReader.Read (buffer, offset, size);
				if (ret <= 0) {
					WriteError (connection, "Failed to read body.");
					return false;
				}

				offset += ret;
			}

			return true;
		}

		public HttpWebRequest CreateRequest (string body = null)
		{
			var query = new QueryData (body != null);
			var request = (HttpWebRequest)HttpWebRequest.Create (Uri.AbsoluteUri + query.GetQueryString ());
			request.Method = "DELETE";

			if (body != null) {
				using (var writer = new StreamWriter (request.GetRequestStream ())) {
					writer.Write (body);
				}
			}

			return request;
		}
	}
}
