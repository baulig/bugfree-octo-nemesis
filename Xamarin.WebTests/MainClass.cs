//
// MainClass.cs
//
// Author:
//       Martin Baulig <martin.baulig@xamarin.com>
//
// Copyright (c) 2013 Xamarin Inc. (http://www.xamarin.com)
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
using System.Text;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Net;
using NUnitLite.Runner;
using NUnit.Framework.Internal;

namespace Xamarin.WebTests
{
	using Framework;
	using Client;
	using Server;

	public class MainClass
	{
		Listener listener;

		public static void Run ()
		{
			var setCtx = typeof(TestExecutionContext).GetMethod ("SetCurrentContext", BindingFlags.Static | BindingFlags.NonPublic);
			setCtx.Invoke (null, new object[] { new TestExecutionContext () });

			var main = new MainClass ();
			main.TestServer ();
		}

		static void RunTheTests ()
		{
			var test = new Simple ();
			test.Post_EmptyRequestStream ();
			test.TestPost (new SimpleTest (RequestFlags.None, TransferMode.Default));
			test.TestPost (new SimpleTest (RequestFlags.None, TransferMode.ContentLength));
			test.TestPost (new SimpleTest (RequestFlags.None, TransferMode.Chunked));
		}

		void TestServer ()
		{
			listener = new Listener (9999);

			foreach (var test in GetPostTests ())
				TestPost (test);
			foreach (var test in GetDeleteTests ())
				TestPost (test);
		}

		IEnumerable<PostHandler> GetPostTests ()
		{
			yield return new PostHandler (listener) { Description = "No request stream" };
			yield return new PostHandler (listener) { Description = "Empty request stream", Body = string.Empty };
			yield return new PostHandler (listener) { Body = "Hello World!" };
		}

		IEnumerable<DeleteHandler> GetDeleteTests ()
		{
			yield return new DeleteHandler (listener);
			yield return new DeleteHandler (listener) { Description = "DELETE with empty request stream", Body = string.Empty };
			yield return new DeleteHandler (listener) { Description = "DELETE with request body", Body = "I have a body!" };
		}

		void TestPost (Handler handler)
		{
			var request = handler.CreateRequest ();
			var response = (HttpWebResponse)request.GetResponse ();

			try {
				Console.WriteLine ("GOT RESPONSE: {0}", response.StatusCode);
				Console.WriteLine ("TEST POST DONE: {0} {1}", handler.Task.IsCompleted, handler.Task.IsFaulted);
			} finally {
				response.Close ();
			}
		}
	}
}

