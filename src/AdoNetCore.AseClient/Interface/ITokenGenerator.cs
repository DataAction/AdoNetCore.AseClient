using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Interface
{
    /// <summary>
    /// Generate requested tokens, ready to be written to the stream (?or maybe attached to a packet which itself will be written to the stream?)
    /// </summary>
    public interface ITokenGenerator
    {
        BaseToken GetCapabilityToken();
    }
}
