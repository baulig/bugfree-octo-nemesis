//
// AbstractPuppy.cs
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
using System.Text.RegularExpressions;

namespace Xamarin.WebTests.Client
{
	public abstract class AbstractPuppy
	{
		public Uri Uri {
			get;
			private set;
		}

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

		public TransferMode TransferMode {
			get;
			private set;
		}

		protected void ReadResponse (HttpWebResponse response)
		{
			if (response.StatusCode != HttpStatusCode.OK)
				throw new InvalidOperationException ();

			Uri = response.ResponseUri;

			using (var reader = new StreamReader (response.GetResponseStream ())) {
				ReadResponse (reader);
			}
		}

		protected virtual void ReadResponse (StreamReader reader)
		{
			string line;
			while ((line = reader.ReadLine ()) != null) {
				var match = Regex.Match (line, @"^([\w_]+):\s*(.*)$");
				if (match == null || !match.Success)
					break;

				var key = match.Groups [1].Value;
				var value = match.Groups [2].Value;
				ProcessResponseHeader (key, value);
			}
		}

		protected virtual void ProcessResponseHeader (string key, string value)
		{
			switch (key) {
			case "METHOD":
				Method = value;
				break;
			case "PATH":
				Path = value;
				break;
			case "REMOTE":
				RemoteAddress = value;
				break;
			case "PORT":
				RemotePort = int.Parse (value);
				break;
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
	}
}

