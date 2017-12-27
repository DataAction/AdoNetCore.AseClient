#if NETCOREAPP2_0 || NET45 || NET46
namespace AdoNetCore.AseClient
{
    public delegate void AseRowUpdatedEventHandler(object sender, AseRowUpdatedEventArgs e);
}
#endif