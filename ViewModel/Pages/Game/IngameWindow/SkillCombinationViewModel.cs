using MyriaLib.Entities.Skills;
using MyriaLib.Services;
using MyriaLib.Systems;
using MyriaRPG.Model;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages.Game.IngameWindow;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow
{
    public class SkillCombinationViewModel : BaseViewModel
    {
        private const int MaxInputSlots = 5;

        [LocalizedKey("pg.skill_combo.title")]
        public string TblTitle { get; set; }

        [LocalizedKey("pg.skill_combo.available")]
        public string TblAvailable { get; set; }

        [LocalizedKey("pg.skill_combo.input_slots")]
        public string TblInputSlots { get; set; }

        [LocalizedKey("pg.skill_combo.created")]
        public string TblCreated { get; set; }

        [LocalizedKey("pg.skill_combo.combine")]
        public string TblCombine { get; set; }

        [LocalizedKey("pg.skill_combo.add_to_slot")]
        public string TblAddToSlot { get; set; }

        [LocalizedKey("app.general.UI.back")]
        public string TblBack { get; set; }

        public string HeaderText => $"{CombinedSkills.Count} combined";
        public string InputCountText => $"{FilledSlotCount} / {MaxInputSlots} inputs selected";
        public bool CanAddMore => FilledSlotCount < MaxInputSlots;
        public bool CanCombine => FilledSlotCount >= 2;

        private string _statusText = "";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasStatus)); }
        }
        public bool HasStatus => !string.IsNullOrEmpty(_statusText);

        public ObservableCollection<SkillVm> AvailableSkills { get; } = new();
        public ObservableCollection<InputSlotVm> InputSlots { get; } = new();
        public ObservableCollection<CombinedSkillVm> CombinedSkills { get; } = new();

        public ICommand AddToInputCommand { get; }
        public ICommand RemoveInputCommand { get; }
        public ICommand CombineCommand { get; }
        public ICommand GoBackCommand { get; }

        private int FilledSlotCount => InputSlots.Count(s => s.IsSet);

        public SkillCombinationViewModel()
        {
            var player = UserAccoundService.CurrentCharacter;

            foreach (var s in player.Skills)
                AvailableSkills.Add(new SkillVm(s));

            for (int i = 0; i < MaxInputSlots; i++)
                InputSlots.Add(new InputSlotVm(i + 1));

            foreach (var c in player.CombinedSkills)
                CombinedSkills.Add(new CombinedSkillVm(c, player.Skills));

            AddToInputCommand = new RelayCommand<SkillVm?>(AddToInput);
            RemoveInputCommand = new RelayCommand<InputSlotVm?>(RemoveInput);
            CombineCommand = new RelayCommand(Combine);
            GoBackCommand = new RelayCommand(() => Navigation.NavigateIngameWindow(new Page_Skills()));
        }

        private void AddToInput(SkillVm? vm)
        {
            if (vm == null) return;
            var empty = InputSlots.FirstOrDefault(s => !s.IsSet);
            if (empty == null) return;

            empty.Set(vm.Id, vm.Name);
            RaiseInputChanged();
            StatusText = "";
        }

        private void RemoveInput(InputSlotVm? slot)
        {
            if (slot == null) return;
            slot.Clear();
            RaiseInputChanged();
            StatusText = "";
        }

        private void Combine()
        {
            var ids = InputSlots.Where(s => s.IsSet).Select(s => s.SkillId!).ToList();
            if (ids.Count < 2) return;

            var player = UserAccoundService.CurrentCharacter;
            var result = SkillCombinationService.TryCreateForPlayer(player, ids);

            if (result == null)
            {
                StatusText = Localization.T("pg.skill_combo.duplicate");
                return;
            }

            CharacterService.SaveCharacter(UserAccoundService.CurrentUser, player);

            CombinedSkills.Add(new CombinedSkillVm(result, player.Skills));
            foreach (var slot in InputSlots) slot.Clear();
            RaiseInputChanged();
            OnPropertyChanged(nameof(HeaderText));
            StatusText = Localization.T("pg.skill_combo.success", result.DisplayName);
        }

        private void RaiseInputChanged()
        {
            OnPropertyChanged(nameof(CanAddMore));
            OnPropertyChanged(nameof(CanCombine));
            OnPropertyChanged(nameof(InputCountText));
            OnPropertyChanged(nameof(FilledSlotCount));
        }
    }

    public class InputSlotVm : BaseViewModel
    {
        public int SlotNumber { get; }

        private string? _skillId;
        private string _label;

        public string? SkillId => _skillId;
        public string Label => _label;
        public bool IsSet => _skillId != null;

        public InputSlotVm(int number)
        {
            SlotNumber = number;
            _label = $"— Slot {number} —";
        }

        public void Set(string id, string name)
        {
            _skillId = id;
            _label = name;
            OnPropertyChanged(nameof(SkillId));
            OnPropertyChanged(nameof(Label));
            OnPropertyChanged(nameof(IsSet));
        }

        public void Clear()
        {
            _skillId = null;
            _label = $"— Slot {SlotNumber} —";
            OnPropertyChanged(nameof(SkillId));
            OnPropertyChanged(nameof(Label));
            OnPropertyChanged(nameof(IsSet));
        }
    }

    public class CombinedSkillVm
    {
        private readonly CombinedSkill _combined;

        public string DisplayName => _combined.DisplayName;
        public string ComponentsText { get; }

        public CombinedSkillVm(CombinedSkill combined, IEnumerable<Skill> playerSkills)
        {
            _combined = combined;
            var nameMap = playerSkills.ToDictionary(s => s.Id, s => s.Name);
            var names = combined.SkillIds
                .Select(id => nameMap.TryGetValue(id, out var n) ? n : id);
            ComponentsText = string.Join(" + ", names);
        }
    }
}
