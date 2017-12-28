#if !NETCORE_OLD
using System;
using System.Collections;
using System.Data;
using System.Data.Common;

namespace AdoNetCore.AseClient
{
    // TODO - merge the two adapters.
    // TODO - tests.

    public sealed class AseDataAdapter : DbDataAdapter, IDbDataAdapter
    {
        private readonly Hashtable _cmdList = new Hashtable();
        private readonly AseDataAdapter2 _oldAdapter;

        public AseDataAdapter()
        {
            _oldAdapter = new AseDataAdapter2(this);
            _oldAdapter.RowUpdated += oldAdapter_RowUpdated;
            _oldAdapter.RowUpdating += oldAdapter_RowUpdating;
        }

        public AseDataAdapter(AseCommand selectCommand)
        {
            _oldAdapter = new AseDataAdapter2(this, selectCommand);
            _oldAdapter.RowUpdated += oldAdapter_RowUpdated;
            _oldAdapter.RowUpdating += oldAdapter_RowUpdating;
        }

        public AseDataAdapter(string selectCommandText, AseConnection selectConnection)
        {
            _oldAdapter = new AseDataAdapter2(this, selectCommandText, selectConnection);
            _oldAdapter.RowUpdated += oldAdapter_RowUpdated;
            _oldAdapter.RowUpdating += oldAdapter_RowUpdating;
        }

        public AseDataAdapter(string selectCommandText, string selectConnectionString)
        {
            _oldAdapter = new AseDataAdapter2(this, selectCommandText, selectConnectionString);
            _oldAdapter.RowUpdated += oldAdapter_RowUpdated;
            _oldAdapter.RowUpdating += oldAdapter_RowUpdating;
        }

        private void oldAdapter_RowUpdated(object obj, AseRowUpdatedEventArgs args)
        {
            OnRowUpdated(args);
        }

        private void oldAdapter_RowUpdating(object obj, AseRowUpdatingEventArgs args)
        {
            OnRowUpdating(args);
        }

        public new AseCommand DeleteCommand
        {
            get => _oldAdapter.DeleteCommand;
            set => _oldAdapter.DeleteCommand = value;
        }

        IDbCommand IDbDataAdapter.DeleteCommand
        {
            get => DeleteCommand;
            set => DeleteCommand = (AseCommand) value;
        }

        public new AseCommand InsertCommand
        {
            get => _oldAdapter.InsertCommand;
            set => _oldAdapter.InsertCommand = value;
        }

        IDbCommand IDbDataAdapter.InsertCommand
        {
            get => InsertCommand;
            set => InsertCommand = (AseCommand) value;
        }

        public new AseCommand SelectCommand
        {
            get => _oldAdapter.SelectCommand;
            set => _oldAdapter.SelectCommand = value;
        }

        IDbCommand IDbDataAdapter.SelectCommand
        {
            get => SelectCommand;
            set => SelectCommand = (AseCommand) value;
        }

        public new AseCommand UpdateCommand
        {
            get => _oldAdapter.UpdateCommand;
            set => _oldAdapter.UpdateCommand = value;
        }

        IDbCommand IDbDataAdapter.UpdateCommand
        {
            get => UpdateCommand;
            set => UpdateCommand = (AseCommand) value;
        }

        public AseCommandBuilder CommandBuilder
        {
            get => _oldAdapter.CommandBuilder;
            set => _oldAdapter.CommandBuilder = value;
        }

        public override int UpdateBatchSize { get; set; } = 1;

        protected override int AddToBatch(IDbCommand command)
        {
            int count = _cmdList.Count;

            var aseCommand = (AseCommand) ((ICloneable) command).Clone();

            _cmdList.Add(count, aseCommand);

            return count;
        }

        protected override void ClearBatch()
        {
            _cmdList.Clear();
        }

        protected override int ExecuteBatch()
        {
            var values = _cmdList.Values;
            var num = 0;

            foreach (DbCommand dbCommand in values)
            {
                dbCommand.ExecuteNonQuery();
                ++num;
            }

            return num;
        }

        protected override IDataParameter GetBatchedParameter(int commandIdentifier, int parameterIndex)
        {
            return ((AseCommand) _cmdList[commandIdentifier]).Parameters[parameterIndex];
        }

        protected override void InitializeBatching()
        {
        }

        protected override void TerminateBatching()
        {
        }

        protected override RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command,
            StatementType statementType, DataTableMapping tableMapping)
        {
            return new AseRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command,
            StatementType statementType, DataTableMapping tableMapping)
        {
            return new AseRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
            if (RowUpdated == null)
            {
                return;
            }

            if (!(value is AseRowUpdatedEventArgs e))
            {
                return;
            }

            RowUpdated(this, e);
        }

        protected override void OnRowUpdating(RowUpdatingEventArgs value)
        {
            if (RowUpdating == null)
            {
                return;
            }

            if (!(value is AseRowUpdatingEventArgs e))
            {
                return;
            }

            RowUpdating(this, e);
        }

        public event AseRowUpdatedEventHandler RowUpdated;

        public event AseRowUpdatingEventHandler RowUpdating;
    }

    internal sealed class AseDataAdapter2
    {
        private readonly AseDataAdapter _thisAdapter;
        private bool _disposed;
        private AseCommand _deleteCmd;
        private AseCommand _insertCmd;
        private AseCommand _selectCmd;
        private AseCommand _updateCmd;
        private AseCommandBuilder _builder;

        internal AseDataAdapter2(AseDataAdapter realAdapter)
        {
            _thisAdapter = realAdapter;
        }

        internal AseDataAdapter2(AseDataAdapter realAdapter, AseCommand selectCommand)
        {
            SelectCommand = selectCommand;
            _thisAdapter = realAdapter;
        }

        internal AseDataAdapter2(AseDataAdapter realAdapter, string selectCommandText, AseConnection selectConnection)
        {
            SelectCommand = new AseCommand(selectConnection) {CommandText = selectCommandText};
            _thisAdapter = realAdapter;
        }

        internal AseDataAdapter2(AseDataAdapter realAdapter, string selectCommandText, string selectConnectionString)
        {
            SelectCommand = new AseCommand(new AseConnection(selectConnectionString)) {CommandText = selectCommandText};
            _thisAdapter = realAdapter;
        }

        ~AseDataAdapter2()
        {
            Dispose(false);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }
            if (disposing)
            {
                _deleteCmd?.Dispose();
                _insertCmd?.Dispose();
                _selectCmd?.Dispose();
                _updateCmd?.Dispose();
            }
            _disposed = true;
        }

        internal AseCommand DeleteCommand
        {
            get => _deleteCmd ?? _builder?.GetDeleteCommand();
            set
            {
                _deleteCmd = value;
                if (_deleteCmd == null || _deleteCmd.Connection != null || _selectCmd == null)
                    return;
                _deleteCmd.Connection = _selectCmd.Connection;
            }
        }

        internal AseCommand InsertCommand
        {
            get => _insertCmd ?? _builder?.GetInsertCommand();
            set
            {
                _insertCmd = value;
                if (_insertCmd == null || _insertCmd.Connection != null || _selectCmd == null)
                {
                    return;
                }

                _insertCmd.Connection = _selectCmd.Connection;
            }
        }

        internal AseCommand SelectCommand
        {
            get => _selectCmd;
            set => _selectCmd = value;
        }

        internal AseCommand UpdateCommand
        {
            get
            {
                AseCommand aseCommand = null;
                try
                {
                    if (_updateCmd != null)
                    {
                        aseCommand = _updateCmd;
                    }

                    if (_builder != null)
                    {
                        aseCommand = _builder.GetUpdateCommand();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                return aseCommand;
            }
            set
            {
                _updateCmd = value;
                if (_updateCmd == null || _updateCmd.Connection != null || _selectCmd == null)
                {
                    return;
                }

                _updateCmd.Connection = _selectCmd.Connection;
            }
        }

        internal AseCommandBuilder CommandBuilder
        {
            get => _builder;
            set
            {
                _builder = value;
                if (_builder == null || _builder.DataAdapter == _thisAdapter)
                {
                    return;
                }

                _builder.DataAdapter = _thisAdapter;
            }
        }

        internal RowUpdatedEventArgs CreateRowUpdatedEvent(DataRow dataRow, IDbCommand command,
            StatementType statementType, DataTableMapping tableMapping)
        {
            return new AseRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        internal RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command,
            StatementType statementType, DataTableMapping tableMapping)
        {
            return new AseRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        internal void OnRowUpdated(RowUpdatedEventArgs value)
        {
            if (RowUpdated == null)
            {
                return;
            }

            if (!(value is AseRowUpdatedEventArgs e))
            {
                return;
            }

            RowUpdated(_thisAdapter, e);
        }

        internal void OnRowUpdating(RowUpdatingEventArgs value)
        {
            if (RowUpdating == null)
            {
                return;
            }

            if (!(value is AseRowUpdatingEventArgs e))
            {
                return;
            }

            RowUpdating(_thisAdapter, e);
        }

        internal event AseRowUpdatedEventHandler RowUpdated;

        internal event AseRowUpdatingEventHandler RowUpdating;
    }
}
#endif