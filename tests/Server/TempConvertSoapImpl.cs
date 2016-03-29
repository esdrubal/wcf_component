using System;
using System.Globalization;

namespace Server
{
	class TempConvertSoapImpl : TempConvertSoap
	{
		public FahrenheitToCelsiusResponse FarenheitToCelsius (FahrenheitToCelsiusRequest request)
		{
			var farenheit = double.Parse (request.Body.Fahrenheit, CultureInfo.InvariantCulture);
			var celsius = ((farenheit - 32) / 9) * 5;
			return new FahrenheitToCelsiusResponse (new FahrenheitToCelsiusResponseBody (celsius.ToString (CultureInfo.InvariantCulture)));
		}

		public CelsiusToFahrenheitResponse CelsiusToFarenheit (CelsiusToFahrenheitRequest request)
		{
			var celsius = double.Parse (request.Body.Celsius, CultureInfo.InvariantCulture);
			var farenheit = ((celsius * 9) / 5) + 32;
			return new CelsiusToFahrenheitResponse (new CelsiusToFahrenheitResponseBody (farenheit.ToString (CultureInfo.InvariantCulture)));
		}

		Func<FahrenheitToCelsiusRequest,FahrenheitToCelsiusResponse> farenheitToCelsius;
		Func<CelsiusToFahrenheitRequest,CelsiusToFahrenheitResponse> celsiusToFarenheit;

		public IAsyncResult BeginFahrenheitToCelsius (FahrenheitToCelsiusRequest request, AsyncCallback callback, object asyncState)
		{
			if (farenheitToCelsius == null)
				farenheitToCelsius = new Func<FahrenheitToCelsiusRequest,FahrenheitToCelsiusResponse> (FarenheitToCelsius);
			return farenheitToCelsius.BeginInvoke (request, callback, asyncState);
		}

		public FahrenheitToCelsiusResponse EndFahrenheitToCelsius (IAsyncResult result)
		{
			return farenheitToCelsius.EndInvoke (result);
		}

		public IAsyncResult BeginCelsiusToFahrenheit (CelsiusToFahrenheitRequest request, AsyncCallback callback, object asyncState)
		{
			if (celsiusToFarenheit == null)
				celsiusToFarenheit = new Func<CelsiusToFahrenheitRequest,CelsiusToFahrenheitResponse> (CelsiusToFarenheit);
			return celsiusToFarenheit.BeginInvoke (request, callback, asyncState);
		}

		public CelsiusToFahrenheitResponse EndCelsiusToFahrenheit (IAsyncResult result)
		{
			return celsiusToFarenheit.EndInvoke (result);
		}
	}
}

