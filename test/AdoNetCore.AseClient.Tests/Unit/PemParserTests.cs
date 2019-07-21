using System;
using System.Linq;
using AdoNetCore.AseClient.Internal;
using NUnit.Framework;

namespace AdoNetCore.AseClient.Tests.Unit
{
    public class PemParserTests
    {
        [Test]
        public void Parse_WithSampleFile_ReturnsCertificates()
        {
            var fileData = string.Join(Environment.NewLine,
                VeriSignClass1,
                VeriSignClass3PublicPrimaryCA_G2,
                VeriSignClass3PublicPrimaryCA_G2,
                VeriSignClass3PrimaryCA_G5,
                ThawtePrimaryRootCA,
                ThawtePremiumServerCA,
                ThawteServerCA);

            var parser = new PemParser();

            var certificates = parser.ParseCertificates(fileData).ToList();

            Assert.AreEqual(7, certificates.Count);
        }

        [TestCase(VeriSignClass1, "JchPXGWtgVzzVtFQzzNQMg==", TestName = "VeriSignClass1")]
        [TestCase(VeriSignClass3PublicPrimaryCA_G2, "xjSJp/tneRC3HqjPB/7ZfQ==", TestName = "VeriSignClass3PublicPrimaryCA_G2")]
        [TestCase(VeriSignClass3PublicPrimary_CA, "vhK/RNC4mg4b0PYfyzGRPA==", TestName = "VeriSignClass3PublicPrimary_CA")]
        [TestCase(VeriSignClass3PrimaryCA_G5, "SjtrzM1YIUq76H0mntHaGA==", TestName = "VeriSignClass3PrimaryCA_G5")]
        [TestCase(ThawtePrimaryRootCA, "bSvbN84v9Ens7dUgV9VONA==", TestName = "ThawtePrimaryRootCA")]
        [TestCase(ThawtePremiumServerCA, "VAnXTF/SoSClOOPFliISNg==", TestName = "ThawtePremiumServerCA")]
        [TestCase(ThawteServerCA, "dWaUoUIXMzylTK8w9v+kNA==", TestName = "ThawteServerCA")]
        public void ParseCertificate_WithSampleCertificate_ReturnsCertificates(string certificate, string serialNumber)
        {
            var parser = new PemParser();

            var result = parser.ParseCertificate(certificate);

            Assert.NotNull(result);
            Assert.AreEqual(serialNumber, Convert.ToBase64String(result.GetSerialNumber()));
        }

        private const string VeriSignClass1 =
@"The file contains root certificates for Certificate Authority
(CA) from leading certificate vendors.
The file will be updated from time to time, including adding
root certificates from other vendors.
Created: 29 December 2016 

VeriSign Class 1 - exp. Jan 7, 2020
-----BEGIN CERTIFICATE-----
MIICPDCCAaUCEDJQM89Q0VbzXIGtZVxPyCUwDQYJKoZIhvcNAQECBQAwXzELMAkG
A1UEBhMCVVMxFzAVBgNVBAoTDlZlcmlTaWduLCBJbmMuMTcwNQYDVQQLEy5DbGFz
cyAxIFB1YmxpYyBQcmltYXJ5IENlcnRpZmljYXRpb24gQXV0aG9yaXR5MB4XDTk2
MDEyOTAwMDAwMFoXDTIwMDEwNzIzNTk1OVowXzELMAkGA1UEBhMCVVMxFzAVBgNV
BAoTDlZlcmlTaWduLCBJbmMuMTcwNQYDVQQLEy5DbGFzcyAxIFB1YmxpYyBQcmlt
YXJ5IENlcnRpZmljYXRpb24gQXV0aG9yaXR5MIGfMA0GCSqGSIb3DQEBAQUAA4GN
ADCBiQKBgQDlGb9to1ZhLZlIcfZn3rmN67eehoAKkQ76OCWvRoiC5XOooJskXQ0f
zGVuDLDQVoQYh5oGmxChc9+0WDlrbsH2FdWoqD+qEgaNMax/sDTXjzRniAnNFBHi
TkVWaR94AoDa3EeRKbs2yWNcxeDXLYd7obcysHswuiovMaruo2fa2wIDAQABMA0G
CSqGSIb3DQEBAgUAA4GBAEtEZmBoZOSYG/OwcuaViXzde7OVwB0u2NgZ0C00PcZQ
mhCGjKo/O6gE/DdSlcPZydvN8oYGxLEb8IKIMEKOF1AcZHq4PplJdJf8rAJD+5YM
VgQlDHx8h50kp9jwMim1pN9dokzFFjKoQvZFprY2ueC/ZTaTwtLXa9zeWdaiNfhF
-----END CERTIFICATE-----
";

        private const string VeriSignClass3PublicPrimaryCA_G2 =
@"VeriSign Class 3 Public Primary CA - G2
Valid From: Sunday, May 17, 1998 4:00:00 PM
Valid to: Tuesday, August 01, 2028 3:59:59 PM
Key Size: RSA(1024 Bits)
Signature Algorithm: sha1RSA
File name in Root package: Class 3 Public Primary Certification Authority - G2

-----BEGIN CERTIFICATE-----
MIIDAjCCAmsCEH3Z/gfPqB63EHln+6eJNMYwDQYJKoZIhvcNAQEFBQAwgcExCzAJ
BgNVBAYTAlVTMRcwFQYDVQQKEw5WZXJpU2lnbiwgSW5jLjE8MDoGA1UECxMzQ2xh
c3MgMyBQdWJsaWMgUHJpbWFyeSBDZXJ0aWZpY2F0aW9uIEF1dGhvcml0eSAtIEcy
MTowOAYDVQQLEzEoYykgMTk5OCBWZXJpU2lnbiwgSW5jLiAtIEZvciBhdXRob3Jp
emVkIHVzZSBvbmx5MR8wHQYDVQQLExZWZXJpU2lnbiBUcnVzdCBOZXR3b3JrMB4X
DTk4MDUxODAwMDAwMFoXDTI4MDgwMTIzNTk1OVowgcExCzAJBgNVBAYTAlVTMRcw
FQYDVQQKEw5WZXJpU2lnbiwgSW5jLjE8MDoGA1UECxMzQ2xhc3MgMyBQdWJsaWMg
UHJpbWFyeSBDZXJ0aWZpY2F0aW9uIEF1dGhvcml0eSAtIEcyMTowOAYDVQQLEzEo
YykgMTk5OCBWZXJpU2lnbiwgSW5jLiAtIEZvciBhdXRob3JpemVkIHVzZSBvbmx5
MR8wHQYDVQQLExZWZXJpU2lnbiBUcnVzdCBOZXR3b3JrMIGfMA0GCSqGSIb3DQEB
AQUAA4GNADCBiQKBgQDMXtERXVxp0KvTuWpMmR9ZmDCOFoUgRm1HP9SFIIThbbP4
pO0M8RcPO/mn+SXXwc+EY/J8Y8+iR/LGWzOOZEAEaMGAuWQcRXfH2G71lSk8UOg0
13gfqLptQ5GVj0VXXn7F+8qkBOvqlzdUMG+7AUcyM83cV5tkaWH4mx0ciU9cZwID
AQABMA0GCSqGSIb3DQEBBQUAA4GBAFFNzb5cy5gZnBWyATl4Lk0PZ3BwmcYQWpSk
U01UbSuvDV1Ai2TT1+7eVmGSX6bEHRBhNtMsJzzoKQm5EWR0zLVznxxIqbxhAe7i
F6YM40AIOw7n60RzKprxaZLvcRTDOaxxp5EJb+RxBrO6WVcmeQD2+A2iMzAo1KpY
oJ2daZH9
-----END CERTIFICATE-----
";

        private const string VeriSignClass3PublicPrimary_CA =
@"VeriSign Class 3 Public Primary CA
Valid From: Sunday, January 28, 1996 4:00:00 PM
Valid to: Wednesday, August 02, 2028 3:59:59 PM
Key Size: RSA(1024 Bits)
Signature Algorithm: sha1RSA
File name in Root package: Class 3 Public Primary Certification Authority
-----BEGIN CERTIFICATE-----
MIICPDCCAaUCEDyRMcsf9tAbDpq40ES/Er4wDQYJKoZIhvcNAQEFBQAwXzELMAkG
A1UEBhMCVVMxFzAVBgNVBAoTDlZlcmlTaWduLCBJbmMuMTcwNQYDVQQLEy5DbGFz
cyAzIFB1YmxpYyBQcmltYXJ5IENlcnRpZmljYXRpb24gQXV0aG9yaXR5MB4XDTk2
MDEyOTAwMDAwMFoXDTI4MDgwMjIzNTk1OVowXzELMAkGA1UEBhMCVVMxFzAVBgNV
BAoTDlZlcmlTaWduLCBJbmMuMTcwNQYDVQQLEy5DbGFzcyAzIFB1YmxpYyBQcmlt
YXJ5IENlcnRpZmljYXRpb24gQXV0aG9yaXR5MIGfMA0GCSqGSIb3DQEBAQUAA4GN
ADCBiQKBgQDJXFme8huKARS0EN8EQNvjV69qRUCPhAwL0TPZ2RHP7gJYHyX3KqhE
BarsAx94f56TuZoAqiN91qyFomNFx3InzPRMxnVx0jnvT0Lwdd8KkMaOIG+YD/is
I19wKTakyYbnsZogy1Olhec9vn2a/iRFM9x2Fe0PonFkTGUugWhFpwIDAQABMA0G
CSqGSIb3DQEBBQUAA4GBABByUqkFFBkyCEHwxWsKzH4PIRnN5GfcX6kb5sroc50i
2JhucwNhkcV8sEVAbkSdjbCxlnRhLQ2pRdKkkirWmnWXbj9T/UWZYB2oK0z5XqcJ
2HUw19JlYD1n1khVdWk/kfVIC0dpImmClr7JyDiGSnoscxlIaU5rfGW/D/xwzoiQ
-----END CERTIFICATE-----
";

        private const string VeriSignClass3PrimaryCA_G5 =
@"VeriSign Class 3 Primary CA - G5
Operational Period: Tue, November 07, 2006 to Wed, July 16, 2036 
Key Size: RSA(2048Bits)
Signature Algorithm: sha1RSA
-----BEGIN CERTIFICATE-----
MIIE0zCCA7ugAwIBAgIQGNrRniZ96LtKIVjNzGs7SjANBgkqhkiG9w0BAQUFADCB
yjELMAkGA1UEBhMCVVMxFzAVBgNVBAoTDlZlcmlTaWduLCBJbmMuMR8wHQYDVQQL
ExZWZXJpU2lnbiBUcnVzdCBOZXR3b3JrMTowOAYDVQQLEzEoYykgMjAwNiBWZXJp
U2lnbiwgSW5jLiAtIEZvciBhdXRob3JpemVkIHVzZSBvbmx5MUUwQwYDVQQDEzxW
ZXJpU2lnbiBDbGFzcyAzIFB1YmxpYyBQcmltYXJ5IENlcnRpZmljYXRpb24gQXV0
aG9yaXR5IC0gRzUwHhcNMDYxMTA4MDAwMDAwWhcNMzYwNzE2MjM1OTU5WjCByjEL
MAkGA1UEBhMCVVMxFzAVBgNVBAoTDlZlcmlTaWduLCBJbmMuMR8wHQYDVQQLExZW
ZXJpU2lnbiBUcnVzdCBOZXR3b3JrMTowOAYDVQQLEzEoYykgMjAwNiBWZXJpU2ln
biwgSW5jLiAtIEZvciBhdXRob3JpemVkIHVzZSBvbmx5MUUwQwYDVQQDEzxWZXJp
U2lnbiBDbGFzcyAzIFB1YmxpYyBQcmltYXJ5IENlcnRpZmljYXRpb24gQXV0aG9y
aXR5IC0gRzUwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCvJAgIKXo1
nmAMqudLO07cfLw8RRy7K+D+KQL5VwijZIUVJ/XxrcgxiV0i6CqqpkKzj/i5Vbex
t0uz/o9+B1fs70PbZmIVYc9gDaTY3vjgw2IIPVQT60nKWVSFJuUrjxuf6/WhkcIz
SdhDY2pSS9KP6HBRTdGJaXvHcPaz3BJ023tdS1bTlr8Vd6Gw9KIl8q8ckmcY5fQG
BO+QueQA5N06tRn/Arr0PO7gi+s3i+z016zy9vA9r911kTMZHRxAy3QkGSGT2RT+
rCpSx4/VBEnkjWNHiDxpg8v+R70rfk/Fla4OndTRQ8Bnc+MUCH7lP59zuDMKz10/
NIeWiu5T6CUVAgMBAAGjgbIwga8wDwYDVR0TAQH/BAUwAwEB/zAOBgNVHQ8BAf8E
BAMCAQYwbQYIKwYBBQUHAQwEYTBfoV2gWzBZMFcwVRYJaW1hZ2UvZ2lmMCEwHzAH
BgUrDgMCGgQUj+XTGoasjY5rw8+AatRIGCx7GS4wJRYjaHR0cDovL2xvZ28udmVy
aXNpZ24uY29tL3ZzbG9nby5naWYwHQYDVR0OBBYEFH/TZafC3ey78DAJ80M5+gKv
MzEzMA0GCSqGSIb3DQEBBQUAA4IBAQCTJEowX2LP2BqYLz3q3JktvXf2pXkiOOzE
p6B4Eq1iDkVwZMXnl2YtmAl+X6/WzChl8gGqCBpH3vn5fJJaCGkgDdk+bW48DW7Y
5gaRQBi5+MHt39tBquCWIMnNZBU4gcmU7qKEKQsTb47bDN0lAtukixlE0kF6BWlK
WE9gyn6CagsCqiUXObXbf+eEZSqVir2G3l6BFoMtEMze/aiCKm0oHw0LxOXnGiYZ
4fQRbxC1lfznQgUy286dUV4otp6F01vvpX1FQHKOtw5rDgb7MzVIcbidJ4vEZV8N
hnacRHr2lVz2XTIIM6RUthg/aFzyQkqFOFSDX9HoLPKsEdao7WNq
-----END CERTIFICATE-----
";


        private const string ThawtePrimaryRootCA =
@"Thawte Primary Root CA
Country = US
Organization = thawte, Inc.
Organizational Unit = Certification Services Division
Organizational Unit = (c) 2006 thawte, Inc. - For authorized use only
Common Name = thawte Primary Root CA
Validity Start: November 17, 2006 
Validity End: July 16, 2036 
Key Size: RSA (2048 bits)
Signature Algorithm: SHA1 With RSA
Serial Number: 34 4e d5 57 20 d5 ed ec 49 f4 2f ce 37 db 2b 6d
Certificate SHA1 fingerprint: 91 c6 d6 ee 3e 8a c8 63 84 e5 48 c2 99 29 5c 75 6c 81 7b 81

-----BEGIN CERTIFICATE-----
MIIEIDCCAwigAwIBAgIQNE7VVyDV7exJ9C/ON9srbTANBgkqhkiG9w0BAQUFADCB
qTELMAkGA1UEBhMCVVMxFTATBgNVBAoTDHRoYXd0ZSwgSW5jLjEoMCYGA1UECxMf
Q2VydGlmaWNhdGlvbiBTZXJ2aWNlcyBEaXZpc2lvbjE4MDYGA1UECxMvKGMpIDIw
MDYgdGhhd3RlLCBJbmMuIC0gRm9yIGF1dGhvcml6ZWQgdXNlIG9ubHkxHzAdBgNV
BAMTFnRoYXd0ZSBQcmltYXJ5IFJvb3QgQ0EwHhcNMDYxMTE3MDAwMDAwWhcNMzYw
NzE2MjM1OTU5WjCBqTELMAkGA1UEBhMCVVMxFTATBgNVBAoTDHRoYXd0ZSwgSW5j
LjEoMCYGA1UECxMfQ2VydGlmaWNhdGlvbiBTZXJ2aWNlcyBEaXZpc2lvbjE4MDYG
A1UECxMvKGMpIDIwMDYgdGhhd3RlLCBJbmMuIC0gRm9yIGF1dGhvcml6ZWQgdXNl
IG9ubHkxHzAdBgNVBAMTFnRoYXd0ZSBQcmltYXJ5IFJvb3QgQ0EwggEiMA0GCSqG
SIb3DQEBAQUAA4IBDwAwggEKAoIBAQCsoPD7gFnUnMekz52hWXMJEEUMDSxuaPFs
W0hoSVk3/AszGcJ3f8wQLZU0HObrTQmnHNK4yZc2AreJ1CRfBsDMRJSUjQJib+ta
3RGNKJpchJAQeg29dGYvajig4tVUROsdB58Hum/u6f1OCyn1PoSgAfGcq/gcfomk
6KHYcWUNo1F77rzSImANuVud37r8UVsLr5iy6S7pBOhih94ryNdOwUxkHt3Ph1i6
Sk/KaAcdHJ1KxtUvkcx8cXIcxcBn6zL9yZJclNqFwJu/U30rCfSMnZEfl2pSy94J
NqR32HuHUETVPm4pafs5SSYeCaWAe0At6+gnhcn+Yf1+5nyXHdWdAgMBAAGjQjBA
MA8GA1UdEwEB/wQFMAMBAf8wDgYDVR0PAQH/BAQDAgEGMB0GA1UdDgQWBBR7W0XP
r87Lev0xkhpqtvNG61dIUDANBgkqhkiG9w0BAQUFAAOCAQEAeRHAS7ORtvzw6WfU
DW5FvlXok9LOAz/t2iWwHVfLHjp2oEzsUHboZHIMpKnxuIvW1oeEuzLlQRHAd9mz
YJ3rG9XRbkREqaYB7FViHXe4XI5ISXycO1cRrK1zN44veFyQaEfZYGDm/Ac9IiAX
xPcW6cTYcvnIc3zfFi8VqT79aie2oetaupgf1eNNZAqdE8hhuvU5HIe6uL17In/2
/qxAeeWsEG89jxt5dovEN7MhGITlNgDrYyCZuen+MwS7QcjBAvlEYyCegc5C09Y/
LHbTY5xZ3Y+m4Q6gLkH3LpVHz7z9M/P2C2F+fpErgUfCJzDupxBdN49cOSvkBPB7
jVaMaA==
-----END CERTIFICATE-----
";

        private const string ThawtePremiumServerCA =
@"Thawte Premium Server CA
Country = ZA
Organization = Thawte Consulting cc
Organizational Unit = Class 3 Public Primary Certification Authority - G2
Organizational Unit = Certification Services Division
Common name = Thawte Premium Server CA
Serial Number: 36 12 22 96 c5 e3 38 a5 20 a1 d2 5f 4c d7 09 54
Valid From: Wednesday, July 31, 1996 
Valid to: Friday, January 01, 2021 
Certificate SHA1 Fingerprint: e0 ab 05 94 20 72 54 93 05 60 62 02 36 70 f7 cd 2e fc 66 66
Key Size: RSA(1024 Bits)
Signature Algorithm: sha1RSA
-----BEGIN CERTIFICATE-----
MIIDNjCCAp+gAwIBAgIQNhIilsXjOKUgodJfTNcJVDANBgkqhkiG9w0BAQUFADCB
zjELMAkGA1UEBhMCWkExFTATBgNVBAgTDFdlc3Rlcm4gQ2FwZTESMBAGA1UEBxMJ
Q2FwZSBUb3duMR0wGwYDVQQKExRUaGF3dGUgQ29uc3VsdGluZyBjYzEoMCYGA1UE
CxMfQ2VydGlmaWNhdGlvbiBTZXJ2aWNlcyBEaXZpc2lvbjEhMB8GA1UEAxMYVGhh
d3RlIFByZW1pdW0gU2VydmVyIENBMSgwJgYJKoZIhvcNAQkBFhlwcmVtaXVtLXNl
cnZlckB0aGF3dGUuY29tMB4XDTk2MDgwMTAwMDAwMFoXDTIxMDEwMTIzNTk1OVow
gc4xCzAJBgNVBAYTAlpBMRUwEwYDVQQIEwxXZXN0ZXJuIENhcGUxEjAQBgNVBAcT
CUNhcGUgVG93bjEdMBsGA1UEChMUVGhhd3RlIENvbnN1bHRpbmcgY2MxKDAmBgNV
BAsTH0NlcnRpZmljYXRpb24gU2VydmljZXMgRGl2aXNpb24xITAfBgNVBAMTGFRo
YXd0ZSBQcmVtaXVtIFNlcnZlciBDQTEoMCYGCSqGSIb3DQEJARYZcHJlbWl1bS1z
ZXJ2ZXJAdGhhd3RlLmNvbTCBnzANBgkqhkiG9w0BAQEFAAOBjQAwgYkCgYEA0jY2
aovXwlue2oFBYo847kkEVdbQ7xwblRZH7xhINTpS9CtqBo87L+pW46+GjZ4X9560
ZXUCTe/LCaIhUdib0GfQug2SBhRz1JPLlyoAnFxODLz6FVL88kRu2hFKbgifLy3j
+ao6hnO2RlNYyIkFvYMRuHM/qgeN9EJN50CdHDcCAwEAAaMTMBEwDwYDVR0TAQH/
BAUwAwEB/zANBgkqhkiG9w0BAQUFAAOBgQBlkKyID1bZ5jA01CbH0FDxkt5r1DmI
CSLGpmODA/eZd9iy5Ri4XWPz1HP7bJyZePFLeH0ZJMMrAoT4vCLZiiLXoPxx7JGH
IPG47LHlVYCsPVLIOQ7C8MAFT9aCdYy9X9LcdpoFEsmvcsPcJX6kTY4XpeCHf+Ga
WuFg3GQjPEIuTQ==
-----END CERTIFICATE-----

";

        private const string ThawteServerCA =
@"Thawte Server CA
Country = ZA
Organization = Thawte Consulting cc
Organizational Unit = Class 3 Public Primary Certification Authority - G2
Organizational Unit = Certification Services Division
Common name = Thawte Server CA
Serial Number: 34 a4 ff f6 30 af 4c a5 3c 33 17 42 a1 94 66 75
Valid From: Wednesday, July 31, 1996
Valid to: Thursday, December 31, 2020
Certificate SHA1 Thumbprint: 9f ad 91 a6 ce 6a c6 c5 00 47 c4 4e c9 d4 a5 0d 92 d8 49 79
Key Size: RSA(1024 Bits)
Signature Algorithm: SHA1	

-----BEGIN CERTIFICATE-----
MIIDIjCCAougAwIBAgIQNKT/9jCvTKU8MxdCoZRmdTANBgkqhkiG9w0BAQUFADCB
xDELMAkGA1UEBhMCWkExFTATBgNVBAgTDFdlc3Rlcm4gQ2FwZTESMBAGA1UEBxMJ
Q2FwZSBUb3duMR0wGwYDVQQKExRUaGF3dGUgQ29uc3VsdGluZyBjYzEoMCYGA1UE
CxMfQ2VydGlmaWNhdGlvbiBTZXJ2aWNlcyBEaXZpc2lvbjEZMBcGA1UEAxMQVGhh
d3RlIFNlcnZlciBDQTEmMCQGCSqGSIb3DQEJARYXc2VydmVyLWNlcnRzQHRoYXd0
ZS5jb20wHhcNOTYwODAxMDAwMDAwWhcNMjEwMTAxMjM1OTU5WjCBxDELMAkGA1UE
BhMCWkExFTATBgNVBAgTDFdlc3Rlcm4gQ2FwZTESMBAGA1UEBxMJQ2FwZSBUb3du
MR0wGwYDVQQKExRUaGF3dGUgQ29uc3VsdGluZyBjYzEoMCYGA1UECxMfQ2VydGlm
aWNhdGlvbiBTZXJ2aWNlcyBEaXZpc2lvbjEZMBcGA1UEAxMQVGhhd3RlIFNlcnZl
ciBDQTEmMCQGCSqGSIb3DQEJARYXc2VydmVyLWNlcnRzQHRoYXd0ZS5jb20wgZ8w
DQYJKoZIhvcNAQEBBQADgY0AMIGJAoGBANOkUG7I/1Zr5s9dtuoMaHVHoqrC2oQl
/Kj0R1HahbUgdJSGHg91yekIYfUGbTBuFRkC6VLAYttNmZ7iagxEOM3+vuNkCXDF
/rFrKbYvScg71CcEJRCXL+eQbcAoQpnXTEPew/UhbVSfXcNY4cDk2VuwuNy0e982
OsK1ZiIS1ocNAgMBAAGjEzARMA8GA1UdEwEB/wQFMAMBAf8wDQYJKoZIhvcNAQEF
BQADgYEAvkBpQW/G28GnvwfAReTQtUMeTJUzNelewj4o9qgNUNX/4gwP/FACjq6R
ua00io2fJ3GqGcxL6ATK1BdrEhrWxl/WzV7/iXa/2EjYWb0IiokdV81FHlK6EpqE
+hiJX+j5MDVqAWC5mYCDhQpu2vTJj15zLTFKY6B08h+LItIpPus=
-----END CERTIFICATE-----
";

    }
}
