//
// GetPuppy.cs
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
using System.Net;
using System.Text.RegularExpressions;

namespace Xamarin.WebTests
{
	public class GetPuppy
	{
		public string Method {
			get;
			private set;
		}

		public string Path {
			get;
			private set;
		}

		public string RemoteAddress {
			get;
			private set;
		}

		public int RemotePort {
			get;
			private set;
		}

		private GetPuppy ()
		{
		}

		public static GetPuppy Read (HttpWebResponse response)
		{
			if (response.StatusCode != HttpStatusCode.OK)
				throw new InvalidOperationException ();

			var puppy = new GetPuppy ();

			using (var reader = new StreamReader (response.GetResponseStream ())) {
				string line;
				while ((line = reader.ReadLine ()) != null) {
					var match = Regex.Match (line, @"^([\w_]+):\s*(.*)$");
					if (match == null)
						break;

					var key = match.Groups [1].Value;
					var value = match.Groups [2].Value;
					switch (key) {
					case "METHOD":
						puppy.Method = value;
						break;
					case "PATH":
						puppy.Path = value;
						break;
					case "REMOTE":
						puppy.RemoteAddress = value;
						break;
					case "PORT":
						puppy.RemotePort = int.Parse (value);
						break;
					}
				}
			}

			return puppy;
		}

		public static GetPuppy Get (RequestFlags flags)
		{
			var request = WebRequests.CreateWebRequest ("www/cgi-bin/get-puppy.pl", flags);
			var response = (HttpWebResponse)request.GetResponse ();
			return Read (response);
		}

		public override string ToString ()
		{
			return string.Format ("[GetPuppy: Method={0}, Path={1}, RemoteAddress={2}, RemotePort={3}]", Method, Path, RemoteAddress, RemotePort);
		}
	}
}
