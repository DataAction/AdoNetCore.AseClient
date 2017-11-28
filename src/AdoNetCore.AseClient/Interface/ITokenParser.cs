using System.IO;
using System.Text;

namespace AdoNetCore.AseClient.Interface
{
    /// <summary>
    /// Parse out tokens from a bunch of bytes
    /// </summary>
    public interface ITokenParser
    {
        IToken[] Parse(Stream stream, Encoding enc);
    }
}
