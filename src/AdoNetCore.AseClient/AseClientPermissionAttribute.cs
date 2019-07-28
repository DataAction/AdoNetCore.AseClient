#if DB_DATAPERMISSION
using System;
using System.Data.Common;
using System.Security;
using System.Security.Permissions;
// ReSharper disable UnusedParameter.Local
// ReSharper disable UnusedMember.Global

namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Code Access Security is no longer recommended by Microsoft and the .NET Community. 
    /// 
    /// In line with approach taken by the .NET Core community for the <see cref="System.Data.SqlClient.SqlClientPermission" />, this 
    /// stub is provided to help clients that depend on this type to compile. However it does not provide any implementation and is not a 
    /// security mechanism.
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method,
        AllowMultiple = true, Inherited = false)]
    public sealed class AseClientPermissionAttribute : DBDataPermissionAttribute
    {
        public AseClientPermissionAttribute(SecurityAction action) : base(default(SecurityAction)) { }
        public override IPermission CreatePermission() { return null; }
    }
}
#endif