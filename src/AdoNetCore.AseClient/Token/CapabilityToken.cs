using AdoNetCore.AseClient.Enum;

namespace AdoNetCore.AseClient.Token
{
    public class CapabilityToken : BaseToken
    {
        public override TokenType Type => TokenType.TDS_CAPABILITY;
    }
}
