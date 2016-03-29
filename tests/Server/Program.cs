using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;

namespace Server
{
	class MainClass
	{
		static string url = Shared.HostHelper.GetHost() + "/TempConvertSoap";

		public static void Main(string[] args)
		{
			Console.WriteLine("Starting server {0}", url);
			using (var serviceHost = OpenHost())
			{
				Console.WriteLine("Listening, press any key to terminate");
				Console.ReadKey();
			}
		}

		public static ServiceHost OpenHost()
		{
			// Init service
			ServiceHost serviceHost = new ServiceHost(typeof(TempConvertSoapImpl), new Uri(url));
			serviceHost.AddServiceEndpoint(typeof(TempConvertSoap), new BasicHttpBinding(), string.Empty);

			serviceHost.Open();

			return serviceHost;
		}
	}
}
