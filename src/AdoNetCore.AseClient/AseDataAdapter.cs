#if !NETCORE_OLD
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace AdoNetCore.AseClient
{

    /// <summary>
    /// This types helps commit INSERT/UPDATE/DELETE operations from a <see cref="DataSet"/> or <see cref="DataTable"/> to the databae.
    /// </summary>
    public sealed class AseDataAdapter : DbDataAdapter, IDbDataAdapter
    {
        /// <summary>
        /// The commands that will be batached up for execution.
        /// </summary>
        private readonly List<AseCommand> _cmdList = new List<AseCommand>();

        /// <summary>
        /// Whether or not this is disposed.
        /// </summary>
        private bool _isDisposed;

        /// <summary>
        /// The DELETE command template.
        /// </summary>
        private AseCommand _deleteCmd;

        /// <summary>
        /// The INSERT command template.
        /// </summary>
        private AseCommand _insertCmd;

        /// <summary>
        /// The SELECT command.
        /// </summary>
        private AseCommand _selectCmd;

        /// <summary>
        /// The UPDATE command template.
        /// </summary>
        private AseCommand _updateCmd;

        /// <summary>
        /// An optional <see cref="AseCommandBuilder"/> for use when generating the <see cref="DeleteCommand"/>, <see cref="InsertCommand"/>, and <see cref="UpdateCommand"/> respectively.
        /// </summary>
        private AseCommandBuilder _builder;

        /// <summary>
        /// Constructor function for an <see cref="AseDataAdapter"/> instance.
        /// </summary>
        public AseDataAdapter()
        {
        }

        /// <summary>
        /// Constructor function for an <see cref="AseDataAdapter"/> instance.
        /// </summary>
        /// <param name="selectCommand">The SELECT command to initiaise with.</param>
        // ReSharper disable once UnusedMember.Global
        public AseDataAdapter(AseCommand selectCommand)
        {
            SelectCommand = selectCommand;
        }

        /// <summary>
        /// Constructor function for an <see cref="AseDataAdapter"/> instance.
        /// </summary>
        /// <param name="selectCommandText">The SELECT command to initiaise with.</param>
        /// <param name="selectConnection">The <see cref="AseConnection"/> to load data with.</param>
        public AseDataAdapter(string selectCommandText, AseConnection selectConnection)
        {
            SelectCommand = new AseCommand(selectConnection) { CommandText = selectCommandText };
        }

        /// <summary>
        /// Constructor function for an <see cref="AseDataAdapter"/> instance.
        /// </summary>
        /// <param name="selectCommandText">The SELECT command to initiaise with.</param>
        /// <param name="selectConnectionString">The connection to load data with.</param>
        // ReSharper disable once UnusedMember.Global
        public AseDataAdapter(string selectCommandText, string selectConnectionString)
        {
            SelectCommand = new AseCommand(new AseConnection(selectConnectionString)) { CommandText = selectCommandText };
        }

        /// <summary>
        /// Desctructor function for an <see cref="AseDataAdapter"/> instance.
        /// </summary>
        ~AseDataAdapter()
        {
            Dispose(false);
        }

        protected override void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }
            if (disposing)
            {
                _deleteCmd?.Dispose();
                _insertCmd?.Dispose();
                _selectCmd?.Dispose();
                _updateCmd?.Dispose();

                RowUpdated = null;
                RowUpdating = null;

                base.Dispose(true);
            }
            _isDisposed = true;
        }

        /// <summary>
        /// The DELETE command template.
        /// </summary>
        public new AseCommand DeleteCommand
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseDataAdapter));
                }

                return _deleteCmd ?? _builder?.GetDeleteCommand();
            }
            set
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseDataAdapter));
                }

                _deleteCmd = value;

                if (_deleteCmd == null || _deleteCmd.Connection != null || _selectCmd == null)
                {
                    return;
                }

                _deleteCmd.Connection = _selectCmd.Connection;
            }
        }

        /// <summary>
        /// The DELETE command template.
        /// </summary>
        IDbCommand IDbDataAdapter.DeleteCommand
        {
            get => DeleteCommand;
            set => DeleteCommand = (AseCommand) value;
        }

        /// <summary>
        /// The INSERT command template.
        /// </summary>
        public new AseCommand InsertCommand
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseDataAdapter));
                }

                return _insertCmd ?? _builder?.GetInsertCommand();
            }
            set
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseDataAdapter));
                }

                _insertCmd = value;

                if (_insertCmd == null || _insertCmd.Connection != null || _selectCmd == null)
                {
                    return;
                }

                _insertCmd.Connection = _selectCmd.Connection;
            }
        }

        /// <summary>
        /// The INSERT command template.
        /// </summary>
        IDbCommand IDbDataAdapter.InsertCommand
        {
            get => InsertCommand;
            set => InsertCommand = (AseCommand) value;
        }

        /// <summary>
        /// The SELECT command.
        /// </summary>
        public new AseCommand SelectCommand
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseDataAdapter));
                }

                return _selectCmd;
            }
            set
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseDataAdapter));
                }

                _selectCmd = value;
            }
        }

        /// <summary>
        /// The SELECT command.
        /// </summary>
        IDbCommand IDbDataAdapter.SelectCommand
        {
            get => SelectCommand;
            set => SelectCommand = (AseCommand) value;
        }

        /// <summary>
        /// The UPDATE command template.
        /// </summary>
        public new AseCommand UpdateCommand
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseDataAdapter));
                }

                return _updateCmd ?? _builder?.GetUpdateCommand();
            }
            set
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseDataAdapter));
                }

                _updateCmd = value;

                if (_updateCmd == null || _updateCmd.Connection != null || _selectCmd == null)
                {
                    return;
                }

                _updateCmd.Connection = _selectCmd.Connection;
            }
        }

        /// <summary>
        /// The UPDATE command template.
        /// </summary>
        IDbCommand IDbDataAdapter.UpdateCommand
        {
            get => UpdateCommand;
            set => UpdateCommand = (AseCommand) value;
        }

        /// <summary>
        /// An optional <see cref="AseCommandBuilder"/> for use when generating the <see cref="DeleteCommand"/>, <see cref="InsertCommand"/>, and <see cref="UpdateCommand"/> respectively.
        /// </summary>
        public AseCommandBuilder CommandBuilder
        {
            get
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseDataAdapter));
                }

                return _builder;
            }
            set
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException(nameof(AseDataAdapter));
                }

                _builder = value;

                if (_builder == null || _builder.DataAdapter == this)
                {
                    return;
                }

                _builder.DataAdapter = this;
            }
        }

        /// <summary>
        /// Gets or sets the number of rows that are processed in each round-trip to the server.
        /// </summary>
        public override int UpdateBatchSize { get; set; } = 1;

        protected override int AddToBatch(IDbCommand command)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseDataAdapter));
            }

            var aseCommand = (AseCommand) ((ICloneable) command).Clone();

            _cmdList.Add(aseCommand);

            return _cmdList.Count - 1;
        }

        protected override void ClearBatch()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseDataAdapter));
            }

            _cmdList.Clear();
        }

        protected override int ExecuteBatch()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseDataAdapter));
            }
            
            // NOTE - naive implementation - this should be a single execution - not a loop.
            foreach (var command in _cmdList)
            {
                command.ExecuteNonQuery();
            }

            return _cmdList.Count;
        }

        protected override IDataParameter GetBatchedParameter(int commandIdentifier, int parameterIndex)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseDataAdapter));
            }

            return _cmdList[commandIdentifier].Parameters[parameterIndex];
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
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseDataAdapter));
            }

            return new AseRowUpdatedEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override RowUpdatingEventArgs CreateRowUpdatingEvent(DataRow dataRow, IDbCommand command,
            StatementType statementType, DataTableMapping tableMapping)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseDataAdapter));
            }

            return new AseRowUpdatingEventArgs(dataRow, command, statementType, tableMapping);
        }

        protected override void OnRowUpdated(RowUpdatedEventArgs value)
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseDataAdapter));
            }

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
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(AseDataAdapter));
            }

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

        // ReSharper disable once EventNeverSubscribedTo.Global
        public event AseRowUpdatedEventHandler RowUpdated;

        public event AseRowUpdatingEventHandler RowUpdating;
    }
}
#endif