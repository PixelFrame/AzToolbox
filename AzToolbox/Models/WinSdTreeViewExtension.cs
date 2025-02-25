using Blazorise.TreeView;
using System.Collections.ObjectModel;
using System.Reflection;
using WinSdUtil.Model;

namespace AzToolbox.Models.SecDescriptor
{
    public static class WinSdTreeViewExtension
    {
        public static TreeViewItem ToTreeView(this AccessControlList acl, AccessMaskType maskType, bool matchedOnly)
        {
            var children = new List<TreeViewItem>();
            var root = new TreeViewItem()
            {
                Label = "ACL",
                Description = "Access Control List",
                Children = children,
                Tag = maskType,
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
                    daceItems.Add(dace.ToTreeView(maskType, trusteeNamePadding, matchedOnly));
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
                    saceItems.Add(sace.ToTreeView(maskType, trusteeNamePadding, matchedOnly));
                }
                saclItem.Children = saceItems;
            }
            return root;
        }

        public static TreeViewItem ToTreeView(this AccessControlEntry ace, AccessMaskType maskType, int trusteeNamePadding, bool matchedOnly)
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
                        Children = new ObservableCollection<TreeViewItem>(AccessMaskToTreeViewItems(ace.Mask.ObjectSpecific, targetType, longestNameLen, false, matchedOnly)),
                    },
                    new()
                    {
                        Label = "Generic".PadRight(objspecLabel.Length),
                        Description = ace.Mask.Standard.ToString("X8")[..4] + "....",
                        Children = new ObservableCollection<TreeViewItem>(AccessMaskToTreeViewItems(ace.Mask.Standard, typeof(AccessMask_Standard), longestNameLen, true, matchedOnly)),
                    },
                ],
                Tag = ace.Mask.Full,
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
                    Description = ace.ApplicationData,
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

        public static IEnumerable<TreeViewItem> AccessMaskToTreeViewItems(uint value, Type targetType, int longestNameLen, bool isStandard, bool matchedOnly)
        {

            var start = isStandard ? 16 : 0;
            var end = isStandard ? 32 : 16;
            var mask = (uint)(isStandard ? 0x00010000 : 0x00000001);

            for (var i = start; i < end; ++i)
            {
                var label = "Undefined".PadRight(longestNameLen);
                if (Enum.IsDefined(targetType, mask))
                {
                    label = Enum.GetName(targetType, mask)!.PadRight(longestNameLen);
                }
                var hit = (mask & value) == mask;
                if (!hit && matchedOnly)
                {
                    mask <<= 1;
                    continue;
                }
                var desc = hit ? "True  " : "False ";
                desc += new string('.', 32 - i - 1);
                if (hit) desc += "1";
                else desc += "0";
                desc += new string('.', i);
                mask <<= 1;
                yield return new()
                {
                    Label = label,
                    Description = desc,
                    Tag = i,
                    Children = null,
                };
            }
        }

        public static void UpdateTreeView(this TreeView<TreeViewItem> tree, AccessMaskType maskType)
        {
            var root = tree.Nodes.FirstOrDefault();
            if (root == null) return;
            root.Tag = maskType;
            var daceNodes = root.Children?.Where(x => x.Label == "DACL").FirstOrDefault()?.Children;
            if (daceNodes != null)
            {
                foreach (var daceNode in daceNodes)
                {
                    var accessMaskNode = daceNode.Children?.Where(x => x.Label == "Access Mask").FirstOrDefault();
                    if (accessMaskNode != null)
                    {
                        var objspecNode = accessMaskNode.Children?.Where(x => x.Label.StartsWith("Object Specific")).FirstOrDefault();
                        if (objspecNode != null)
                        {
                            var objspecLabel = $"Object Specific ({maskType})";
                            objspecNode.Label = objspecLabel;
                            var objspecItems = objspecNode.Children;
                            if (objspecItems != null)
                            {
                                var longestNameLen = CalcLongestNameLen(maskType, out var targetType);
                                foreach (var item in objspecItems)
                                {
                                    var idx = (int)item.Tag!;
                                    var label = "Undefined".PadRight(longestNameLen);
                                    if (Enum.IsDefined(targetType, 1u << idx))
                                    {
                                        label = Enum.GetName(targetType, 1u << idx)!.PadRight(longestNameLen);
                                    }
                                    item.Label = label;
                                }
                            }
                        }
                    }
                }
            }
            var saceNodes = root.Children?.Where(x => x.Label == "SACL").FirstOrDefault()?.Children;
            if (saceNodes != null)
            {
                foreach (var saceNode in saceNodes)
                {
                    var accessMaskNode = saceNode.Children?.Where(x => x.Label == "Access Mask").FirstOrDefault();
                    if (accessMaskNode != null)
                    {
                        var objspecNode = accessMaskNode.Children?.Where(x => x.Label.StartsWith("Object Specific")).FirstOrDefault();
                        if (objspecNode != null)
                        {
                            var objspecLabel = $"Object Specific ({maskType})";
                            objspecNode.Label = objspecLabel;
                            var objspecItems = objspecNode.Children;
                            if (objspecItems != null)
                            {
                                var longestNameLen = CalcLongestNameLen(maskType, out var targetType);
                                foreach (var item in objspecItems)
                                {
                                    var idx = (int)item.Tag!;
                                    var label = "Undefined".PadRight(longestNameLen);
                                    if (Enum.IsDefined(targetType, 1u << idx))
                                    {
                                        label = Enum.GetName(targetType, 1u << idx)!.PadRight(longestNameLen);
                                    }
                                    item.Label = label;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void UpdateTreeView(this TreeView<TreeViewItem> tree, bool matchedOnly)
        {
            var root = tree.Nodes.FirstOrDefault();
            if (root == null) return;
            var maskType = (AccessMaskType)root.Tag!;
            var daceNodes = root.Children?.Where(x => x.Label == "DACL").FirstOrDefault()?.Children;
            if (daceNodes != null)
            {
                var longestNameLen = CalcLongestNameLen(maskType, out var targetType);
                foreach (var daceNode in daceNodes)
                {
                    var accessMaskNode = daceNode.Children?.Where(x => x.Label == "Access Mask").FirstOrDefault();
                    if (accessMaskNode != null)
                    {
                        var maskValue = (uint)accessMaskNode.Tag!;
                        var objspecMaskNodes = (ObservableCollection<TreeViewItem>)accessMaskNode.Children!.ElementAt(0).Children!;
                        objspecMaskNodes.Clear();
                        foreach (var item in (AccessMaskToTreeViewItems(maskValue & 0x0000FFFF, targetType, longestNameLen, false, matchedOnly)))
                        {
                            objspecMaskNodes.Add(item);
                        };
                        var stdMaskNodes = (ObservableCollection<TreeViewItem>)accessMaskNode.Children!.ElementAt(1).Children!;
                        stdMaskNodes.Clear();
                        foreach (var item in AccessMaskToTreeViewItems(maskValue & 0xFFFF0000, typeof(AccessMask_Standard), longestNameLen, true, matchedOnly))
                        {
                            stdMaskNodes.Add(item);
                        }
                    }
                }
            }
            var saceNodes = root.Children?.Where(x => x.Label == "DACL").FirstOrDefault()?.Children;
            if (saceNodes != null)
            {
                var longestNameLen = CalcLongestNameLen(maskType, out var targetType);
                foreach (var saceNode in saceNodes)
                {
                    var accessMaskNode = saceNode.Children?.Where(x => x.Label == "Access Mask").FirstOrDefault();
                    if (accessMaskNode != null)
                    {
                        var maskValue = (uint)accessMaskNode.Tag!;
                        var objspecMaskNodes = (ObservableCollection<TreeViewItem>)accessMaskNode.Children!.ElementAt(0).Children!;
                        objspecMaskNodes.Clear();
                        foreach (var item in (AccessMaskToTreeViewItems(maskValue & 0x0000FFFF, targetType, longestNameLen, false, matchedOnly)))
                        {
                            objspecMaskNodes.Add(item);
                        };
                        var stdMaskNodes = (ObservableCollection<TreeViewItem>)accessMaskNode.Children!.ElementAt(1).Children!;
                        stdMaskNodes.Clear();
                        foreach (var item in AccessMaskToTreeViewItems(maskValue & 0xFFFF0000, typeof(AccessMask_Standard), longestNameLen, true, matchedOnly))
                        {
                            stdMaskNodes.Add(item);
                        }
                    }
                }
            }
        }
    }
}
