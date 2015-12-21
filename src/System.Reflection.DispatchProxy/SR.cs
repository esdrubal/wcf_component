using System;
using System.Globalization;

static partial class SR
{
	internal static string Format (string name, params object[] args)
	{
		return Format (CultureInfo.InvariantCulture, name, args);
	}

	internal static string Format (CultureInfo culture, string name, params object[] args)
	{
		return string.Format (culture, name, args);
	}

	internal static string Format (string name)
	{
		return name;
	}

	internal static string Format (CultureInfo culture, string name)
	{
		return name;
	}
}

// Use the following script to extract strings from .NET strings.resx:
//
// csharp -r:System.Xml.Linq.dll
// using System.Xml.Linq;
// using System.Xml.XPath
// var d = XDocument.Load ("external/corefx/src/System.Reflection.DispatchProxy/src/Resources/Strings.resx");
// foreach (var j in d.XPathSelectElements ("/root/data")){ var v = j.XPathSelectElement ("value"); Console.WriteLine ("\tpublic const string {0}=\"{1}\";", j.Attribute ("name").Value, v.Value); }
//

static partial class SR
{
	public const string BaseType_Cannot_Be_Sealed="The base type '{0}' cannot be sealed.";
	public const string BaseType_Must_Have_Default_Ctor="The base type '{0}' must have a public parameterless constructor.";
	public const string InterfaceType_Must_Be_Interface="The type '{0}' must be an interface, not a class.";
	public const string BaseType_Cannot_Be_Abstract="The base type '{0}' cannot be abstract.";
}
