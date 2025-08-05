using System.Text;

namespace MSBee.Tasks10;

public static class StringExtensions {
    public static string Replace(this string str, string oldValue, string? newValue, StringComparison comparisonType) {
        // Check inputs.
        if (str == null) {
            // Same as original .NET C# string.Replace behavior.
            throw new ArgumentNullException(nameof(str));
        }

        if (oldValue == null) {
            // Same as original .NET C# string.Replace behavior.
            throw new ArgumentNullException(nameof(oldValue));
        }

        if (oldValue.Length == 0) {
            // Same as original .NET C# string.Replace behavior.
            throw new ArgumentException("String cannot be of zero length.");
        }

        if (str.Length == 0) {
            // Same as original .NET C# string.Replace behavior.
            return str;
        }

        // Prepare string builder for storing the processed string.
        // Note: StringBuilder has a better performance than String by 30-40%.
        var resultStringBuilder = new StringBuilder(str.Length);

        // Analyze the replacement: replace or remove.
        var isReplacementNullOrEmpty = string.IsNullOrEmpty(newValue);

        // Replace all values.
        const int valueNotFound = -1;
        int foundAt;
        var startSearchFromIndex = 0;

        while ((foundAt = str.IndexOf(oldValue, startSearchFromIndex, comparisonType)) != valueNotFound) {
            // Append all characters until the found replacement.
            var charsUntilReplacment = foundAt - startSearchFromIndex;
            var isNothingToAppend = charsUntilReplacment == 0;

            if (!isNothingToAppend) {
                resultStringBuilder.Append(str, startSearchFromIndex, charsUntilReplacment);
            }

            // Process the replacement.
            if (!isReplacementNullOrEmpty) {
                resultStringBuilder.Append(newValue);
            }

            // Prepare start index for the next search.
            // This needed to prevent infinite loop, otherwise method always start search
            // from the start of the string. For example: if an oldValue == "EXAMPLE", newValue == "example"
            // and comparisonType == "any ignore case" will conquer to replacing:
            // "EXAMPLE" to "example" to "example" to "example" … infinite loop.
            startSearchFromIndex = foundAt + oldValue.Length;

            if (startSearchFromIndex == str.Length) {
                // It is end of the input string: no more space for the next search.
                // The input string ends with a value that has already been replaced.
                // Therefore, the string builder with the result is complete and no further action is required.
                return resultStringBuilder.ToString();
            }
        }

        // Append the last part to the result.
        var charsUntilStringEnd = str.Length - startSearchFromIndex;

        resultStringBuilder.Append(str, startSearchFromIndex, charsUntilStringEnd);

        return resultStringBuilder.ToString();
    }
}
