using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace AdoNetCore.AseClient.Internal
{
    internal sealed class PemParser
    {
        private static readonly Regex CertificateRegex = new Regex(
            "(?<certificate>(-----BEGIN CERTIFICATE-----)(.|\r\n|\r|\n)+?(-----END CERTIFICATE-----))",
            RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);

        public IEnumerable<X509Certificate2> ParseCertificates(string data)
        {
            var matches = CertificateRegex.Matches(data);

            foreach (Match match in matches)
            {
                var certificateMatchGroup = match.Groups["certificate"];
                if (certificateMatchGroup.Success)
                {
                    yield return ParseCertificate(certificateMatchGroup.Value);
                }
            }
        }
        public X509Certificate2 ParseCertificate(string data)
        {
            return new X509Certificate2(Encoding.ASCII.GetBytes(data));
        }
    }
}
