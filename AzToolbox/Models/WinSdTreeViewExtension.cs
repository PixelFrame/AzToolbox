using System.Reflection;
using WinSdUtil.Model;

namespace AzToolbox.Models
{
    public static class WinSdTreeViewExtension
    {
        public static TreeViewItem ToTreeView(this AccessControlList acl, AccessMaskType maskType)
        {
            var children = new List<TreeViewItem>();
            var root = new TreeViewItem()
            {
                Label = "ACL",
                Description = "Access Control List",
                Children = children,
            };
            if (acl.Owner != null)
            {
                children.Add(new TreeViewItem()
                {
                    Label = "Owner",
                    Description = acl.Owner.DisplayName,
                    Children = acl.Owner.ToTreeView(),
                });
            }
            if (acl.Group != null)
            {
                children.Add(new TreeViewItem()
                {
                    Label = "Group",
                    Description = acl.Group.DisplayName,
                    Children = acl.Group.ToTreeView(),
                });
            }
            if (acl.DAclAces != null)
            {
                var daclItem = new TreeViewItem()
                {
                    Label = "DACL",
                    Description = "Discretionary Access Control List",
                };
                children.Add(daclItem);

                var daceItems = new List<TreeViewItem>();
                int trusteeNamePadding = acl.DAclAces.Select(acl => acl.Trustee.DisplayName.Length).Max() + 1;
                foreach (var dace in acl.DAclAces)
                {
                    daceItems.Add(dace.ToTreeView(maskType, trusteeNamePadding));
                }
                daclItem.Children = daceItems;
            }
            if (acl.SAclAces != null)
            {
                var saclItem = new TreeViewItem()
                {
                    Label = "SACL",
                    Description = "System Access Control List",
                };
                children.Add(saclItem);

                var saceItems = new List<TreeViewItem>();
                int trusteeNamePadding = acl.SAclAces.Select(acl => acl.Trustee.DisplayName.Length).Max() + 1;
                foreach (var sace in acl.SAclAces)
                {
                    saceItems.Add(sace.ToTreeView(maskType, trusteeNamePadding));
                }
                saclItem.Children = saceItems;
            }
            return root;
        }

        public static TreeViewItem ToTreeView(this AccessControlEntry ace, AccessMaskType maskType, int trusteeNamePadding)
        {
            var children = new List<TreeViewItem>();
            var root = new TreeViewItem()
            {
                Label = ace.Type.ToString(),
                Description = ace.Trustee.DisplayName.PadRight(trusteeNamePadding) + ace.Mask.ToSDDL(),
                Children = children,
            };
            children.AddRange(ace.Trustee.ToTreeView());
            children.Add(new TreeViewItem()
            {
                Label = "Flags",
                Description = ace.Flags.ToString(),
                Children = null,
            });
            var objspecLabel = $"Object Specific ({maskType})";
            var longestNameLen = CalcLongestNameLen(maskType, out var targetType);
            children.Add(new TreeViewItem()
            {
                Label = "Access Mask",
                Description = $"0x{ace.Mask.Full:X8}",
                Children =
                [
                    new()
                    {
                        Label = objspecLabel,
                        Description = "...." + ace.Mask.ObjectSpecific.ToString("X8")[4..],
                        Children = AccessMaskToTreeViewItems(ace.Mask.ObjectSpecific, targetType, longestNameLen, false),
                    },
                    new()
                    {
                        Label = "Generic".PadRight(objspecLabel.Length),
                        Description = ace.Mask.Standard.ToString("X8")[..4] + "....",
                        Children = AccessMaskToTreeViewItems(ace.Mask.Standard, typeof(AccessMask_Standard), longestNameLen, true),
                    },
                ],
            });
            if (ace.ObjectGuid != Guid.Empty)
            {
                children.Add(new TreeViewItem()
                {
                    Label = "Object GUID",
                    Description = $"{ace.AdObjectGuid.Type}: {ace.AdObjectGuid.DisplayName} ({ace.AdObjectGuid.Guid})",
                    Children = null,
                });
            }
            if (ace.InheritObjectGuid != Guid.Empty)
            {
                children.Add(new TreeViewItem()
                {
                    Label = "Inherited Object GUID",
                    Description = $"{ace.AdInheritObjectGuid.Type}: {ace.AdInheritObjectGuid.DisplayName} ({ace.AdInheritObjectGuid.Guid})",
                    Children = null,
                });
            }
            if (ace.ApplicationData != null && ace.ApplicationData.Length > 0)
            {
                children.Add(new TreeViewItem()
                {
                    Label = "Application Data",
                    Description = Convert.ToHexString(ace.ApplicationData),
                    Children = null,
                });
            }
            return root;
        }

        public static TreeViewItem[] ToTreeView(this Trustee trustee)
        {
            return
            [
                new()
                {
                    Label = "SID",
                    Description = trustee.Sid,
                    Tag = trustee,
                    Children = null,
                },new()
                {
                    Label = "Name",
                    Description = trustee.Name,
                    Tag = trustee,
                    Children = null,
                },
            ];
        }

        public static int CalcLongestNameLen(AccessMaskType maskType, out Type targetType)
        {
            var maskTypeName = $"WinSdUtil.Model.AccessMask_{maskType}";
            Assembly asm = typeof(AccessMaskType).Assembly;
            targetType = asm.GetType(maskTypeName)!;
            var isStandard = targetType == typeof(AccessMask_Standard);
            var enumNames = Enum.GetNames(targetType);
            var longestNameLen = enumNames.Max(x => x.Length);
            if (longestNameLen < 23) longestNameLen = 23;

            return longestNameLen;
        }

        public static TreeViewItem[] AccessMaskToTreeViewItems(uint Value, Type targetType, int longestNameLen, bool isStandard)
        {
            var currentValue = Convert.ToUInt32(Value);
            var treeViewItems = new TreeViewItem[16];

            var start = isStandard ? 16 : 0;
            var end = isStandard ? 32 : 16;
            var mask = (uint)(isStandard ? 0x00010000 : 0x00000001);

            for (var i = start; i < end; ++i)
            {
                var idx = isStandard ? i - 16 : i;
                var label = "Undefined".PadRight(longestNameLen);
                if (Enum.IsDefined(targetType, mask))
                {
                    label = Enum.GetName(targetType, mask)!.PadRight(longestNameLen);
                }
                var hit = (mask & currentValue) == mask;
                var desc = hit ? "True  " : "False ";
                desc += new string('.', 32 - i - 1);
                if (hit) desc += "1";
                else desc += "0";
                desc += new string('.', i);
                treeViewItems[idx] = new TreeViewItem()
                {
                    Label = label,
                    Description = desc,
                    Tag = i,
                    Children = null,
                };
                mask <<= 1;
            }

            return treeViewItems;
        }
    }
}
