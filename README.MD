# AzToolbox

Web Toolbox built with Blazor WASM hosted on Azure Web App service.

## Implemented Utilities

```
AzToolbox
|
+---Networking
|   +---DhcpSearchList   : Encode/Decode DHCP Option 119
|   +---PacParse         : Parse Proxy Auto-Configuration File
|   +---BDPCalc          : BDP Calculator
|   +---ULAGen           : Generate IPv6 ULA subnet
|
+---Windows
|   +---VpnBuld          : VPN Profile XML Builder
|   +---WfpAnalyze       : WFP Log Analyze Helper
|   +---SecDescriptor    : Security Descriptor Tool
|
+---Common
|   +---Base64Utils      : Encode/Decode Base64
|   +---RngUtils         : Generate Random Characters/Strings
|   +---DummyFileCreator : Create Dummy File filled with Random Data / FF / 00
|   +---HEXEH            : Binary data interpreter
|
+---Experimental
    +---ArchiveReader    : Display images from a zip archive
```

## Roadmap

### Planned

- [x] Common/Dummy File Creator [Need File System Access API]
- [x] Common/HEXEH Frontend (HEX interpreter)
- [ ] Common/HEX Editor (KzA.Blazor.HexDump)
- [x] Experimental/Archive Reader
- [ ] Common/Dice

### Under Consideration

- [ ] Windows/Pbk to ProfileXML (Some binary data structures are not documented)
- [ ] PCAP Creator (Online text2pcap with a GUI, probably difficult to implement)

## Need Investigation

- WASM Multithreading: Not Available for Blazor yet :(
- Native(C/CPP) Library?