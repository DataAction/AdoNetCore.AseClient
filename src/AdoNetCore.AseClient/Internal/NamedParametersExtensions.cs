using System;
using System.Text;
using System.Text.RegularExpressions;

namespace AdoNetCore.AseClient.Internal
{
    internal static class NamedParametersExtensions
    {
        // All patterns are intended to be joined into a single regexp pattern
        // As such, they are all intended to be mutually exclusive
        // i.e. starting at any index i, no two patterns should find a match starting at the same position j

        // SQL quoted string 'strings with '' escaping'
        internal const string AnsiString = "'(?:[^']|'')*'";

        // SQL quoted identifier "id with "" escaping"
        internal const string AnsiQuotedIdentifier = "\"(?:[^\"]|\"\")*\"";

        // SQL square quoted identifier [id with ]] escaping]
        internal const string SquareQuotedIdentifier = @"\[(?:[^\]]|\]\])*]";

        // SQL inline comments --
        internal const string AnsiLineComment = @"--[^\n]*(?:\n|$)";

        // C style comments, both inline // and block /* */ 
        internal const string CComment = @"/(?:/[^\n]*(?:\n|$)|\*.*?\*/)";

        // Simple sequence of characters that aren't part of comments, strings,
        // quoted identifiers, or the question marks we are trying to separate
        // NOTE: negative lookahead used to consume / and - when they aren't forming a comment
        // They can be removed, but would require merging all the individual patterns and impact testability and maintainability
        internal const string SimpleSqlPart = "(?:[^'\"[/?-]|/(?![/*])|-(?!-))+";

        // Final two patterns: one to tokenise sql strings,
        // and the final to merge the tokenised input and collect positional parameters ?

        // A single non-question-mark piece of sql.
        // It could be any of a quoted identifier, string, comment, or character sequence
        internal const string SqlPart = AnsiString
            + "|" + AnsiQuotedIdentifier
            + "|" + SquareQuotedIdentifier
            + "|" + AnsiLineComment
            + "|" + CComment
            + "|" + SimpleSqlPart;

        // Matches on a single question mark, or a sequence of sql parts (merged into one string)
        internal static readonly Regex QuestionableSql = new Regex(
            "(?:" + SqlPart + ")+|\\?", RegexOptions.Compiled | RegexOptions.Singleline
        );

        public static string ToNamedParameters(this string query)
        {
            // Break apart the query, replace all parameters (isolated as single ? strings),
            // and rejoin them all to reform the string using named parameters

            var matches = QuestionableSql.Matches(query);
            var replacedSql = new StringBuilder();
            int sqlLength = 0;
            int paramIndex = 0;
            foreach (Match m in matches)
            {
                string val = m.Value;
                sqlLength += val.Length;
                if (val == "?")
                {
                    replacedSql.Append("@p").Append(paramIndex);
                    paramIndex++;
                }
                else
                {
                    replacedSql.Append(val);
                }
            }

            // Verify query was separated correctly
            // This should only trigger on unbalanced inputs, eg "select 'abc from foo"
            if (sqlLength != query.Length)
            {
                throw new ArgumentException($"Input query did not split cleanly: {query}");
            }
            return replacedSql.ToString();
        }
    }
}
