using System.Text;
using System.Text.RegularExpressions;

namespace Nitro
{
    public static class Utilities
    {
        /// <summary>
		/// Makes a string look nicer (example: converts "thisIsATest123" to "This is a test 123"
		/// </summary>
		/// <param name="input">The input string</param>
		/// <returns>The prettified string</returns>
		public static string Prettify(string input)
        {
            StringBuilder builder = new StringBuilder(input);
            if (builder.Length > 0)
            {
                if (char.IsLower(builder[0]))
                {
                    builder[0] = char.ToUpper(builder[0]);
                }
                builder.Replace("_", "");

                var output = builder.ToString();

                output = Regex.Replace(output, @"([a-z])([A-Z])", "$1 $2");
                output = Regex.Replace(output, @"([a-zA-Z])([A-Z])([a-z])", "$1 $2$3");

                return output;
            }

            return input;
        }

        public static string CalculatePlacement(int position)
        {
            if (position == 11 || position == 12 || position == 13)
            {
                return $"{position}th";
            }
            if (position % 10 == 1)
            {
                return $"{position}st";
            }
            else if (position % 10 == 2)
            {
                return $"{position}nd";
            }
            else if (position % 10 == 3)
            {
                return $"{position}rd";
            }
            else
            {
                return $"{position}th";
            }
        }
    }
}
