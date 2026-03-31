using MyriaLib.Entities.Items;
using MyriaLib.Systems.Enums;
using MyriaLib.Systems;
using MyriaRPG.ViewModel.UserControls.IngameWindow;

namespace MyriaRPG.Model
{
    public class ShopEquipmentItemVm : ShopItemVm
    {
        public EquipmentType SlotType { get; set; }
        public List<PlayerClass> AllowedClasses { get; set; }
        public bool IsTool { get; set; }

        public static ShopEquipmentItemVm FromEquipment(EquipmentItem item)
        {
            return new ShopEquipmentItemVm
            {
                Id = item.Id,
                Name = Localization.T($"item.{item.Id}"),
                BuyPrice = item.BuyPrice,
                SlotType = item.SlotType,
                AllowedClasses = item.AllowedClasses,
                IsTool = item.IsTool
            };
        }
    }
}
