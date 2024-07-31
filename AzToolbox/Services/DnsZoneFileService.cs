using KzA.DNS.ResourceRecord;
using KzA.DNS.Zone;
using System.Collections.ObjectModel;

namespace AzToolbox.Services
{
    public class DnsZoneFileService
    {
        private PrimaryZone _zone = new();

        public DnsZoneFileService()
        {
            Name        = "contoso.org";
            SOA.MNAME   = "ns.contoso.org.";
            SOA.RNAME   = "hostmaster.contoso.org.";
            SOA.SERIAL  = 1;
            SOA.REFRESH = 900;
            SOA.RETRY   = 600;
            SOA.EXPIRE  = 86400;
            SOA.MINIMUM = 3600;

            ViewRecords.Add(new NS()
            {
                Name = "@",
                HostName = "ns.contoso.org."
            });
            ViewRecords.Add(new A()
            {
                Name = "ns",
                Data = System.Net.IPAddress.Parse("10.1.1.1")
            });
        }

        public SOA SOA => _zone.SOA;
        public string Name { get => _zone.Name; set => _zone.Name = value; }
        public ObservableCollection<IRecord> ViewRecords { get; set; } = [];

        public static ArbitraryRecord CreateArbitraryRecord(string name, RRType type, string data, int ttl = -1)
        {
            var record = new ArbitraryRecord
            {
                Name = name,
                TTL = ttl,
                Data = data
            };
            record.ModifyType(type);
            return record;
        }

        public string GenerateFile()
        {
            _zone.DefaultTTL = SOA.MINIMUM;
            _zone.Records.Clear();
            _zone.Records.AddRange(ViewRecords);
            return _zone.ToZoneFile();
        }

        public void ClearAll()
        {
            _zone = new();
            ViewRecords.Clear();
        }

        public void ClearRecords()
        {
            ViewRecords.Clear();
        }

        public static readonly RRType[] SupportedRRTypes = [
            RRType.A, RRType.AAAA, RRType.CNAME,
            RRType.NS, RRType.TXT,
            RRType.MX, RRType.SRV,
            RRType.SVCB, RRType.HTTPS
        ];
    }
}
