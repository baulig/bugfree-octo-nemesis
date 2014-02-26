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

		public string Description {
			get; set;
		}

		public Task<bool> Task {
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
				Console.WriteLine ("HANDLE REQUEST: {0}", Uri);
				var success = DoHandleRequest (connection);
				Console.WriteLine ("HANDLE REQUEST DONE: {0} {1}", Uri, success);
				tcs.SetResult (success);
			} catch (Exception ex) {
				Console.WriteLine ("HANDLE REQUEST EX: {0} {1}", Uri, ex);
				WriteError (connection, "Caught unhandled exception", ex);
				tcs.SetException (ex);
			}
		}

		protected abstract bool DoHandleRequest (Connection connection);

		public HttpWebRequest CreateRequest ()
		{
			lock (this) {
				if (hasRequest)
					throw new InvalidOperationException ();
				hasRequest = true;
			}

			var request = (HttpWebRequest)HttpWebRequest.Create (Uri);
			CreateRequest (request);
			return request;
		}

		protected virtual void CreateRequest (HttpWebRequest request)
		{
		}
	}
}

