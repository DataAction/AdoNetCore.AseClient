using System;
using AdoNetCore.AseClient.Enum;
using AdoNetCore.AseClient.Interface;
using AdoNetCore.AseClient.Token;

namespace AdoNetCore.AseClient.Internal.Handler
{
    internal class EnvChangeTokenHandler : ITokenHandler
    {
        private readonly DbEnvironment _environment;

        public EnvChangeTokenHandler(DbEnvironment environment)
        {
            _environment = environment;
        }

        public bool CanHandle(TokenType type)
        {
            return type == TokenType.TDS_ENVCHANGE;
        }

        public void Handle(IToken token)
        {
            switch (token)
            {
                case EnvironmentChangeToken t:
                    foreach (var change in t.Changes)
                    {
                        Logger.Instance?.WriteLine($"{t.Type}: {change.Type} - {change.OldValue} -> {change.NewValue}");
                        switch (change.Type)
                        {
                            case EnvironmentChangeToken.ChangeType.TDS_ENV_DB:
                                _environment.Database = change.NewValue;
                                break;
                            case EnvironmentChangeToken.ChangeType.TDS_ENV_PACKSIZE:
                                if (int.TryParse(change.NewValue, out int newPackSize))
                                {
                                    _environment.PacketSize = newPackSize;
                                }
                                break;
                        }
                    }
                    break;
                default:
                    return;
            }
        }
    }
}