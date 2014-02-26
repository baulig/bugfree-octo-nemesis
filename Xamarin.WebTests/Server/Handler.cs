//
// Behavior.cs
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
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Xamarin.WebTests.Server
{
	public abstract class Handler
	{
		public Listener Listener {
			get;
			private set;
		}

		public Uri Uri {
			get;
			private set;
		}

		public Task Task {
			get { return tcs.Task; }
		}

		bool hasRequest;
		TaskCompletionSource<bool> tcs;

		protected void WantToModify ()
		{
			if (hasRequest)
				throw new InvalidOperationException ();
		}

		protected Handler (Listener listener)
		{
			Listener = listener;

			tcs = new TaskCompletionSource<bool> ();
			Uri = Listener.RegisterHandler (this);
		}

		protected IDictionary<string,string> ParseQuery (Connection connection)
		{
			var dict = new Dictionary<string,string> ();
			var query = connection.RequestUri.Query;
			if (string.IsNullOrEmpty (query))
				return dict;
			if (query [0] != '?')
				throw new InvalidOperationException ();

			query = query.Substring (1);
			var parts = query.Split (new char[] { '&' }, StringSplitOptions.None);
			Console.WriteLine ("QUERY PARTS: {0}", parts.Length);

			foreach (var part in parts) {
				int pos = part.IndexOf ('=');
				var key = part.Substring (0, pos);
				var value = part.Substring (pos + 1);
				dict.Add (key, value);
				Console.WriteLine ("QUERY: {0} {1}", key, value);
			}

			return dict;
		}

		protected void WriteError (Connection connection, string message, params object[] args)
		{
			WriteSimpleResponse (connection, 500, "ERROR", string.Format (message, args));
		}

		protected void WriteSuccess (Connection connection, string body = null)
		{
			WriteSimpleResponse (connection, 200, "OK", body);
		}

		protected void WriteSimpleResponse (Connection connection, int status, string message, string body)
		{
			Console.WriteLine ("RESPONSE: {0} {1} {2}", status, message, body);

			connection.ResponseWriter.WriteLine ("HTTP/1.1 {0} {1}", status, message);
			connection.ResponseWriter.WriteLine ("Content-Type: text/plain");
			if (body != null) {
				connection.ResponseWriter.WriteLine ("Content-Length: {0}", body.Length);
				connection.ResponseWriter.WriteLine ("");
				connection.ResponseWriter.WriteLine (body);
			} else {
				connection.ResponseWriter.WriteLine ("");
			}
		}

		public void HandleRequest (Connection connection)
		{
			try {
				var success = DoHandleRequest (connection);
				tcs.SetResult (success);
			} catch (Exception ex) {
				tcs.SetException (ex);
			}
		}

		protected abstract bool DoHandleRequest (Connection connection);

		protected HttpWebRequest CreateRequest ()
		{
			lock (this) {
				if (hasRequest)
					throw new InvalidOperationException ();
				hasRequest = true;
			}

			return (HttpWebRequest)HttpWebRequest.Create (Uri);
		}
	}
}

