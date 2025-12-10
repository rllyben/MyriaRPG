using MyriaRPG.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow
{
    public class QuestListPageViewModel : BaseViewModel
    {
        // Mode
        private bool _showActive = true;
        public bool ShowActive
        {
            get => _showActive;
            set
            {
                if (_showActive == value) return;
                _showActive = value;
                if (value) ShowAvailable = false;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsShowingActive));
                OnPropertyChanged(nameof(IsShowingAvailable));
                UpdateMode();
            }
        }

        private bool _showAvailable;
        public bool ShowAvailable
        {
            get => _showAvailable;
            set
            {
                if (_showAvailable == value) return;
                _showAvailable = value;
                if (value) ShowActive = false;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsShowingActive));
                OnPropertyChanged(nameof(IsShowingAvailable));
                UpdateMode();
            }
        }

        public bool IsShowingActive => ShowActive;
        public bool IsShowingAvailable => ShowAvailable;

        public string HeaderSuffix => IsShowingActive ? "(Active)" : "(Available)";

        // Data
        public ObservableCollection<QuestListItemVm> Quests { get; } = new();
        private QuestListItemVm? _selectedQuest;
        public QuestListItemVm? SelectedQuest
        {
            get => _selectedQuest;
            set { _selectedQuest = value; OnPropertyChanged(); }
        }

        // Commands (logic later)
        public ICommand AcceptQuestCommand { get; }
        public ICommand AbandonQuestCommand { get; }
        public ICommand TrackQuestCommand { get; }

        // Optional: used by your in-game window title binding
        public string WindowTitle => "Quests";

        public QuestListPageViewModel()
        {
            // Commands: for now, no real logic; just stubs
            AcceptQuestCommand = new RelayCommand<QuestListItemVm?>(AcceptQuest);
            AbandonQuestCommand = new RelayCommand<QuestListItemVm?>(AbandonQuest);
            TrackQuestCommand = new RelayCommand<QuestListItemVm?>(TrackQuest);

            // Default mode
            ShowActive = true;

            // TEMP: demo quests (replace with real data later)
            Quests.Add(new QuestListItemVm
            {
                Id = "q_wolves",
                Title = "Culling the Wolves",
                Level = 5,
                AreaName = "Cera Valley",
                ShortDescription = "Thin the wolf packs near the valley entrance.",
                Description = "The guards have complained about aggressive wolves attacking travelers. " +
                              "Cull their numbers so the road becomes safer.",
                ProgressText = "3 / 10 wolves slain",
                Objectives = new[]
                {
                    "Defeat 10 Valley Wolves",
                    "Return to Guard Captain Ren"
                },
                Rewards = new[]
                {
                    "250 EXP",
                    "35 Silver",
                    "Leather Vest"
                }
            });

            Quests.Add(new QuestListItemVm
            {
                Id = "q_flowers",
                Title = "Healing Petals",
                Level = 4,
                AreaName = "Lumina Fields",
                ShortDescription = "Collect glowing petals for the healer.",
                Description = "The healer in Lumina needs fresh glowing petals to brew a potent salve.",
                ProgressText = "0 / 5 petals collected",
                Objectives = new[]
                {
                    "Collect 5 Glowing Petals",
                    "Bring them to the Healer in Lumina"
                },
                Rewards = new[]
                {
                    "120 EXP",
                    "Small Healing Potion x3"
                }
            });

            SelectedQuest = Quests.FirstOrDefault();
        }

        private void UpdateMode()
        {
            // TODO: later:
            // - When ShowActive: load active quests from MyriaLib
            // - When ShowAvailable: load available quests for the current area/player level
            // For now, keep the same demo list.
        }

        private void AcceptQuest(QuestListItemVm? quest)
        {
            if (quest == null) return;
            // TODO: call into MyriaLib quest system to accept
        }

        private void AbandonQuest(QuestListItemVm? quest)
        {
            if (quest == null) return;
            // TODO: call into MyriaLib quest system to abandon/remove
        }

        private void TrackQuest(QuestListItemVm? quest)
        {
            if (quest == null) return;
            // TODO: mark as 'tracked' so it can show in HUD/log
        }
    }

    public class QuestListItemVm : BaseViewModel
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public int Level { get; set; }
        public string AreaName { get; set; } = "";

        public string ShortDescription { get; set; } = "";
        public string Description { get; set; } = "";
        public string ProgressText { get; set; } = "";

        public IEnumerable<string> Objectives { get; set; } = Array.Empty<string>();
        public IEnumerable<string> Rewards { get; set; } = Array.Empty<string>();
    }
}
