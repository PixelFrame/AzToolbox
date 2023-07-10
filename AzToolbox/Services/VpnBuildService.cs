using ProfileXMLBuilder.Lib;

namespace AzToolbox.Services
{
    public class VpnBuildService
    {
        public bool Win11Profile = false;
        public bool UserTunnel = true;
        public bool ForceTunnel = false;
        public bool DomainNRPT = true;
        public bool SimpleRoute = true;
        public bool CertSelectUseRadiusCA = true;
        public string VpnServer = "";
        public string DomainDnsSuffix = "";
        public string TrustedNetworkDetection = "";
        public string RadiusServerName = "";
        public string RootCAHash = "";
        public string CertSelectCAHash = "";
        public string DnsServers = "";
        public string DomainSubnets = "";
        public string DomainControllerAddresses = "";
        public bool Peap = false;
        public bool Tls = true;
        public string ProfileXml = "";
        public NativeProtocolType TunnelProtocol = NativeProtocolType.IKEv2;
        public string ProtocolListString = "";
        public int? RetryTimeInHours = null;

        public bool? RegisterDNS { get; set; } = true;
        public bool? BypassForLocal { get; set; } = null;
        public DataEncryptionLevel? DataEncryption { get; set; } = null;
        public bool? AlwaysOnActive { get; set; } = null;
        public bool? DisableAdvancedOptionsEditButton { get; set; } = null;
        public bool? DisableDisconnectButton { get; set; } = null;
        public bool? DisableIKEv2Fragmentation { get; set; } = null;
        public int? IPv4InterfaceMetric { get; set; } = null;
        public int? IPv6InterfaceMetric { get; set; } = null;
        public uint? NetworkOutageTime { get; set; } = null;
        public bool? PrivateNetwork { get; set; } = null;
        public bool? UseRasCredential { get; set; } = null;
        public bool? PlumbIKEv2TSAsRoutes { get; set; } = null;
        public bool DisableServerValidationPrompt { get; set; } = true;
        public bool? AllPurposeEnabled { get; set; } = null;

        public List<TrafficFilter> TrafficFilters = new();
        public List<DomainNameInformation> Nrpt = new();
        public List<Route> Routes = new();
        public List<AppTrigger> AppTriggers = new();
        public CryptographySuite? CryptographySuite = null;
        public DeviceCompliance? DeviceCompliance = null;
        public Proxy? Proxy = null;
        public List<Eku> EkuMapping = new();

        public bool NotIKEv2
        {
            get => TunnelProtocol != NativeProtocolType.IKEv2;
        }

        public void Build()
        {
            var builder = new Builder();
            builder.SetAlwaysOn(true)
                .SetDnsSuffix(DomainDnsSuffix)
                .SetServers(VpnServer)
                .SetTrustedNetworkDetection(TrustedNetworkDetection);

            if (UserTunnel)
            {
                AuthenticationMethod auth;
                if (Peap)
                {
                    if (Tls) auth = AuthenticationMethod.UserPeapTls;
                    else auth = AuthenticationMethod.UserPeapMschapv2;
                }
                else
                {
                    if (Tls) auth = AuthenticationMethod.UserEapTls;
                    else auth = AuthenticationMethod.UserEapMschapv2;
                }
                if (CertSelectUseRadiusCA)
                {
                    CertSelectCAHash = RootCAHash;
                }
                builder.SetDeviceTunnel(false)
                    .SetDisableClassBasedDefaultRoute(true)
                    .SetAuthentication(auth,
                        RadiusServerName,
                        new List<string>(RootCAHash.Split(',')),
                        DisableServerValidationPrompt,
                        new List<string>(CertSelectCAHash.Split(',')),
                        AllPurposeEnabled,
                        (EkuMapping.Count == 0) ? null : EkuMapping)
                    .AddDomainNameInformation(VpnServer, null, null, null, null)
                    .AddDomainNameInformation(Nrpt.ToArray())
                    .AddTrafficFilters(TrafficFilters.ToArray())
                    .SetProxy(Proxy);

                if (DomainNRPT)
                {
                    foreach (var suffix in DomainDnsSuffix.Split(','))
                    {
                        builder.AddDomainNameInformation(suffix.Trim(), DnsServers, null, null, null)
                            .AddDomainNameInformation($".{suffix.Trim()}", DnsServers, null, null, null);
                    }
                }

                if (TunnelProtocol == NativeProtocolType.ProtocolList)
                {
                    var pl = new HashSet<NativeProtocolListType>();
                    var protocols = ProtocolListString.Split(",");
                    foreach (var protocol in protocols)
                    {
                        var _ = Enum.TryParse<NativeProtocolListType>(protocol.Trim(), true, out var p);
                        if (_)
                        {
                            pl.Add(p);
                        }
                        else
                        {
                            throw new InvalidDataException($"{protocol} is not a valid native protocol");
                        }
                    }
                    builder.SetNativeProtocolList(pl.ToArray(), RetryTimeInHours);
                }
                else
                {
                    builder.SetNativeProtocolType(TunnelProtocol);
                }

                if (!ForceTunnel)
                {
                    builder.SetRoutingPolicyType(RoutingPolicyType.SplitTunnel)
                        .AddRoutes(Routes.ToArray());
                    if (SimpleRoute)
                    {
                        foreach (var subnet in DomainSubnets.Split(','))
                        {
                            var subnetParts = subnet.Split('/');
                            builder.AddRoute(subnetParts[0], byte.Parse(subnetParts[1]), null, null);
                        }
                    }
                }
                else
                {
                    builder.SetRoutingPolicyType(RoutingPolicyType.ForceTunnel);
                }

                if (TunnelProtocol == NativeProtocolType.IKEv2)
                {
                    builder.SetCryptographySuite(CryptographySuite);
                }
            }
            else
            {
                builder.SetDeviceTunnel(true)
                    .SetNativeProtocolType(NativeProtocolType.IKEv2)
                    .SetRoutingPolicyType(RoutingPolicyType.SplitTunnel)
                    .SetDisableClassBasedDefaultRoute(true)
                    .SetAuthentication(AuthenticationMethod.MachineCert, null, null, null, null, null, null)
                    .AddTrafficFilters(TrafficFilters.ToArray())
                    .AddRoutes(Routes.ToArray())
                    .SetCryptographySuite(CryptographySuite);
                if (SimpleRoute)
                {
                    foreach (var dc in DomainControllerAddresses.Split(','))
                    {
                        builder.AddRoute(dc, 32, null, null);
                    }
                }
            }

            builder
                .SetRegisterDNS(RegisterDNS)
                .SetByPassForLocal(BypassForLocal)
                .SetAlwaysOnActive(AlwaysOnActive)
                .SetDataEncryption(DataEncryption)
                .SetDisableAdvancedOptionsEditButton(DisableAdvancedOptionsEditButton)
                .SetDisableDisconnectButton(DisableDisconnectButton)
                .SetDisableIKEv2Fragmentation(DisableIKEv2Fragmentation)
                .SetIPv4InterfaceMetric(IPv4InterfaceMetric)
                .SetIPv6InterfaceMetric(IPv6InterfaceMetric)
                .SetNetworkOutageTime(NetworkOutageTime)
                .SetPrivateNetwork(PrivateNetwork)
                .SetUseRasCredentials(UseRasCredential)
                .SetPlumbIKEv2TSAsRoutes(PlumbIKEv2TSAsRoutes);

            Win11Profile = builder.Win11Profile;
            ProfileXml = builder.GetXml();
        }

        public async Task<string> GetDeployScript(HttpClient httpClient)
        {
            var url = UserTunnel ? "/assets/VpnDeploy-User.txt" : "/assets/VpnDeploy-Device.txt";
            return await httpClient.GetStringAsync(url);
        }
    }
}
