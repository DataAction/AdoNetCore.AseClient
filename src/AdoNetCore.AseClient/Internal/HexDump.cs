using System;
using System.Text;

namespace AdoNetCore.AseClient.Internal
{
    internal class HexDump
    {
        public static string Dump(byte[] bytes, int offset = 0, int length = -1, int bytesPerLine = 16)
        {
            if (bytes == null) return "<null>";
            var bytesLength = length > -1 ? length : bytes.Length;

            var hexChars = "0123456789ABCDEF".ToCharArray();

            var firstHexColumn =
                  8                   // 8 characters for the address
                + 3;                  // 3 spaces

            var firstCharColumn = firstHexColumn
                + (bytesPerLine * 3)       // - 2 digit for the hexadecimal value and 1 space
                + ((bytesPerLine - 1) / 8) // - 1 extra space every 8 characters from the 9th
                + 2;                  // 2 spaces 

            var lineLength = firstCharColumn
                + bytesPerLine           // - characters to show the ascii value
                + Environment.NewLine.Length; // Carriage return and line feed (should normally be 2)

            var line = (new String(' ', lineLength - 2) + Environment.NewLine).ToCharArray();
            var expectedLines = (bytesLength + bytesPerLine - 1) / bytesPerLine;
            var result = new StringBuilder(expectedLines * lineLength);

            for (var i = offset; i < bytesLength; i += bytesPerLine)
            {
                line[0] = hexChars[(i >> 28) & 0xF];
                line[1] = hexChars[(i >> 24) & 0xF];
                line[2] = hexChars[(i >> 20) & 0xF];
                line[3] = hexChars[(i >> 16) & 0xF];
                line[4] = hexChars[(i >> 12) & 0xF];
                line[5] = hexChars[(i >> 8) & 0xF];
                line[6] = hexChars[(i >> 4) & 0xF];
                line[7] = hexChars[(i >> 0) & 0xF];

                var hexColumn = firstHexColumn;
                var charColumn = firstCharColumn;

                for (var j = 0; j < bytesPerLine; j++)
                {
                    if (j > 0 && (j & 7) == 0) hexColumn++;
                    if (i + j >= bytesLength)
                    {
                        line[hexColumn] = ' ';
                        line[hexColumn + 1] = ' ';
                        line[charColumn] = ' ';
                    }
                    else
                    {
                        var b = bytes[i + j];
                        line[hexColumn] = hexChars[(b >> 4) & 0xF];
                        line[hexColumn + 1] = hexChars[b & 0xF];
                        line[charColumn] = AsciiSymbol(b);
                    }
                    hexColumn += 3;
                    charColumn++;
                }
                result.Append(line);
            }
            return result.ToString();
        }

        static char AsciiSymbol(byte val)
        {
            if (val < 32) return '.';  // Non-printable ASCII
            if (val < 127) return (char)val;   // Normal ASCII
            // Handle the hole in Latin-1
            if (val == 127) return '.';
            if (val < 0x90) return "€.‚ƒ„…†‡ˆ‰Š‹Œ.Ž."[val & 0xF];
            if (val < 0xA0) return ".‘’“”•–—˜™š›œ.žŸ"[val & 0xF];
            if (val == 0xAD) return '.';   // Soft hyphen: this symbol is zero-width even in monospace fonts
            return (char)val;   // Normal Latin-1
        }
    }
}
