using MyriaLib.Entities.NPCs;
using MyriaLib.Entities.Players;
using MyriaLib.Services;
using MyriaLib.Systems;
using MyriaRPG.Utils;
using MyriaRPG.ViewModel.Pages.Game.IngameWindow.NpcInteraction;
using System.Collections.ObjectModel;

namespace MyriaRPG.ViewModel.UserControls.IngameWindow
{
    public class DialogPanelViewModel : BaseViewModel
    {
        private readonly Npc _npc;
        private readonly Player _player;
        private readonly Action<string> _onNavigate;

        private string _dialogText;
        public string DialogText
        {
            get => _dialogText;
            set { _dialogText = value; OnPropertyChanged(); }
        }

        public ObservableCollection<ServiceOption> ServiceOptions { get; } = new();

        public DialogPanelViewModel(Npc npc, Player player, Action<string> onNavigate)
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
                    Description = Localization.T($"npc.service.{service}.desc"),
                    Command = new RelayCommand(() => HandleService(service))
                });
            }
        }

        private void HandleService(string serviceId)
        {
            if (serviceId == "heal")
            {
                NpcActionResult res = _npc.HealingAction(UserAccoundService.CurrentCharacter);
                DialogText = Localization.T(res.MessageKey, res.MessageArgs);
                return;
            }

            _onNavigate(serviceId);
        }
    }
}
