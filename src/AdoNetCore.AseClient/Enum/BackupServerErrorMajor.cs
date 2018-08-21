namespace AdoNetCore.AseClient.Enum
{
    public enum BackupServerErrorMajor
    {
        // http://infocenter.sybase.com/help/index.jsp?topic=/com.sybase.infocenter.dc31654.1600/doc/html/san1360629238802.html
        SystemError = 1,
        OpenServerEventError = 2,
        BackupServerRPCError = 3,
        IoServiceLayerError = 4,
        NetworkDataTransferError = 5,
        VolumeHandlingError = 6,
        OptionParsingError = 7
    }
}
