//
// ConnectionReuse.cs
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
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using SD = System.Diagnostics;
using NUnit.Framework;

namespace Xamarin.WebTests.Tests
{
	using Framework;
	using Client;

	[TestFixture]
	[Category("LongRunning")]
	public class ConnectionReuse : WebTestFixture
	{
		[Category("Reuse")]
		[TestCaseSource ("ReuseTests")]
		public void TestConnectionReuse (ReuseTest test)
		{
			int port = -1;
			int connections = 0;
			Debug ("TestReuse", test);
			for (int i = 0; i < test.Count; i++) {
				var puppy = GetPuppy.Get (test.Flags, test.TransferMode);
				if (puppy.RemotePort == port)
					continue;
				if (i > 0)
					Debug ("TestReuse - NEW CONNECTION", i, port, puppy);
				connections++;
				port = puppy.RemotePort;
				if (connections > test.Limit)
					break;
			}

			Assert.That (connections, Is.EqualTo (1), "#1");
		}

		[Category("Connections")]
		[TestCaseSource ("ReuseTests")]
		public void TestConnectionCount (ReuseTest test)
		{
			var puppy = GetPuppy.Get (test.Flags, test.TransferMode);
			var sp = ServicePointManager.FindServicePoint (puppy.Uri);
			Assert.That (sp, Is.Not.Null, "#1");
			Assert.That (sp.CurrentConnections, Is.EqualTo (1), "#2");

			sp.CloseConnectionGroup (string.Empty);
			Assert.That (sp.CurrentConnections, Is.EqualTo (0), "#3");
		}

		[Category("Connections")]
		[TestCaseSource ("ReuseTests")]
		public void TestConnectionIdleTime (ReuseTest test)
		{
			var puppy = GetPuppy.Get (test.Flags, test.TransferMode);
			var sp = ServicePointManager.FindServicePoint (puppy.Uri);
			sp.MaxIdleTime = 2500;

			Assert.That (sp, Is.Not.Null, "#1");
			Assert.That (sp.CurrentConnections, Is.EqualTo (1), "#2");

			Thread.Sleep (2000);

			Assert.That (sp.CurrentConnections, Is.EqualTo (1), "#3");

			Thread.Sleep (600);

			Assert.That (sp.CurrentConnections, Is.EqualTo (0), "#4");
		}

		public static IEnumerable ReuseTests ()
		{
			foreach (var flags in NoProxyFlags)
				foreach (var mode in AllTransferModes)
					yield return new ReuseTest (flags, mode, 250);
		}

		public class ReuseTest : SimpleTest
		{
			public int Count {
				get;
				private set;
			}

			public int Limit {
				get;
				private set;
			}

			public ReuseTest (RequestFlags flags, TransferMode mode, int count, int limit = 10)
				: base (flags, mode)
			{
				Count = count;
				Limit = limit;
			}

			public override string ToString ()
			{
				return string.Format ("[ReuseTest: Flags={0}, TransferMode={1}, Count={2}]", Flags, TransferMode, Count);
			}
		}

	}
}

