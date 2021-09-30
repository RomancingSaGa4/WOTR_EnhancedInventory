﻿using EnhancedInventory.Controllers;
using Kingmaker;
using Kingmaker.PubSubSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EnhancedInventory.Events
{
    public class OnAreaLoad : IAreaHandler
    {
        public void OnAreaDidLoad()
        {
            Main.RefreshRemappers();

            if (Main.Settings.EnableInventorySearchBar)
            {
                LoadInventorySearchBar();
            }

            if (Main.Settings.EnableSpellbookSearchBar)
            {
                LoadSpellbookSearchBar();
            }

            if (Main.Settings.EnableHighlightableLoot)
            {
                LoadHighlightLoot();
            }

            if (Main.Settings.EnableVisualOverhaulSorting)
            {
                SetupSortingStyle();
            }
        }

        public void OnAreaBeginUnloading()
        { }

        private readonly (string, InventoryType)[] m_inventory_paths = new (string, InventoryType)[]
        {
            // Regular, in-game inventory.
            ("ServiceWindowsPCView/InventoryView/Inventory/Stash/StashContainer", InventoryType.InventoryStash),

            // World map inventory.
            ("ServiceWindowsConfig/InventoryView/Inventory/Stash/StashContainer", InventoryType.InventoryStash),

            // Vendor screen: PC inventory view.
            ("VendorPCView/MainContent/PlayerStash", InventoryType.InventoryStash),

            // Vendor screen: Vendor goods view.
            ("VendorPCView/MainContent/VendorBlock", InventoryType.Vendor),

            // Shared stash: PC inventory view.
            ("LootPCView/Window/Inventory", InventoryType.LootInventoryStash),

            // Shared stash: Stash items view.
            ("LootPCView/Window/Collector", InventoryType.LootCollector),
        };

        private void LoadInventorySearchBar()
        {
            foreach ((string path, InventoryType type) in m_inventory_paths)
            {
                Transform filters_block_transform = Game.Instance.UI.MainCanvas.transform.Find(path);
                if (filters_block_transform != null)
                {
                    filters_block_transform.gameObject.AddComponent<InventoryController>().Type = type;
                }
            }
        }

        private void LoadSpellbookSearchBar()
        {
            string[] paths = new string[]
            {
                "ServiceWindowsPCView/SpellbookView/SpellbookScreen", // game
                "ServiceWindowsConfig/SpellbookView/SpellbookScreen", // world map
            };

            foreach (string path in paths)
            {
                Transform spellbook = Game.Instance.UI.MainCanvas.transform.Find(path);
                if (spellbook != null)
                {
                    spellbook.gameObject.AddComponent<SpellbookController>();
                }
            }
        }

        private void LoadHighlightLoot()
        {
            Transform stash = Game.Instance.UI.MainCanvas.transform.Find("LootPCView/Window/Collector/Collector/StashScrollView/Viewport/Content");
            if (stash != null)
            {
                stash.gameObject.AddComponent<LootHighlightController>();
            }
        }

        private void SetupSortingStyle()
        {
            foreach ((string path, InventoryType type) in m_inventory_paths)
            {
                string viewport_path = $"{path}/{InventoryController.PathToSorter(type)}/Sorting/Dropdown/Template/Viewport";
                Transform viewport = Game.Instance.UI.MainCanvas.transform.Find(viewport_path);

                // This happens if we're on a screen that we don't have access to or screens that have different formatting.
                if (viewport == null) continue;

                Transform content = viewport.Find("Content");
                Transform item = content.Find("Item");

                VerticalLayoutGroup group = content.GetComponent<VerticalLayoutGroup>();
                TextMeshProUGUI item_label = item.Find("Item Label").GetComponent<TextMeshProUGUI>();
                RectTransform item_background = item.Find("Item Background").GetComponent<RectTransform>();
                RectTransform item_checkmark = item.Find("Item Checkmark").GetComponent<RectTransform>();
                RectTransform item_bottom_border = item.Find("BottomBorderImage").GetComponent<RectTransform>();

                group.spacing = 4;
                group.padding.top = 0;
                group.padding.bottom = 0;

                item_label.fontSize = 16.0f;
                item_label.horizontalAlignment = HorizontalAlignmentOptions.Center;

                item_background.anchorMin = new Vector2(0.0f, 0.0f);
                item_background.anchorMax = new Vector2(1.0f, 1.0f);
                item_background.offsetMin = new Vector2(0.0f, 0.0f);
                item_background.offsetMax = new Vector2(0.0f, 0.0f);

                item_checkmark.anchorMin = new Vector2(0.0f, 0.0f);
                item_checkmark.anchorMax = new Vector2(1.0f, 1.0f);
                item_checkmark.offsetMin = new Vector2(0.0f, 0.0f);
                item_checkmark.offsetMax = new Vector2(0.0f, 0.0f);

                item_bottom_border.anchorMin = new Vector2(0.0f, 0.0f);
                item_bottom_border.anchorMax = new Vector2(1.0f, 0.0f);
                item_bottom_border.offsetMin = new Vector2(0.0f, -2.0f);
                item_bottom_border.offsetMax = new Vector2(0.0f, 0.0f);

                GameObject.Destroy(viewport.Find("TopBorderImage").gameObject);
            }
        }
    }
}
