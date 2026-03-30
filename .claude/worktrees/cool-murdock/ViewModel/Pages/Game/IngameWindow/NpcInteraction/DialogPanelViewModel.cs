using MyriaLib.Entities.NPCs;
using MyriaLib.Entities.Players;
using MyriaRPG.Utils;
using MyriaLib.Systems;
using System.Collections.ObjectModel;
using MyriaLib.Services;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow.NpcInteraction
{
    public class DialogPanelViewModel : BaseViewModel
    {
        private readonly Npc _npc;
        private readonly Player _player;
        private readonly Action<BaseViewModel> _onNavigate;

        private string _dialogText;
        public string DialogText
        {
            get => _dialogText;
            set { _dialogText = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ServiceOption> ServiceOptions { get; } = new();

        public DialogPanelViewModel(Npc npc, Player player, Action<BaseViewModel> onNavigate)
        {
            _npc = npc;
            _player = player;
            _onNavigate = onNavigate;

            DialogText = Localization.T("npc." + npc.Id + "dialog");

            foreach (var service in npc.Services)
            {
                ServiceOptions.Add(new ServiceOption
                {
                    Text = Localization.T($"npc.service.{service}.title"),
                    Command = new RelayCommand(() => HandleService(service))
                });

            }

        }

        public void HandleService(string serviceId)
        {
            // Healer: immediate action
            if (serviceId == "heal")
            {
                NpcActionResult res = _npc.HealingAction(UserAccoundService.CurrentCharacter);
                DialogText = Localization.T(res.MessageKey, res.MessageArgs);
                return;
            }

            // Shop panels
            if (serviceId == "buy_items" || serviceId == "shop" || serviceId == "shop_equipment")
            {
                _onNavigate(new ShopPanelViewModel(_npc, _player, BackToMe));
                return;
            }

            // Smith panels
            if (serviceId == "upgrade")
            {
                _onNavigate(new UpgradePanelViewModel(_npc, _player, BackToMe));
                return;
            }

            if (serviceId == "craft")
            {
                _onNavigate(new CraftPanelViewModel(_npc, _player, BackToMe));
                return;
            }

            // default
            DialogText = Localization.T("npc.result.unimplemented"); // add this key
        }

        private void BackToMe() => _onNavigate(this);
    }

}
