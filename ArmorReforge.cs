using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using System;
using Terraria.ID;
using Terraria.Utilities;

namespace ArmorReforge
{
    internal static class ArmorReforgeUtils
    {
        internal static bool IsArmor(this Item item) => (item.headSlot != -1 || item.bodySlot != -1 || item.legSlot != -1);// && !item.vanity;
        internal static void OverrideReforge(this Item item) => item.accessory = (item.IsArmor() && !item.accessory) || item.accessory;
        internal static void RerollArmor(this Item item) => item.Prefix(-2);
        internal static int ArmorRecipeValue(this Item item) // Get armor recipe value.
        {
            foreach (var recipe in Main.recipe)
            {
                if (recipe.HasResult(item.netID))
                {
                    var recipeValue = 0;
                    List<Item> getIngredients = recipe.requiredItem;
                    foreach (var ingredient in getIngredients)
                    {
                        recipeValue += (ingredient.value * ingredient.stack);
                    }
                    return (int)(recipeValue + (item.rare * recipeValue) * 0.1) / 3;
                }
            }
            return item.value;
        }
        internal static readonly int[] accModifier = { // Vanilla modifiers
                            PrefixID.Hard,
                            PrefixID.Guarding,
                            PrefixID.Armored,
                            PrefixID.Warding,
                            PrefixID.Precise,
                            PrefixID.Lucky,
                            PrefixID.Jagged,
                            PrefixID.Spiked,
                            PrefixID.Angry,
                            PrefixID.Menacing,
                            PrefixID.Brisk,
                            PrefixID.Fleeting,
                            PrefixID.Hasty2,
                            PrefixID.Quick2,
                            PrefixID.Wild,
                            PrefixID.Rash,
                            PrefixID.Intrepid,
                            PrefixID.Violent,
                            PrefixID.Arcane
                        };
    }
    internal class ArmorReforge : Mod
    {
        internal static ArmorReforge Instance;
        //internal Item hoveredItem;
        //internal const string ModifyArmorReforgeConfig_Permission = "ModifyArmorReforgeConfig";
        //internal const string ModifyArmorReforgeConfig_Display = "Modify ArmorReforge Config";
        public override void Load()
        {
            On.Terraria.UI.ItemSlot.MouseHover_ItemArray_int_int += ItemSlot_MouseHover_ItemArray_int_int;
            Instance = this;
        }
        public override void Unload()
        {
            Instance = null;
        }
        //public override void PostSetupContent()
        //{
        //    ModLoader.TryGetMod("HEROsMod", out Mod HEROsMod);
        //    if (HEROsMod != null)
        //    {
        //        HEROsMod.Call(
        //            "AddPermission",
        //            ModifyArmorReforgeConfig_Permission,
        //            ModifyArmorReforgeConfig_Display
        //        );
        //    }
        //}
        private void ItemSlot_MouseHover_ItemArray_int_int(On.Terraria.UI.ItemSlot.orig_MouseHover_ItemArray_int_int orig, Item[] inv, int context, int slot) // credits to jopo
        {
            orig(inv, context, slot);
            // EquipArmorVanity = 9;
            // EquipAccessoryVanity = 11;
            // EquipAccessory = 10;
            //hoveredItem = null;
            if (context == 10) // disable armor on accessory
            {
                //int socialAccessories = -1;
                //if (slot < (socialAccessories == -1 ? 18 + Main.LocalPlayer.GetAmountOfExtraAccessorySlotsToShow() : 13 + socialAccessories))
                if (Main.mouseItem.IsArmor())
                {
                    Main.mouseItem.accessory = false;
                    //hoveredItem = Main.HoverItem;
                    //Main.HoverItem.social = false;
                }
            }
            else if (context == 5) // reforge slot selected
            {
                if (Main.mouseItem.IsArmor())
                    Main.mouseItem.OverrideReforge();
            }
            //if (context == 9 && ModContent.GetInstance<ServerConfig>().SocialArmor)
            //{
            //    hoveredItem = Main.HoverItem;
            //    Main.HoverItem.social = false;
            //}
        }
        internal class ArmorReforgePlayer : ModPlayer
        {
            public override void PostUpdate()
            {
                //Main.NewText("Item: " + Main.mouseItem.Name + " Hover: " + Player.mouseInterface + " Cursor: " + Main.cursorOverride);
                if (Main.InReforgeMenu && Main.reforgeItem.IsAir && !Main.mouseItem.IsAir)
                {
                    //Main.mouseItem.OverrideReforge();
                    if (Main.mouseItem.IsArmor() && Main.mouseItem.vanity) // This allows vanity items to be used in reforging.
                    {
                        Main.mouseItem.vanity = false;
                        Main.mouseItem.canBePlacedInVanityRegardlessOfConditions = false;
                    }
                }
            }
        }
        internal class ArmorReforgeItem : GlobalItem
        {
            public override bool ReforgePrice(Item item, ref int reforgePrice, ref bool canApplyDiscount)
            {
                if (item.IsArmor())
                {
                    if (item.value < 2 || reforgePrice < 2)
                    {
                        item.value = item.ArmorRecipeValue();
                    }
                    else // Make armor reforging a bit more expensive.
                    {
                        reforgePrice = (item.ArmorRecipeValue() + item.value) / 2;

                        if (reforgePrice < 2) // Manually assign a price value based on defense if value can't be determined by recipe.
                        {
                            reforgePrice = (item.defense * 2500 * Math.Abs(item.rare + 2)) / 2;
                        }
                    }
                    if (item.value < 2 && reforgePrice < 2)
                        reforgePrice = 20000; // 2 gold
                }
                return base.ReforgePrice(item, ref reforgePrice, ref canApplyDiscount);
            }
            public override void PostReforge(Item item)
            {
                if (item.IsArmor())
                {
                    if (item.value < 2) // If there is no default assigned value for armor
                        item.value = item.ArmorRecipeValue();                    
                }
            }
            public override bool? PrefixChance(Item item, int pre, UnifiedRandom rand)
            {
                if (item.IsArmor())
                {
                    if (pre == -3)
                    {
                        return true; // Returning true when pre is -3 makes this item be placeable in reforge slot
                    }
                    if (pre == -1)
                    {
                        if (item.maxStack > 1) // no prefixes on crafted stackable items
                            return false;

                        return rand.NextBool(2); // 50% chance to get random prefix on item creation
                    }
                }
                return base.PrefixChance(item, pre, rand);
            }
            public override int ChoosePrefix(Item item, UnifiedRandom rand)
            {
                if (item.IsArmor())
                {
                    var moddedPrefixes = PrefixLoader.GetPrefixesInCategory(PrefixCategory.Accessory);
                    List<int> allowedPrefixes = new();
                    foreach (var prefix in ArmorReforgeUtils.accModifier)
                    {
                        allowedPrefixes.Add(prefix);
                    }
                    foreach (var prefix in moddedPrefixes)
                    {
                        allowedPrefixes.Add(prefix.Type);
                    }
                    return allowedPrefixes[rand.Next(allowedPrefixes.Count)];
                }
                return base.ChoosePrefix(item, rand);
            }
            public override void UpdateInventory(Item item, Player player)
            {
                //Main.NewText("Item: " + item.Name + " Value: " + item.);
                if (item.IsArmor())
                    item.accessory = false;
                //    item.OverrideReforge();
                if (item.IsArmor() && item.canBePlacedInVanityRegardlessOfConditions && item.vanity == false) // This allows vanity items to be used in reforging.
                {
                    item.vanity = true;
                    item.canBePlacedInVanityRegardlessOfConditions = false;
                }
                base.UpdateInventory(item, player);
            }
        }
    }
}