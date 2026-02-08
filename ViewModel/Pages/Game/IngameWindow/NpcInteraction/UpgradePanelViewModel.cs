using MyriaLib.Entities.Items;
using MyriaLib.Entities.NPCs;
using MyriaLib.Entities.Players;
using MyriaLib.Systems;
using MyriaRPG.Model;
using MyriaRPG.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow.NpcInteraction
{
    public class UpgradePanelViewModel : BaseViewModel
    {
        private readonly Npc _npc;
        private readonly Player _player;
        private readonly Action _goBack;

        public string Title => Localization.T("npc.upgrade.title");
        public string BtnBack => Localization.T("app.general.UI.back");
        public string BtnUpgrade => Localization.T("npc.upgrade.upgrade");

        public ObservableCollection<ItemVm> Equipment { get; } = new();

        private ItemVm _selectedEquipment;
        public ItemVm SelectedEquipment
        {
            get => _selectedEquipment;
            set
            {
                _selectedEquipment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedName));
                OnPropertyChanged(nameof(CanUpgrade));
            }

        }

        public string SelectedName => Localization.T(SelectedEquipment?.Name) ?? "";

        public bool CanUpgrade => SelectedEquipment?.IsEquipable ?? false;

        public ICommand BackCommand { get; }
        public ICommand UpgradeCommand { get; }

        public UpgradePanelViewModel(Npc npc, Player player, Action goBack)
        {
            _npc = npc;
            _player = player;
            _goBack = goBack;

            BackCommand = new RelayCommand(_goBack);
            UpgradeCommand = new RelayCommand(UpgradeSelected);

            LoadUpgradable();
        }

        private void LoadUpgradable()
        {
            Equipment.Clear();

            foreach (var eq in _player.Inventory.Items.OfType<EquipmentItem>())
                Equipment.Add(new ItemVm(eq));

            SelectedEquipment = Equipment.FirstOrDefault();
        }

        private void UpgradeSelected()
        {
            if (SelectedEquipment == null || !SelectedEquipment!.IsEquipable) return;

            EquipmentItem eq = _player.Inventory.Items.Where(i => i.Id == SelectedEquipment.Id).FirstOrDefault() as EquipmentItem;
            if (eq == null)
                return;

            NpcActionResult result = _npc.UpgradeItem(_player, eq);

            // refresh list
            LoadUpgradable();
        }

    }

}
