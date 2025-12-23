using MyriaLib.Entities.NPCs;
using MyriaLib.Entities.Players;
using MyriaLib.Services;
using MyriaLib.Services.Formatter;
using MyriaLib.Services.Manager;
using MyriaLib.Systems.Enums;
using MyriaRPG.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow
{
    public class QuestListPageViewModel : BaseViewModel
    {
        private Player _player = UserAccoundService.CurrentCharacter;
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
            set 
            { 
                _selectedQuest = value; 
                if (_selectedQuest != null) 
                    HasSelected = true; 
                else 
                    HasSelected = false; 
                OnPropertyChanged(); 
            }

        }
        private bool _hasSelected;
        public bool HasSelected
        {
            get => _hasSelected;
            set { _hasSelected = value; OnPropertyChanged(); }
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

            foreach (Quest quest in _player.ActiveQuests)
            {
                QuestListItemVm temp = new QuestListItemVm()
                {
                    Id = quest.Id,
                    Title = quest.Name,
                    Level = quest.RequiredLevel,
                    ShortDescription = quest.Description,
                    Description = quest.Description,
                    ProgressText = QuestFormatter.BuildProgressText(quest)
                };
                List<string> AllObjectives = new List<string>();
                foreach(string line in QuestFormatter.BuildItemsObjectivesLine(quest))
                {
                    AllObjectives.Add(line);
                }
                foreach(string line in QuestFormatter.BuildKillsObjectiveLine(quest))
                {
                    AllObjectives.Add(line);
                }
                temp.Objectives = AllObjectives;
                temp.Rewards = QuestFormatter.BuildRewardsLine(quest);
                Quests.Add(temp);
            }
            SelectedQuest = Quests.FirstOrDefault();
        }

        private void UpdateMode()
        {
            Quests.Clear();

            if (IsShowingActive)
            {
                foreach (var q in _player.ActiveQuests
                             /*.Where(q => q.Status != QuestStatus.Completed)*/)
                {
                    Quests.Add(ToVm(q));
                }

            }
            else
            {
                var all = QuestManager.GetAvailableForPlayer(_player);

                var activeIds = new HashSet<string>(_player.ActiveQuests.Select(q => q.Id));
                foreach (var q in all.Where(q => !activeIds.Contains(q.Id)))
                {
                    // later: filter by level/area/etc.
                    Quests.Add(ToVm(q));
                }

            }

            SelectedQuest = Quests.FirstOrDefault();
            OnPropertyChanged(nameof(HeaderSuffix));
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

        private QuestListItemVm ToVm(Quest quest) => new QuestListItemVm
        {
            Id = quest.Id,
            Title = quest.Name,
            Level = quest.RequiredLevel,
            AreaName = quest.GiverNpc ?? "Unknown",
            ShortDescription = quest.Description,
            Description = quest.Description,
            ProgressText = QuestFormatter.BuildProgressText(quest)
        };

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
