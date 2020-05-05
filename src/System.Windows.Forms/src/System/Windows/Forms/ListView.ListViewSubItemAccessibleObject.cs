﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using static System.Windows.Forms.ColumnHeader;
using static Interop;

namespace System.Windows.Forms
{
    public partial class ListView
    {
        internal class ListViewSubItemAccessibleObject : AccessibleObject
        {
            private ListView _owningListView;
            private ListViewItem _owningItem;
            private ListViewItem.ListViewSubItem _owningSubItem;

            public ListViewSubItemAccessibleObject(ListViewItem.ListViewSubItem owningSubItem, ListViewItem owningItem)
            {
                _owningSubItem = owningSubItem ?? throw new ArgumentNullException(nameof(owningSubItem));
                _owningItem = owningItem ?? throw new ArgumentNullException(nameof(owningItem));
                _owningListView = owningItem.ListView ?? throw new ArgumentNullException(nameof(owningItem.ListView));
            }

            internal override UiaCore.IRawElementProviderFragmentRoot FragmentRoot
                => _owningListView.AccessibilityObject;

            public override Rectangle Bounds
                => new Rectangle(
                        _owningListView.AccessibilityObject.Bounds.X + _owningSubItem.Bounds.X,
                        _owningListView.AccessibilityObject.Bounds.Y + _owningSubItem.Bounds.Y,
                        _owningSubItem.Bounds.Width,
                        _owningSubItem.Bounds.Height);

            internal override UiaCore.IRawElementProviderFragment? FragmentNavigate(UiaCore.NavigateDirection direction)
            {
                switch (direction)
                {
                    case UiaCore.NavigateDirection.Parent:
                        return _owningItem.AccessibilityObject;
                    case UiaCore.NavigateDirection.NextSibling:
                        int nextSubItemIndex = GetCurrentSubItemIndex() + 1;
                        if (_owningItem.SubItems.Count > nextSubItemIndex)
                        {
                            return _owningItem.SubItems[nextSubItemIndex].AccessibilityObject;
                        }
                        break;
                    case UiaCore.NavigateDirection.PreviousSibling:
                        int previousSubItemIndex = GetCurrentSubItemIndex() - 1;
                        if (previousSubItemIndex >= 0)
                        {
                            return _owningItem.SubItems[previousSubItemIndex].AccessibilityObject;
                        }
                        break;
                }

                return base.FragmentNavigate(direction);
            }

            /// <summary>
            ///  Gets or sets the accessible name.
            /// </summary>
            public override string? Name
            {
                get => _owningSubItem.Text;
                set => base.Name = value;
            }

            public override AccessibleObject Parent
                => _owningItem.AccessibilityObject;

            internal override int[]? RuntimeId
            {
                get
                {
                    var owningItemRuntimeId = Parent.RuntimeId;
                    if (owningItemRuntimeId is null)
                    {
                        return base.RuntimeId;
                    }

                    var runtimeId = new int[5];
                    runtimeId[0] = owningItemRuntimeId[0];
                    runtimeId[1] = owningItemRuntimeId[1];
                    runtimeId[2] = owningItemRuntimeId[2];
                    runtimeId[3] = owningItemRuntimeId[3];
                    runtimeId[4] = GetCurrentSubItemIndex();
                    return runtimeId;
                }
            }

            internal override object? GetPropertyValue(UiaCore.UIA propertyID)
                => propertyID switch
                {
                    UiaCore.UIA.ControlTypePropertyId => UiaCore.UIA.TextControlTypeId,
                    UiaCore.UIA.NamePropertyId => Name,
                    UiaCore.UIA.FrameworkIdPropertyId => NativeMethods.WinFormFrameworkId,
                    UiaCore.UIA.ProcessIdPropertyId => Process.GetCurrentProcess().Id,
                    UiaCore.UIA.AutomationIdPropertyId => AutomationId,
                    UiaCore.UIA.RuntimeIdPropertyId => RuntimeId,
                    UiaCore.UIA.HasKeyboardFocusPropertyId => _owningListView.Focused && _owningListView.FocusedItem == _owningItem,
                    UiaCore.UIA.IsKeyboardFocusablePropertyId => (State & AccessibleStates.Focusable) == AccessibleStates.Focusable,
                    UiaCore.UIA.IsEnabledPropertyId => _owningListView.Enabled,
                    UiaCore.UIA.IsOffscreenPropertyId => (State & AccessibleStates.Offscreen) == AccessibleStates.Offscreen,
                    UiaCore.UIA.BoundingRectanglePropertyId => Bounds,
                    UiaCore.UIA.IsGridItemPatternAvailablePropertyId => IsPatternSupported(UiaCore.UIA.GridItemPatternId),
                    UiaCore.UIA.IsTableItemPatternAvailablePropertyId => IsPatternSupported(UiaCore.UIA.TableItemPatternId),
                    _ => base.GetPropertyValue(propertyID)
                };

            /// <summary>
            ///  Gets the accessible state.
            /// </summary>
            public override AccessibleStates State
                => AccessibleStates.Focusable;

            internal override UiaCore.IRawElementProviderSimple ContainingGrid
                => _owningListView.AccessibilityObject;

            internal override int Row
                => _owningItem.Index;

            internal override int Column
                => _owningItem.SubItems.IndexOf(_owningSubItem);

            internal override UiaCore.IRawElementProviderSimple[]? GetColumnHeaderItems()
                => new UiaCore.IRawElementProviderSimple[] { _owningListView.Columns[Column].AccessibilityObject };

            internal override bool IsPatternSupported(UiaCore.UIA patternId)
            {
                if (patternId == UiaCore.UIA.GridItemPatternId ||
                    patternId == UiaCore.UIA.TableItemPatternId)
                {
                    return true;
                }

                return base.IsPatternSupported(patternId);
            }

            private string AutomationId
                => string.Format("{0}-{1}", typeof(ListViewItem.ListViewSubItem).Name, GetCurrentSubItemIndex());

            private int GetCurrentSubItemIndex()
                => _owningItem.SubItems.IndexOf(_owningSubItem);
        }
    }
}
