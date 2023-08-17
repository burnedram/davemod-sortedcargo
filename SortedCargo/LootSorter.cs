using DR;
using System;
using System.Linq;
using TMPro;
using Generic = System.Collections.Generic;
using Il2CppGeneric = Il2CppSystem.Collections.Generic;
using ScrollerCellData = PauseLootBoxScrollPanel.ScrollerCellData;

namespace SortedCargo;

public class LootSorter
{
    /// <summary>
    /// -1 is "no grade", e.g. certain ingredients<br/>
    /// 1 is usually dead fish<br/>
    /// swap these around since dead fish is trash<br/>
    /// idk what 0 means
    /// </summary>
    public static int Grader(int grade)
    {
        if (grade == 1)
            return -1;
        if (grade == -1)
            return 1;
        return grade;
    }

    public static readonly Func<(ScrollerCellData cell, IItemBase item), float>
        KEY_WEIGHT = d => d.item.ItemWeight;
    public static readonly Func<(ScrollerCellData cell, IItemBase item), int>
        KEY_GRADE = d => Grader(d.cell.grade),
        KEY_RANK = d => d.item.ItemRank,
        KEY_ID = d => d.cell.itemID;
    public static readonly Func<(ScrollerCellData cell, IItemBase item), ItemType>
        KEY_TYPE = d => d.cell.itemType;
    public static readonly Comparison<(ScrollerCellData cell, IItemBase item)>
        SORT_COMMON = (left, right) =>
        {
            int res;
            if ((res = KEY_TYPE(left) - KEY_TYPE(right)) != 0) return res;
            if ((res = KEY_RANK(left) - KEY_RANK(right)) != 0) return res;
            if ((res = Generic::Comparer<float>.Default.Compare(KEY_WEIGHT(right), KEY_WEIGHT(left))) != 0) return res;
            if ((res = KEY_ID(left) - KEY_ID(right)) != 0) return res;
            return KEY_GRADE(left) - KEY_GRADE(right);
        };

    public readonly PauseLootBoxScrollPanel LootPanel;
    public readonly TextMeshProUGUI Label;
    public readonly Generic::IEnumerator<Il2CppGeneric::List<IScrollerCellData>> Enumerator;

    public LootSorter(PauseLootBoxScrollPanel lootPanel, TextMeshProUGUI label) {
        LootPanel = lootPanel;
        Label = label;
        Enumerator = SortEnumerator();
    }

    public void Sort()
    {
        Enumerator.MoveNext();
        var focus = LootPanel.GetFocusIndex();
        LootPanel.SetDataList(Enumerator.Current);
        LootPanel.UnfocusAll();
        LootPanel.SetFocus(focus);
    }

    private bool SyncState(
        ref Generic::List<IScrollerCellData> defaultOrder,
        Generic::HashSet<IntPtr> ptrs,
        ref Generic::List<(ScrollerCellData cell, IItemBase item)> commonOrder,
        ref string oldDescription,
        string description)
    {
        if (Label.text != oldDescription) {
            // Pause panel has been closed and opened
            oldDescription = Label.text;
            return false;
        }
        Label.text = oldDescription = description;

        var data = Enumerable.Range(0, LootPanel.DataCount)
            .Select(LootPanel.GetData)
            .ToList();
        if (ptrs.SetEquals(data.Select(d => d.Pointer)))
            return true;

        if (ptrs.IsSupersetOf(data.Select(d => d.Pointer)))
        {
            // Items have only been removed
            ptrs.IntersectWith(data.Select(d => d.Pointer));
            commonOrder.RemoveAll(d => !ptrs.Contains(d.cell.Pointer));
        }
        else
        {
            // Items have been added (and maybe removed)
            // Should be because pause panel has been closed and opened again
            ptrs.Clear();
            ptrs.UnionWith(data.Select(d => d.Pointer));
            commonOrder = data
                .Select(cell => cell.Cast<ScrollerCellData>())
                .Select(cell => (cell, item: DataManager.Instance.GetItemBase(cell.itemID)))
                .ToList();
            commonOrder.Sort(SORT_COMMON);
        }
        defaultOrder = data;
        return true;
    }

    private Generic::IEnumerator<Il2CppGeneric::List<IScrollerCellData>> SortEnumerator()
    {
        var defaultDesc = Label.text;
        var defaultOrder = new Generic::List<IScrollerCellData>();
        var ptrs = new Generic::HashSet<IntPtr>();
        var commonOrder = new Generic::List<(ScrollerCellData cell, IItemBase item)>();

        while (true)
        {
            var desc = defaultDesc;
            while (!SyncState(ref defaultOrder, ptrs, ref commonOrder, ref desc, defaultDesc + " ▲"));
            yield return defaultOrder
                .Reverse<IScrollerCellData>()
                .ToIl2CppList();

            if (!SyncState(ref defaultOrder, ptrs, ref commonOrder, ref desc, "Weight ▼"))
                continue;
            yield return commonOrder
                .OrderBy(KEY_WEIGHT)
                .Select(d => d.cell.Cast<IScrollerCellData>())
                .ToIl2CppList();

            if (!SyncState(ref defaultOrder, ptrs, ref commonOrder, ref desc, "Weight ▲"))
                continue;
            yield return commonOrder
                .OrderByDescending(KEY_WEIGHT)
                .Select(d => d.cell.Cast<IScrollerCellData>())
                .ToIl2CppList();

            if (!SyncState(ref defaultOrder, ptrs, ref commonOrder, ref desc, "Grade ▼"))
                continue;
            yield return commonOrder
                .OrderBy(KEY_GRADE)
                .Select(d => d.cell.Cast<IScrollerCellData>())
                .ToIl2CppList();

            if (!SyncState(ref defaultOrder, ptrs, ref commonOrder, ref desc, "Grade ▲"))
                continue;
            yield return commonOrder
                .OrderByDescending(KEY_GRADE)
                .Select(d => d.cell.Cast<IScrollerCellData>())
                .ToIl2CppList();

            if (!SyncState(ref defaultOrder, ptrs, ref commonOrder, ref desc, "Rank ▼"))
                continue;
            yield return commonOrder
                .OrderBy(KEY_RANK)
                .Select(d => d.cell.Cast<IScrollerCellData>())
                .ToIl2CppList();

            if (!SyncState(ref defaultOrder, ptrs, ref commonOrder, ref desc, "Rank ▲"))
                continue;
            yield return commonOrder
                .OrderByDescending(KEY_RANK)
                .Select(d => d.cell.Cast<IScrollerCellData>())
                .ToIl2CppList();

            if (!SyncState(ref defaultOrder, ptrs, ref commonOrder, ref desc, "Type ▼"))
                continue;
            yield return commonOrder
                .OrderBy(KEY_TYPE)
                .Select(d => d.cell.Cast<IScrollerCellData>())
                .ToIl2CppList();

            if (!SyncState(ref defaultOrder, ptrs, ref commonOrder, ref desc, "Type ▲"))
                continue;
            yield return commonOrder
                .OrderByDescending(KEY_TYPE)
                .Select(d => d.cell.Cast<IScrollerCellData>())
                .ToIl2CppList();

            if (!SyncState(ref defaultOrder, ptrs, ref commonOrder, ref desc, defaultDesc))
                continue;
            yield return defaultOrder.ToIl2CppList();
        }
    }
}

public static class Il2CppLinqExtensions {
    public static Il2CppGeneric::List<T> ToIl2CppList<T>(this Generic::IEnumerable<T> enumerable)
    {
        var res = new Il2CppGeneric::List<T>();
        foreach (var item in enumerable)
            res.Add(item);
        return res;
    }
}
