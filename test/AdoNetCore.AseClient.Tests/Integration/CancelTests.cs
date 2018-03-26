using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using AdoNetCore.AseClient.Internal;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Integration
{
    [TestFixture]
    [Category("basic")]
    public class CancelTests
    {
        public CancelTests()
        {
            Logger.Enable(timestamps: true);
        }

        [Test]
        public void ExecuteNonQueryAsync_NoCancel_Succeeds()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "waitfor delay '00:00:01'";
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQueryAsync(new CancellationToken()).Wait();
                }
            }
        }

        [Test]
        public void ExecuteNonQueryAsync_AlreadyCanceled_Cancels()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "waitfor delay '00:00:10'";
                    command.CommandType = CommandType.Text;
                    var task = command.ExecuteNonQueryAsync(new CancellationToken(true));
                    var ex = Assert.Throws<AggregateException>(() => task.Wait());
                    Assert.IsTrue(ex.InnerException is TaskCanceledException);
                    Assert.IsTrue(task.IsCanceled);
                }
            }
        }

        [Test]
        public void ExecuteNonQueryAsync_DelayedCancel_Cancels()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "waitfor delay '00:00:10'";
                    command.CommandType = CommandType.Text;
                    var cts = new CancellationTokenSource(100);
                    var task = command.ExecuteNonQueryAsync(cts.Token);
                    var ex = Assert.Throws<AggregateException>(() => task.Wait());
                    Assert.IsTrue(ex.InnerException is TaskCanceledException);
                    Assert.IsTrue(task.IsCanceled);
                }
            }
        }

        [Test]
        public void ExecuteQuickNonQueryAsync_DelayedCancel_DoesNothing()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select 1";
                    command.CommandType = CommandType.Text;
                    var cts = new CancellationTokenSource(100);
                    var task = command.ExecuteNonQueryAsync(cts.Token);
                    task.Wait();
                    Assert.IsTrue(task.IsCompleted);
                }
            }
        }

        [Test]
        public void ExecuteScalarAsync_NoCancel_Succeeds()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "waitfor delay '00:00:01' select 1";
                    command.CommandType = CommandType.Text;
                    var task = command.ExecuteScalarAsync(new CancellationToken());
                    task.Wait();
                    Assert.AreEqual(1, task.Result);
                }
            }
        }

        [Test]
        public void ExecuteScalarAsync_AlreadyCanceled_Cancels()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "waitfor delay '00:00:10' select 1";
                    command.CommandType = CommandType.Text;
                    var task = command.ExecuteScalarAsync(new CancellationToken(true));
                    var ex = Assert.Throws<AggregateException>(() => task.Wait());
                    Assert.IsTrue(ex.InnerException is TaskCanceledException);
                    Assert.IsTrue(task.IsCanceled);
                }
            }
        }

        [Test]
        public void ExecuteScalarAsync_DelayedCancel_Cancels()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "waitfor delay '00:00:10' select 1";
                    command.CommandType = CommandType.Text;
                    var cts = new CancellationTokenSource(100);
                    var task = command.ExecuteScalarAsync(cts.Token);
                    var ex = Assert.Throws<AggregateException>(() => task.Wait());
                    Assert.IsTrue(ex.InnerException is TaskCanceledException);
                    Assert.IsTrue(task.IsCanceled);
                }
            }
        }

        [Test]
        public void ExecuteQuickScalarAsync_DelayedCancel_DoesNothing()
        {
            using (var connection = new AseConnection(ConnectionStrings.Default))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "select 1";
                    command.CommandType = CommandType.Text;
                    var cts = new CancellationTokenSource(100);
                    var task = command.ExecuteScalarAsync(cts.Token);
                    task.Wait();
                    Assert.IsTrue(task.IsCompleted);
                    Assert.AreEqual(1, task.Result);
                }
            }
        }
    }
}
