# AzToolbox

Web Toolbox built with Blazor WASM hosted on Azure Web App service.

## Implemented Utilities

```
AzToolbox
|
+---Networking
|   +---DhcpSearchList : Encode/Decode DHCP Option 119
|   +---PacParse       : Parse Proxy Auto-Configuration File
|   +---BDPCalc        : BDP Calculator
|
+---Windows
|   +---VpnBuld        : VPN Profile XML Builder
|   +---WfpAnalyze     : WFP Log Analyze Helper
|   +---SecDescriptor  : Security Descriptor Tool
|
+---Common
    +---Base64Utils    : Encode/Decode Base64
    +---RngUtils       : Generate Random Characters/Strings
```

## Roadmap

### Planned

- [ ] Common/Dummy File Creator
- [ ] Common/Unit Converter
- [ ] Common/HEXEH Frontend (HEX Converter)
- [ ] Common/HEX Editor (KzA.Blazor.HexDump)
- [ ] Common/Comic Archive Reader
- [ ] Windows/Well-Known Registry
- [ ] Networking/RFC Tree

### Under Consideration

- [ ] Windows/Pbk to ProfileXML (Some binary data structures are not documented)
- [ ] PCAP Creator (Online text2pcap with a GUI, probably difficult to implement)

## Need Investigation

- WASM Multithreading: Need .NET 8
- Native(C/CPP) Library?
- Component Loading As Needed