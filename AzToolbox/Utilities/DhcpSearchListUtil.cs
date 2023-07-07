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
                return INDEX_MAX + 1;
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

        private static string ReadName(IEnumerable<byte> SearchList, int index, ref int elapsed, ref List<ushort> passedPtr)
        {
            /* 
             * We will record all the pointers passed, if we encounter a pointer that we have passed before, it means we have a loop in the option value.
             * This detection method makes us still support forward pointers.
             */
            var str = string.Empty;
            byte len = SearchList.ElementAt(index);
            if (len > LABEL_LEN_MAX)
            {
                var ptr = (ushort)(BinaryPrimitives.ReadUInt16BigEndian(SearchList.Skip(index).Take(2).ToArray()) & ~POINTER_MASK);
                if (passedPtr.Contains(ptr))
                {
                    throw new Exception("Loop detected in the option value.");
                }
                passedPtr.Add(ptr);
                elapsed += 2;
                int _ = 0;
                return ReadName(SearchList, ptr, ref _, ref passedPtr);
            }
            else if (len == 0)
            {
                passedPtr.Clear();
                elapsed += 1;
                return str;
            }
            else
            {
                elapsed += len + 1;
                str += System.Text.Encoding.ASCII.GetString(SearchList.Skip(index + 1).Take(len).ToArray());
                return str + '.' + ReadName(SearchList, index + len + 1, ref elapsed, ref passedPtr);
            }
        }

        public static List<string> DecodeSearchList(IEnumerable<byte> SearchList)
        {
            List<string> result = new();
            var index = 0;
            while (index < SearchList.Count())
            {
                int elapsed = 0;
                var passedPtr = new List<ushort>();
                var name = ReadName(SearchList, index, ref elapsed, ref passedPtr);
                index += elapsed;
                result.Add(name.Remove(name.Length - 1));
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