namespace AdoNetCore.AseClient.Internal
{
    internal sealed class ConnectionStringItem
    {
        public string PropertyName { get; private set; }
        public string PropertyValue { get; private set; }

        public ConnectionStringItem(string propertyName, string propertyValue)
        {
            PropertyName = propertyName;
            PropertyValue = propertyValue;
        }
    }
}