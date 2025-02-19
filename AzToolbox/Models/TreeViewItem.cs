using Blazorise;
using System.Reflection;
using System.Text;
using WinSdUtil.Model;

namespace AzToolbox.Models
{
    public class TreeViewItem
    {
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public object? Tag { get; set; }
        public IEnumerable<TreeViewItem>? Children { get; set; }
        public bool HasChildren => Children?.Any() ?? false;


        public override string ToString()
        {
            var sbResult = new StringBuilder();
            PrintNode("", true, true, ref sbResult);

            return sbResult.ToString();
        }

        private void PrintNode(string indent, bool root, bool last, ref StringBuilder sbResult)
        {
            sbResult.Append(indent);
            if (root) { }
            else if (last)
            {
                sbResult.Append(@"└─");
                indent += "  ";
            }
            else
            {
                sbResult.Append(@"├─");
                indent += "│ ";
            }
            sbResult.AppendLine($"{Label}: {Description}");
            if (HasChildren)
            {
                for (int i = 0; i < Children!.Count(); i++)
                    Children!.ElementAt(i).PrintNode(indent, false, i == Children!.Count() - 1, ref sbResult);
            }
        }
    }
}