using System;
using System.Net;

namespace Shared
{
	public class HostHelper
	{
		public static string GetHost ()
		{ 
			//var entry = Dns.GetHostEntry (Dns.GetHostName ()); 

			return String.Format ("http://{0}:{1}", "192.168.1.66", 9543);
		}
	}
}

