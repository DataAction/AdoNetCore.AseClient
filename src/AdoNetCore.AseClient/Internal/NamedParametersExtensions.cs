using System.Text.RegularExpressions;

namespace AdoNetCore.AseClient.Internal
{
    internal static class NamedParametersExtensions
    {
        private const string Placeholder = "3f256325-c423-49bb-a3ce-7911c5531d95";

        private static readonly Regex SingleQuoteRegex = new Regex(
            $@"(?<='[^']*)({Placeholder})(?=[^']*')",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex SquareBracketRegex = new Regex(
            $@"(?<=\[[^]]*)({Placeholder})(?=[^]]*\])",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex DoubleQuoteRegex = new Regex(
            $@"(?<=""[^""]*)({Placeholder})(?=[^""]*"")",
            RegexOptions.Compiled | RegexOptions.Multiline);

        private static readonly Regex QuestionMarkParameterRegex = new Regex(
            $"({Placeholder})",
            RegexOptions.Compiled | RegexOptions.Multiline);

        public static string ToNamedParameters(this string query)
        {
            // Step 1 - replace all '?' with placeholder.
            query = query.Replace("?", Placeholder);

            // Step 2 - replace instances of the placeholder between square brackets with the original question mark.
            query = SquareBracketRegex.Replace(query, "?");

            // Step 3 - replace instances of the placeholder between double quotes with the original question mark.
            query = DoubleQuoteRegex.Replace(query, "?");

            // Step 4 - replace instances of the placeholder between double quotes with the original question mark.
            query = SingleQuoteRegex.Replace(query, "?");

            // Step 5 - replace instances of the placeholder with parameters.
            var i = 0;
            string Evaluator(Match match) => $"@p{i++}";
            
            return QuestionMarkParameterRegex.Replace(query, Evaluator);
        }
    }
}
