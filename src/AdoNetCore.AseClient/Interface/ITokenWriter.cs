using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Interface
{
    public interface ITokenWriter
    {
        void Write(params BaseToken[] tokens);
    }
}
