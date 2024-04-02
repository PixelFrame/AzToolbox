using ProfileXMLBuilder.Lib;

namespace AzToolbox.Services
{
    public class VpnBuildService
    {
        public bool Win11Profile { get; set; } = false;
        public bool UserTunnel { get; set; } = true;
        public bool ForceTunnel { get; set; } = false;
        public bool DomainNRPT { get; set; } = true;
        public bool SimpleRoute { get; set; } = true;
        public bool CertSelectUseRadiusCA { get; set; } = true;
        public string VpnServer { get; set; } = "";
        public string DomainDnsSuffix { get; set; } = "";
        public string TrustedNetworkDetection { get; set; } = "";
        public string RadiusServerName { get; set; } = "";
        public string RootCAHash { get; set; } = "";
        public string CertSelectCAHash { get; set; } = "";
        public string DnsServers { get; set; } = "";
        public string DomainSubnets { get; set; } = "";
        public string ExcludedSubnets { get; set; } = "";
        public string DomainControllerAddresses { get; set; } = "";
        public bool Peap { get; set; } = false;
        public bool Tls { get; set; } = true;
        public string ProfileXml { get; set; } = "";
        public NativeProtocolType TunnelProtocol { get; set; } = NativeProtocolType.IKEv2;
        public string ProtocolListString { get; set; } = "";
        public int? RetryTimeInHours { get; set; } = null;
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
        public List<TrafficFilter> TrafficFilters { get; set; } = new();
        public List<DomainNameInformation> Nrpt { get; set; } = new();
        public List<Route> Routes { get; set; } = new();
        public List<AppTrigger> AppTriggers { get; set; } = new();
        public CryptographySuite? CryptographySuite { get; set; } = null;
        public DeviceCompliance? DeviceCompliance { get; set; } = null;
        public Proxy? Proxy { get; set; } = null;
        public List<Eku> EkuMapping { get; set; } = new();

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

                if (ForceTunnel)
                {
                    builder.SetRoutingPolicyType(RoutingPolicyType.ForceTunnel)
                        .AddRoutes(Routes.ToArray());
                    if (SimpleRoute)
                    {
                        foreach (var subnet in ExcludedSubnets.Split(','))
                        {
                            var subnetParts = subnet.Split('/');
                            builder.AddRoute(subnetParts[0], byte.Parse(subnetParts[1]), true, null);
                        }
                    }
                }
                else
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

        public enum PreSets
        {
            User_Split_PEAP__TLS,
            User_Split_EAP__TLS,
            User_Split_PEAP__MSCHAPv2,
            User_Split_EAP__MSCHAPv2,
            User_Force_PEAP__TLS,
            User_Force_EAP__TLS,
            User_Force_PEAP__MSCHAPv2,
            User_Force_EAP__MSCHAPv2,
            Device
        }

        public void LoadPreSet(PreSets set)
        {
            switch (set)
            {
                case PreSets.User_Split_PEAP__TLS:
                    LoadPreSet_User_Split_PEAP_TLS(); break;
                case PreSets.User_Split_EAP__TLS:
                    LoadPreSet_User_Split_EAP_TLS(); break;
                case PreSets.User_Split_PEAP__MSCHAPv2:
                    LoadPreSet_User_Split_PEAP_MSCHAPv2(); break;
                case PreSets.User_Split_EAP__MSCHAPv2:
                    LoadPreSet_User_Split_EAP_MSCHAPv2(); break;
                case PreSets.User_Force_PEAP__TLS:
                    LoadPreSet_User_Force_PEAP_TLS(); break;
                case PreSets.User_Force_EAP__TLS:
                    LoadPreSet_User_Force_EAP_TLS(); break;
                case PreSets.User_Force_PEAP__MSCHAPv2:
                    LoadPreSet_User_Force_PEAP_MSCHAPv2(); break;
                case PreSets.User_Force_EAP__MSCHAPv2:
                    LoadPreSet_User_Force_EAP_MSCHAPv2(); break;
                case PreSets.Device:
                    LoadPreSet_Device(); break;
            }
            Build();
        }

        private void LoadPreSet_User_Split_PEAP_TLS()
        {
            UserTunnel = true;
            ForceTunnel = false;
            DomainNRPT = true;
            SimpleRoute = true;
            Routes.Clear();
            CertSelectUseRadiusCA = true;
            VpnServer = "ras.contoso.com";
            DomainDnsSuffix = "corp.contoso.com";
            TrustedNetworkDetection = "corp.contoso.com";
            RadiusServerName = "nps.corp.contoso.com";
            RootCAHash = "00112233aabbccddeeffffeeddccbbaa445566";
            DnsServers = "10.1.1.1";
            DomainSubnets = "10.1.1.0/24";
            Peap = true;
            Tls = true;
            TunnelProtocol = NativeProtocolType.Automatic;
            RegisterDNS = true;
            Nrpt.Clear();
        }
        private void LoadPreSet_User_Split_EAP_TLS()
        {
            UserTunnel = true;
            ForceTunnel = false;
            DomainNRPT = true;
            SimpleRoute = true;
            Routes.Clear();
            CertSelectUseRadiusCA = true;
            VpnServer = "ras.contoso.com";
            DomainDnsSuffix = "corp.contoso.com";
            TrustedNetworkDetection = "corp.contoso.com";
            RadiusServerName = "nps.corp.contoso.com";
            RootCAHash = "00112233aabbccddeeffffeeddccbbaa445566";
            DnsServers = "10.1.1.1";
            DomainSubnets = "10.1.1.0/24";
            Peap = false;
            Tls = true;
            TunnelProtocol = NativeProtocolType.Automatic;
            RegisterDNS = true;
            Nrpt.Clear();
        }
        private void LoadPreSet_User_Split_PEAP_MSCHAPv2()
        {
            UserTunnel = true;
            ForceTunnel = false;
            DomainNRPT = true;
            SimpleRoute = true;
            Routes.Clear();
            CertSelectUseRadiusCA = true;
            VpnServer = "ras.contoso.com";
            DomainDnsSuffix = "corp.contoso.com";
            TrustedNetworkDetection = "corp.contoso.com";
            RadiusServerName = "nps.corp.contoso.com";
            RootCAHash = "00112233aabbccddeeffffeeddccbbaa445566";
            DnsServers = "10.1.1.1";
            DomainSubnets = "10.1.1.0/24";
            Peap = true;
            Tls = false;
            TunnelProtocol = NativeProtocolType.Automatic;
            RegisterDNS = true;
            Nrpt.Clear();
        }
        private void LoadPreSet_User_Split_EAP_MSCHAPv2()
        {
            UserTunnel = true;
            ForceTunnel = false;
            DomainNRPT = true;
            SimpleRoute = true;
            Routes.Clear();
            VpnServer = "ras.contoso.com";
            DomainDnsSuffix = "corp.contoso.com";
            TrustedNetworkDetection = "corp.contoso.com";
            DnsServers = "10.1.1.1";
            DomainSubnets = "10.1.1.0/24";
            Peap = false;
            Tls = false;
            TunnelProtocol = NativeProtocolType.Automatic;
            RegisterDNS = true;
            Nrpt.Clear();
        }
        private void LoadPreSet_User_Force_PEAP_TLS()
        {
            UserTunnel = true;
            ForceTunnel = true;
            DomainNRPT = false;
            SimpleRoute = true;
            Routes.Clear();
            CertSelectUseRadiusCA = true;
            VpnServer = "ras.contoso.com";
            DomainDnsSuffix = "corp.contoso.com";
            TrustedNetworkDetection = "corp.contoso.com";
            RadiusServerName = "nps.corp.contoso.com";
            RootCAHash = "00112233aabbccddeeffffeeddccbbaa445566";
            DnsServers = "10.1.1.1";
            ExcludedSubnets = "192.168.0.0/16";
            Peap = true;
            Tls = true;
            TunnelProtocol = NativeProtocolType.Automatic;
            IPv4InterfaceMetric = 10;
            RegisterDNS = true;
            Nrpt.Clear();
            Nrpt.Add(
                new() { DomainName = ".", DnsServers = DnsServers, }
                );
        }
        private void LoadPreSet_User_Force_EAP_TLS()
        {
            UserTunnel = true;
            ForceTunnel = true;
            DomainNRPT = false;
            SimpleRoute = true;
            Routes.Clear();
            CertSelectUseRadiusCA = true;
            VpnServer = "ras.contoso.com";
            DomainDnsSuffix = "corp.contoso.com";
            TrustedNetworkDetection = "corp.contoso.com";
            RadiusServerName = "nps.corp.contoso.com";
            RootCAHash = "00112233aabbccddeeffffeeddccbbaa445566";
            DnsServers = "10.1.1.1";
            ExcludedSubnets = "192.168.0.0/16";
            Peap = false;
            Tls = true;
            TunnelProtocol = NativeProtocolType.Automatic;
            IPv4InterfaceMetric = 10;
            RegisterDNS = true;
            Nrpt.Clear();
            Nrpt.Add(
                new() { DomainName = ".", DnsServers = DnsServers, }
                );
        }
        private void LoadPreSet_User_Force_PEAP_MSCHAPv2()
        {
            UserTunnel = true;
            ForceTunnel = true;
            DomainNRPT = false;
            SimpleRoute = true;
            Routes.Clear();
            VpnServer = "ras.contoso.com";
            DomainDnsSuffix = "corp.contoso.com";
            TrustedNetworkDetection = "corp.contoso.com";
            RadiusServerName = "nps.corp.contoso.com";
            RootCAHash = "00112233aabbccddeeffffeeddccbbaa445566";
            DnsServers = "10.1.1.1";
            ExcludedSubnets = "192.168.0.0/16";
            Peap = true;
            Tls = false;
            TunnelProtocol = NativeProtocolType.Automatic;
            IPv4InterfaceMetric = 10;
            RegisterDNS = true;
            Nrpt.Clear();
            Nrpt.Add(
                new() { DomainName = ".", DnsServers = DnsServers, }
                );
        }
        private void LoadPreSet_User_Force_EAP_MSCHAPv2()
        {
            UserTunnel = true;
            ForceTunnel = true;
            DomainNRPT = false;
            SimpleRoute = true;
            Routes.Clear();
            VpnServer = "ras.contoso.com";
            DomainDnsSuffix = "corp.contoso.com";
            TrustedNetworkDetection = "corp.contoso.com";
            DnsServers = "10.1.1.1";
            ExcludedSubnets = "192.168.0.0/16";
            Peap = false;
            Tls = false;
            TunnelProtocol = NativeProtocolType.Automatic;
            IPv4InterfaceMetric = 10;
            RegisterDNS = true;
            Nrpt.Clear();
            Nrpt.Add(
                new() { DomainName = ".", DnsServers = DnsServers, }
                );
        }
        private void LoadPreSet_Device()
        {
            UserTunnel = false;
            ForceTunnel = false;
            DomainNRPT = true;
            SimpleRoute = true;
            VpnServer = "ras.contoso.com";
            DomainDnsSuffix = "corp.contoso.com";
            TrustedNetworkDetection = "corp.contoso.com";
            DnsServers = "10.1.1.1";
            DomainControllerAddresses = "10.1.1.1,10.1.1.2";
            TunnelProtocol = NativeProtocolType.IKEv2;
            RegisterDNS = true;
        }
    }
}
