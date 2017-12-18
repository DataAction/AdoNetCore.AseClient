#if NETCOREAPP2_0 || NET45
using System;
using System.Data;
using System.Data.Common;
using System.Security.Permissions;
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Global

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Code Access Security is no longer recommended by Microsoft and the .NET Community. 
    /// 
    /// In line with approach taken by the .NET Core community for the <b>System.Data.SqlClient.SqlClientPermission</b>, this 
    /// stub is provided to help clients that depend on this type to compile. However it does not provide any implementation and is not a 
    /// security mechanism.
    /// 
    /// </summary>
    [Serializable]
    public sealed class AseClientPermission : DBDataPermission
    {
        public AseClientPermission() : base(default(PermissionState)) { }
        public AseClientPermission(PermissionState state) : base(default(PermissionState)) { }
        public AseClientPermission(PermissionState state, bool allowBlankPassword) : base(default(PermissionState)) { }
        public override void Add(string connectionString, string restrictions, KeyRestrictionBehavior behavior) { }
        public override System.Security.IPermission Copy() { return null; }
    }
}
#endif
