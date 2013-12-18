//
// Settings.cs
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
using System.Configuration;

namespace Xamarin.WebTests
{
	public static class Settings
	{
		public static readonly string WebRoot;
		public static readonly string WebHost;
		public static readonly string WebPrefix;
		public static readonly string SquidAddress;
		public static readonly string SquidAddressSSL;
		public static readonly string SquidUser;
		public static readonly string SquidPass;

		static Settings ()
		{
			WebRoot = GetOption ("web_root");
			WebHost = GetOption ("web_host");
			WebPrefix = GetOption ("web_prefix");
			SquidAddress = GetOption ("squid_address");
			SquidAddressSSL = GetOption ("squid_address_ssl");
			SquidUser = GetOption ("squid_user", true);
			SquidPass = GetOption ("squid_pass", true);
		}

		static string GetOption (string name, bool optional = false)
		{
			var value = ConfigurationManager.AppSettings [name];
			if (!optional && value == null)
				throw new InvalidOperationException ();
			return value;
		}
	}
}

