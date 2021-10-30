using EnhancedInventory.Settings;
using EnhancedInventory.Util;
using HarmonyLib;
using Kingmaker.Blueprints.Root;
using Kingmaker.UI.Common;
using Kingmaker.UI.MVVM._PCView.Slots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UniRx;

namespace EnhancedInventory.Hooks
{
    // Handles both adding selected sorters to the sorter dropdowns and making sure that the dropdown is properly updates to match the selected sorter.
    [HarmonyPatch(typeof(ItemsFilterPCView))]
    public static class ItemsFilterPCView_
    {
        private static readonly MethodInfo[] m_cbs = new MethodInfo[]
        {
            AccessTools.Method(typeof(ItemsFilterPCView_), nameof(SetDropdown)),
            AccessTools.Method(typeof(ItemsFilterPCView_), nameof(SetSorter)),
            AccessTools.Method(typeof(ItemsFilterPCView_), nameof(ObserveFilterChange)),
        };

        private static void SetDropdown(ItemsFilterPCView instance, ItemsFilter.SorterType val)
        {
            instance.m_Sorter.value = Main.SorterMapper.From((int)val);
        }

        private static void SetSorter(ItemsFilterPCView instance, int val)
        {
            instance.ViewModel.SetCurrentSorter((ItemsFilter.SorterType)Main.SorterMapper.To(val));
        }

        private static ItemsFilter.FilterType _last_filter;

        private static void ObserveFilterChange(ItemsFilterPCView instance, ItemsFilter.FilterType filter)
        {
            if (_last_filter != filter)
            {
                _last_filter = filter;
                instance.ScrollToTop();
            }
        }

        // In BindViewImplementation, there are two inline delegates; we replace both of those in order with our own.
        [HarmonyTranspiler]
        [HarmonyPatch(nameof(ItemsFilterPCView.BindViewImplementation))]
        public static IEnumerable<CodeInstruction> BindViewImplementation(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> il = instructions.ToList();

            int ldftn_count = 0;

            for (int i = 0; i < il.Count && ldftn_count < m_cbs.Length; ++i)
            {
                if (il[i].opcode == OpCodes.Ldftn)
                {
                    il[i].operand = m_cbs[ldftn_count++];
                }
            }

            return il.AsEnumerable();
        }

        // Adds the sorters to the dropdown.
        [HarmonyPostfix]
        [HarmonyPatch(nameof(ItemsFilterPCView.Initialize), new Type[] { })]
        public static void Initialize(ItemsFilterPCView __instance)
        {
            __instance.m_Sorter.ClearOptions();

            List<string> options = new List<string>();

            foreach (SorterCategories flag in EnumHelper.ValidSorterCategories)
            {
                if (Main.Settings.SorterOptions.HasFlag(flag))
                {
                    (int idx, string text) = Main.SorterCategoryMap[flag];

                    if (text == null)
                    {
                        text = LocalizedTexts.Instance.ItemsFilter.GetText((ItemsFilter.SorterType)idx);
                        Main.SorterCategoryMap[flag] = (idx, text);
                    }

                    options.Add(text);
                }
            }

            __instance.m_Sorter.AddOptions(options);
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(ItemsFilterPCView.Initialize), new Type[] { typeof(bool) })]
        public static void Initialize_Prefix(ref bool needReset)
        {
            needReset = false;
        }
    }
}
