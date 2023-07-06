using System.Buffers.Binary;

namespace AzToolbox.Utilities
{
    internal static class DhcpSearchListUtil
    {
        const ushort INDEX_MAX = 0x3FFF;
        const ushort POINTER_MASK = 0xC000;
        const byte LABEL_LEN_MAX = 0x3F;

        private static ushort TableLookup(in string suffix, in ushort idx, in Dictionary<string, ushort> SuffixIdxTable)
        {
            if (idx > INDEX_MAX)
            {
                throw new Exception("Index too large, the option cannot be compressed.");
            }
            if (SuffixIdxTable.ContainsKey(suffix))
            {
                return SuffixIdxTable[suffix];
            }
            else
            {
                SuffixIdxTable.Add(suffix, idx);
                return 0x4000;
            }
        }

        public static List<byte> EncodeSearchList(IEnumerable<string> SearchList, bool Compression = true)
        {
            List<byte> result = new();
            Dictionary<string, ushort> SuffixIdxTable = new();
            ushort index = 0;
            foreach (var name in SearchList)
            {
                var suffix = name;
                while (suffix.Length > 0)
                {
                    if (Compression)
                    {
                        var lookupResult = TableLookup(in suffix, in index, in SuffixIdxTable);
                        if (lookupResult <= INDEX_MAX)
                        {
                            var pointer = (ushort)(lookupResult | POINTER_MASK);
                            var pointerBytes = new byte[2];
                            BinaryPrimitives.WriteUInt16BigEndian(pointerBytes, pointer);
                            result.AddRange(pointerBytes);
                            index += 2;
                            break;
                        }
                    }
                    if (suffix.Contains('.'))
                    {
                        var label = suffix[..suffix.IndexOf('.')];
                        if (label.Length > 255)
                        {
                            throw new Exception($"Label too long: {label}");
                        }
                        result.Add((byte)label.Length);
                        result.AddRange(System.Text.Encoding.ASCII.GetBytes(label));
                        index += (ushort)(label.Length + 1);
                        suffix = suffix[(suffix.IndexOf('.') + 1)..];
                    }
                    else
                    {
                        if (suffix.Length > LABEL_LEN_MAX)
                        {
                            throw new Exception($"Label too long: {suffix}");
                        }
                        result.Add((byte)suffix.Length);
                        result.AddRange(System.Text.Encoding.ASCII.GetBytes(suffix));
                        result.Add(0x0);
                        index += (ushort)(suffix.Length + 2);
                        suffix = string.Empty;
                    }
                }
            }
            return result;
        }

        public static string GetNetshCommand(IEnumerable<byte> OptionValue, string? ScopeId)
        {
            return $"netsh dhcp server V4{(ScopeId is null ? string.Empty : $" scope {ScopeId}")} set optionvalue 119 BYTE {string.Join(' ', OptionValue.Select(x => x.ToString("X2")))}";
        }

        public static string GetPowerShellCommand(IEnumerable<byte> OptionValue, string? ScopeId)
        {
            return $"Add-DhcpServerv4OptionValue -OptionId 119{(ScopeId is null ? string.Empty : $" -ScopeId {ScopeId}")} -Value {string.Join(',', OptionValue)}";
        }
    }
}