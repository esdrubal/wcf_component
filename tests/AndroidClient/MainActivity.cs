using System.Reflection;

using Android.App;
using Android.OS;
using Xamarin.Android.NUnitLite;
using System.Threading.Tasks;

namespace AndroidClient
{
	[Activity (Label = "AndroidClient", MainLauncher = true)]
	public class MainActivity : TestSuiteActivity
	{
		protected override void OnCreate (Bundle bundle)
		{
			// tests can be inside the main assembly
			AddTest (Assembly.GetExecutingAssembly ());

			Intent.PutExtra("automated", true);

			// Once you called base.OnCreate(), you cannot add more assemblies.
			base.OnCreate (bundle);
		}
	}
}

