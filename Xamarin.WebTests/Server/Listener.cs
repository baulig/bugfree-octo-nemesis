//
// Listener.cs
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
using System.Net.Sockets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Xamarin.WebTests.Server
{
	public class Listener
	{
		TcpListener listener;
		Dictionary<string,Site> sites;
		Uri uri;

		public Listener (int port)
		{
			uri = new Uri (string.Format ("http://127.0.0.1:{0}/", port));
			listener = new TcpListener (IPAddress.Loopback, port);
			sites = new Dictionary<string, Site> ();
		}

		public void Start ()
		{
			listener.Start ();
			listener.BeginAcceptSocket (AcceptSocketCB, null);
		}

		public Uri RegisterSite (string path, Handler handler)
		{
			sites.Add (path, new Site (path, handler));
			return new Uri (uri, path);
		}

		public Site GetSite (string path)
		{
			return sites [path];
		}

		public Uri Uri {
			get { return uri; }
		}

		void AcceptSocketCB (IAsyncResult ar)
		{
			var socket = listener.EndAcceptSocket (ar);
			Console.WriteLine ("GOT SOCKET: {0}", socket);

			try {
				HandleConnection (socket);
			} catch (Exception ex) {
				Console.WriteLine ("EX: {0}", ex);
				throw;
			} finally {
				socket.Close ();
			}
		}

		void HandleConnection (Socket socket)
		{
			var connection = new Connection (this, socket);
			connection.ReadHeaders ();

			Console.WriteLine ("PATH: {0} - {1} {2}", connection.RequestUri, connection.RequestUri.AbsolutePath, connection.RequestUri.Query);

			var site = GetSite (connection.RequestUri.AbsolutePath);
			site.Handler.HandleRequest (connection);

			connection.Close ();
		}

	}
}

