using AresMod.Utils;
using ProjectM;
using Wetstone.API;
using Unity.Entities;

namespace AresMod.Commands
{
    [Command("inventory, i", Usage = "inventory", Description = "Clears everything except weapons and equipped armor from inventory.")]
    public static class InventoryClear
    {
        public static void Initialize(Context ctx)
        {
            var player = ctx.Event.SenderCharacterEntity;
            InventoryUtilities.TryGetInventoryEntity(ctx.EntityManager, player, out Entity playerInventory);

            for (int i = 9; i < InventoryUtilities.GetItemSlots(ctx.EntityManager, playerInventory); i++)
            {
                InventoryUtilitiesServer.ClearSlot(VWorld.Server.EntityManager, playerInventory, i);
            }
            ctx.Event.User.SendSystemMessage($"<color=#ffff00ff>Inventory cleared!</color>");
        }
    }
}