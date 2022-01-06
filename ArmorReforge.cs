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
        public static bool IsArmor(this Item item) => (item.headSlot != -1 || item.bodySlot != -1 || item.legSlot != -1);// && !item.vanity;
        public static void OverrideReforge(this Item item) => item.accessory = (item.IsArmor() && !item.accessory) || item.accessory;
        public static void RerollArmor(this Item item) => item.Prefix(-2);
        public static int ArmorRecipeValue(this Item item) // Get armor recipe value.
        {
            for (int i = 0; i < Recipe.numRecipes; i++)
            {
                if (Main.recipe[i].HasResult(item.netID))
                {
                    int armorValue = 0;
                    List<Item> getIngredients = Main.recipe[i].requiredItem;
                    foreach (var ingredient in getIngredients)
                    {
                        armorValue += (ingredient.value * ingredient.stack);
                    }
                    return (int)(armorValue + (item.rare * armorValue) * 0.1) / 3;
                }
            }
            return item.value;
        }
        public static readonly int[] accModifier = {
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
    public class ArmorReforge : Mod
    {
        internal static ArmorReforge Instance;
        //internal const string ModifyArmorReforgeConfig_Permission = "ModifyArmorReforgeConfig";
        //internal const string ModifyArmorReforgeConfig_Display = "Modify ArmorReforge Config";

        public override void Load()
        {
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
        public class ArmorReforgePlayer : ModPlayer
        {
            public override void PostUpdate()
            {
                if (Main.InReforgeMenu && Main.reforgeItem.IsAir && !Main.mouseItem.IsAir)
                {
                    Main.mouseItem.OverrideReforge();
                    if (Main.mouseItem.IsArmor() && Main.mouseItem.vanity) // This allows vanity items to be used in reforging.
                    {
                        Main.mouseItem.vanity = false;
                        Main.mouseItem.canBePlacedInVanityRegardlessOfConditions = true;
                    }
                }
            }
        }
        public class ArmorReforgeItem : GlobalItem
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
                            reforgePrice = (item.defense * 1500 * Math.Abs(item.rare + 2)) / 2;
                        }
                    }
                    if (item.value < 2 && reforgePrice < 2)
                        reforgePrice = 20000;
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
                        return rand.Next(2) == 0; // 50% chance to get random prefix on item creation
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
                if (item.IsArmor())
                    item.OverrideReforge();

                if (item.IsArmor() && item.canBePlacedInVanityRegardlessOfConditions && item.vanity == false) // This allows vanity items to be used in reforging.
                {
                    item.vanity = true;
                    item.canBePlacedInVanityRegardlessOfConditions = false;
                }
                base.UpdateInventory(item, player);
            }
            public override void OnCreate(Item item, ItemCreationContext context)
            {
                if (item.IsArmor() && Main.rand.Next(2) == 0)
                {
                    item.OverrideReforge();
                    item.RerollArmor();
                }
            }
        }
    }
}
