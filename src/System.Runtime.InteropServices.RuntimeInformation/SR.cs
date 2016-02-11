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

static partial class SR
{
        public const string Argument_EmptyValue="Value cannot be empty.";
}
