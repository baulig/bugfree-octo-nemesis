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

namespace Xamarin.WebTests.RemoteServer.Client
{
	using Infrastructure;
	using Framework;

	public sealed class GetPuppy : AbstractPuppy
	{
		GetPuppy ()
		{
		}

		public static GetPuppy Read (HttpWebResponse response)
		{
			var puppy = new GetPuppy ();
			puppy.ReadResponse (response);
			return puppy;
		}

		public static GetPuppy Get (RequestFlags flags, TransferMode mode)
		{
			var request = CreateRequest (flags, mode);
			var response = (HttpWebResponse)request.GetResponse ();
			return Read (response);
		}

		public static HttpWebRequest CreateRequest (RequestFlags flags, TransferMode mode)
		{
			return WebTestFixture.CreateWebRequest ("www/cgi-bin/get-puppy.pl?mode=" + GetModeString (mode), flags);
		}

		public override string ToString ()
		{
			return string.Format ("[GetPuppy: Method={0}, Path={1}, RemoteAddress={2}, RemotePort={3}]", Method, Path, RemoteAddress, RemotePort);
		}
	}
}
