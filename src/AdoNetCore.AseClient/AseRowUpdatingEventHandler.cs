namespace AdoNetCore.AseClient
{
    /// <summary>
    /// Delegate definition for when a row changes.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The change.</param>
    public delegate void AseRowUpdatingEventHandler(object sender, AseRowUpdatingEventArgs e);
}
