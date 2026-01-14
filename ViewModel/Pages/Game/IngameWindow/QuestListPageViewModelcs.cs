using MyriaLib.Entities.NPCs;
using MyriaLib.Entities.Players;
using MyriaLib.Services;
using MyriaLib.Services.Formatter;
using MyriaLib.Services.Manager;
using MyriaLib.Systems;
using MyriaLib.Systems.Enums;
using MyriaRPG.Model;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages.Game.IngameWindow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow
{
    public class QuestListPageViewModel : BaseViewModel
    {
        private string tbl_Title;
        private string tbl_Info;
        private string tbl_Level;
        private string tbl_Description;
        private string tbl_Objectives;
        private string tbl_Rewards;
        private string btn_Track;
        private string btn_Abandon;
        private string btn_Accept;
        private string btn_Active;
        private string btn_Available;
        [LocalizedKey("pg.quests.title")]
        public string TblTitle
        {
            get { return tbl_Title; }
            set
            {
                tbl_Title = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.quests.details.selecthint")]
        public string TblInfo
        {
            get { return tbl_Info; }
            set
            {
                tbl_Info = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.level")]
        public string TblLevel
        {
            get { return tbl_Level; }
            set
            {
                tbl_Level = value + " ";
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.quests.section.description")]
        public string TblDescription
        {
            get { return tbl_Description; }
            set
            {
                tbl_Description = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.quests.section.objectives")]
        public string TblObjectives
        {
            get { return tbl_Objectives; }
            set
            {
                tbl_Objectives = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.quests.section.rewards")]
        public string TblRewards
        {
            get { return tbl_Rewards; }
            set
            {
                tbl_Rewards = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.quests.btn.return")]
        public string BtnTrack
        {
            get { return btn_Track; }
            set
            {
                btn_Track = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.quests.btn.abandon")]
        public string BtnAbandon
        {
            get { return btn_Abandon; }
            set
            {
                btn_Abandon = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.quests.btn.accept")]
        public string BtnAccept
        {
            get { return btn_Accept; }
            set
            {
                btn_Accept = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.quests.tab.active")]
        public string BtnActive
        {
            get { return btn_Active; }
            set
            {
                btn_Active = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.quests.tab.available")]
        public string BtnAvailable
        {
            get { return btn_Available; }
            set
            {
                btn_Available = value;
                OnPropertyChanged();
            }

        }
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

        public string HeaderSuffix => IsShowingActive ? $"({MyriaLib.Systems.Localization.T("pg.quests.tab.active")})" : $"({MyriaLib.Systems.Localization.T("pg.quests.tab.available")})";

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
                    HasSelected = Visibility.Visible; 
                else 
                    HasSelected = Visibility.Hidden; 
                OnPropertyChanged(); 
            }

        }
        private Visibility _hasSelected = Visibility.Visible;
        public Visibility HasSelected
        {
            get => _hasSelected;
            set {_hasSelected = value; OnPropertyChanged(); }
        }
        // Commands (logic later)
        public ICommand AcceptQuestCommand { get; }
        public ICommand AbandonQuestCommand { get; }
        public ICommand CompleteQuestCommand { get; }

        // Optional: used by your in-game window title binding
        public string WindowTitle
        {
            get => _windowTitle;
            set { _windowTitle = value; OnPropertyChanged(); }
        }
        private string _windowTitle = MyriaLib.Systems.Localization.T("pg.quests.title");

        public QuestListPageViewModel()
        {
            // Commands: for now, no real logic; just stubs
            AcceptQuestCommand = new RelayCommand<QuestListItemVm?>(AcceptQuest);
            AbandonQuestCommand = new RelayCommand<QuestListItemVm?>(AbandonQuest);

            // Default mode
            ShowActive = true;

            foreach (Quest quest in _player.ActiveQuests)
            {
                if (quest.Status == QuestStatus.Returned)
                    continue;
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
            CompleteQuestCommand = new RelayCommand<QuestListItemVm?>(CompleteQuest, a => CanReturnSelectedQuest());
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
        private bool CanReturnSelectedQuest()
        {
            if (SelectedQuest == null)
                return false;
            else if (!IsShowingActive)
                return false;
            return _player.ActiveQuests.Where(b => b.Id == SelectedQuest.Id).First().Status == QuestStatus.Completed;
        }
        private void AcceptQuest(QuestListItemVm? quest)
        {
            if (quest == null) 
                return;
            if (_player.ActiveQuests.Any(a => a.Id == quest.Id))
                return;
            _player.ActiveQuests.Add(QuestManager.GetQuestById(quest.Id));
            Quests.Remove(quest);
        }

        private void AbandonQuest(QuestListItemVm? quest)
        {
            if (quest == null) 
                return;
            if (_player.ActiveQuests.Any(a => a.Id != quest.Id))
                return;
            Quest playQuest = _player.ActiveQuests.Where(a => a.Id == quest.Id).First();
            _player.ActiveQuests.Remove(playQuest);
            Quests.Remove(quest);
        }

        private void CompleteQuest(QuestListItemVm? quest)
        {
            if (quest == null) return;
            Quest playQuest = _player.ActiveQuests.Where(a => a.Id == quest.Id).First();
            playQuest.GrantRewards(_player);
            playQuest.Status = QuestStatus.Returned;
            _player.CompletedQuests.Add(playQuest);
            _player.ActiveQuests.Remove(playQuest);
            Quests.Remove(quest);
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
