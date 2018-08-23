namespace AdoNetCore.AseClient.Internal.Handler
{
    using System;
    using AdoNetCore.AseClient.Enum;

    public class BackupServerErrorInfo
    {
        private BackupServerErrorInfo() { }

        public BackupServerErrorMajor Major { get; internal set; }
        public int Minor { get; internal set; }
        public BackupServerSeverity Severity { get; internal set; }
        public int State { get; internal set; }
        public string Message { get; internal set; }

        internal static bool TryParse(string message, out BackupServerErrorInfo info)
        {
            info = null;
            // Backup Server error messages are in this form: MMM DD YYY: Backup Server:N.N.N.N: Message Text
            // The four components of a Backup Server error message are major.minor.severity.state.
            //
            // new Regex("^Backup Server: ?(?<major>\\d+)\\.(?<minor>\\d+)\\.(?<severity>\\d+)\\.(?<state>\\d+): (?<message>.*)");
            //
            // Examples:
            //     "Backup Server: 4.172.1.4: The value of 'allocated pages threshold' has been set to 40%.",
            //     "Backup Server: 4.41.1.1: Creating new disk file /doesnotexist/foo.",
            //     "Backup Server: 4.141.2.40: [11] The 'open' call failed for database/archive device while working on stripe device '/doesnotexist/foo' with error number 2 (No such file or directory). Refer to your operating system documentation for further details.\0"

            if (!message.StartsWith("Backup Server:")) { return false; }
            const string marker = ":";
            int idxFirstColon = message.IndexOf(marker) + marker.Length;
            int idxSecondColon = message.IndexOf(marker, startIndex: idxFirstColon);
            string majMinSevStat = message.Substring(idxFirstColon, idxSecondColon - idxFirstColon).Trim();
            string backupServerMessage = message.Substring(idxSecondColon + marker.Length).Trim();

            var errorCodeSegments = majMinSevStat.Split(new[] { "." }, StringSplitOptions.None);
            if (errorCodeSegments.Length != 4) { return false; }
            if (!int.TryParse(errorCodeSegments[0], out int major)) { return false; }
            if (!int.TryParse(errorCodeSegments[1], out int minor)) { return false; }
            if (!int.TryParse(errorCodeSegments[2], out int severity)) { return false; }
            if (!int.TryParse(errorCodeSegments[3], out int state)) { return false; }

            info = new BackupServerErrorInfo
            {
                Major = (BackupServerErrorMajor)major,
                Minor = minor,
                Severity = (BackupServerSeverity)severity,
                State = state,
                Message = backupServerMessage
            };
            return true;
        }
    }
}