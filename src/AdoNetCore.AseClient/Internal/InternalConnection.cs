using System;
using System.Net.Sockets;
using AdoNetCore.AseClient.Interface;

namespace AdoNetCore.AseClient.Internal
{
    public class InternalConnection : IInternalConnection
    {
        private readonly ConnectionParameters _parameters;
        private readonly Socket _socket;
        private readonly ITokenGenerator _tokenGen;
        private readonly ITokenParser _tokenParse;
        private readonly ITokenWriter _tokenWrite;
        private readonly ITokenReader _tokenRead;

        public InternalConnection(ConnectionParameters parameters, Socket socket, ITokenGenerator tokenGen, ITokenParser tokenParse, ITokenWriter tokenWrite, ITokenReader tokenRead)
        {
            _parameters = parameters;
            _socket = socket;
            _tokenGen = tokenGen;
            _tokenParse = tokenParse;
            _tokenWrite = tokenWrite;
            _tokenRead = tokenRead;
        }

        public void Connect()
        {
            //todo: flesh out
            //socket is established already
            //login
            
            //send capabilities
            _tokenWrite.Write(_tokenGen.GetCapabilityToken());

            //parse response
        }

        public void ChangeDatabase(string databaseName)
        {
            if (string.Equals(databaseName, Database, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            Database = databaseName;

            //Send a USE X command
            throw new System.NotImplementedException();
        }

        public string Database { get; private set; }
    }
}
