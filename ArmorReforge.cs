using Terraria;
using Terraria.ModLoader;
using System.Collections.Generic;
using System;
using Terraria.ID;

namespace ArmorReforge
{
    internal static class ArmorReforgeUtils
    {
        public static bool IsArmor(this Item item) => (item.headSlot != -1 || item.bodySlot != -1 || item.legSlot != -1); // && !item.vanity);
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
                    foreach(var ingredient in getIngredients)
                    {
                        armorValue += (ingredient.value * ingredient.stack);
                    }
                    return (int)(armorValue + (item.rare* armorValue) *0.1)/3;
                }
            }
            return item.value;
        }
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
                    if (Main.mouseItem.IsArmor() && Main.mouseItem.vanity)
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
                        reforgePrice = (item.ArmorRecipeValue() + item.value)/2;

                        if (reforgePrice < 2) // Manually assign a price value based on defense if value can't be determined by recipe.
                        {
                            reforgePrice = (item.defense * 1500 * Math.Abs(item.rare+2))/2;
                        }
                    }
                    if (item.value < 2 && reforgePrice < 2)
                        reforgePrice = 20000;
                }
                return base.ReforgePrice(item, ref reforgePrice, ref canApplyDiscount);
            }
            public override bool AllowPrefix(Item item, int pre)
            {
                return base.AllowPrefix(item, pre);
            }
            public override void PostReforge(Item item)
            {
                if (item.IsArmor())
                {
                    Random rand = new();
                    item.OverrideReforge();
                    item.RerollArmor();
                    if (item.prefix == 0) // Allow rolling of vanity modifiers (no modded support?)
                    {
                        int[] accModifier = {
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
                        int index = rand.Next(accModifier.Length);
                        item.prefix = accModifier[index];
                    }
                    if (item.value < 2) // If there is no default assigned value for armor
                        item.value = item.ArmorRecipeValue();
                }
            }
            public override bool CanEquipAccessory(Item item, Player player, int slot, bool modded)
            {
                if (item.IsArmor())
                    return false;

                return base.CanEquipAccessory(item, player, slot, modded);
            }

            public override void OnCreate(Item item, ItemCreationContext context)
            {
                if (item.IsArmor() && Main.rand.Next(2) == 0)
                {
                    item.OverrideReforge();
                    item.RerollArmor();
                }
            }

            public override void UpdateInventory(Item item, Player player)
            {
                if (item.IsArmor())
                    item.OverrideReforge();

                if (item.IsArmor() && item.canBePlacedInVanityRegardlessOfConditions && item.vanity == false)
                {
                    item.vanity = true;
                    item.canBePlacedInVanityRegardlessOfConditions = false;
                }
                //base.UpdateInventory(item, player);
            }
        }
    }
}
