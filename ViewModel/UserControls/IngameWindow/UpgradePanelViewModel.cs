using MyriaLib.Entities.Items;
using MyriaLib.Entities.NPCs;
using MyriaLib.Entities.Players;
using MyriaLib.Systems;
using MyriaRPG.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.UserControls.IngameWindow
{
    public class UpgradePanelViewModel : BaseViewModel
    {
        private readonly Npc _npc;
        private readonly Player _player;
        private readonly Action _goBack;

        public string Title => Localization.T("npc.upgrade.title");
        public string BtnBack => Localization.T("app.general.UI.back");
        public string BtnUpgrade => Localization.T("npc.upgrade.button");
        public string CostLabel => Localization.T("npc.upgrade.cost");

        public ObservableCollection<EquipmentItemVm> Equipment { get; } = new();

        private EquipmentItemVm _selectedEquipment;
        public EquipmentItemVm SelectedEquipment
        {
            get => _selectedEquipment;
            set
            {
                _selectedEquipment = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedName));
                OnPropertyChanged(nameof(SelectedLevel));
                OnPropertyChanged(nameof(UpgradeCost));
                OnPropertyChanged(nameof(CanUpgrade));
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        public string SelectedName => SelectedEquipment?.Name ?? "";
        public string SelectedLevel => SelectedEquipment != null ? $"{Localization.T("npc.upgrade.level")}: +{SelectedEquipment.UpgradeLevel}" : "";
        public string UpgradeCost => SelectedEquipment != null ? $"{CostLabel}: {SelectedEquipment.UpgradeCost}" : "";

        private string _statusMessage = "";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public bool CanUpgrade => SelectedEquipment != null && _player.Money.Coins.TotalBronze >= SelectedEquipment.UpgradeCost;

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
            StatusMessage = "";

            foreach (var eq in _player.Inventory.Items.OfType<EquipmentItem>())
            {
                Equipment.Add(new EquipmentItemVm(eq));
            }

            if (Equipment.Count == 0)
            {
                StatusMessage = Localization.T("npc.upgrade.noEquipment");
                return;
            }

            SelectedEquipment = Equipment.FirstOrDefault();
        }

        private void UpgradeSelected()
        {
            if (SelectedEquipment == null) return;

            // Find the actual item in inventory
            EquipmentItem eq = _player.Inventory.Items
                .OfType<EquipmentItem>()
                .FirstOrDefault(i => i.Id == SelectedEquipment.Id);

            if (eq == null)
            {
                StatusMessage = Localization.T("npc.upgrade.fail");
                return;
            }

            // Check cost
            if (_player.Money.Coins.TotalBronze < SelectedEquipment.UpgradeCost)
            {
                StatusMessage = Localization.T("npc.upgrade.notEnoughMoney");
                return;
            }

            // Perform upgrade
            NpcActionResult result = _npc.UpgradeItem(_player, eq);

            if (result.Success)
            {
                StatusMessage = Localization.T(result.MessageKey, result.MessageArgs);
                LoadUpgradable(); // Refresh list
            }
            else
            {
                StatusMessage = Localization.T(result.MessageKey, result.MessageArgs);
            }
        }
    }

    public class EquipmentItemVm : BaseViewModel
    {
        private EquipmentItem _item;

        public string Id => _item.Id;
        public string Name => Localization.T($"item.{_item.Id}");
        public int UpgradeLevel => _item.UpgradeLevel;
        public int UpgradeCost => 100 * (UpgradeLevel + 1); // Simple formula: 100, 200, 300, etc.

        public EquipmentItemVm(EquipmentItem item)
        {
            _item = item;
        }

        public override string ToString() => Name;
    }
}
