using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal
{
    public class TokenGenerator : ITokenGenerator
    {
        public BaseToken GetCapabilityToken()
        {
            return new CapabilityToken();
        }
    }
}
