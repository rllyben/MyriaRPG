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
    public class SkillSlotViewModel : BaseViewModel
    {
        [LocalizedKey("pg.skill_slots.title")]
        public string TblTitle { get; set; }

        [LocalizedKey("pg.skill_slots.available")]
        public string TblAvailable { get; set; }

        [LocalizedKey("pg.skill_slots.active")]
        public string TblActiveSlots { get; set; }

        [LocalizedKey("pg.skill_slots.slot")]
        public string TblSlot { get; set; }

        [LocalizedKey("pg.skill_slots.unslot")]
        public string TblUnslot { get; set; }

        [LocalizedKey("pg.skill_slots.regular")]
        public string TblRegular { get; set; }

        [LocalizedKey("pg.skill_slots.combined")]
        public string TblCombined { get; set; }

        [LocalizedKey("pg.skill_slots.fusion")]
        public string TblFusion { get; set; }

        [LocalizedKey("app.general.UI.back")]
        public string TblBack { get; set; }

        public string SlotCountText => $"{_player.SkillSlots.Count} / {_player.SkillSlotCount} slots";

        public bool HasCombinedSkills => AvailableCombinedSkills.Count > 0;
        public bool HasFusionSkills => AvailableFusionSkills.Count > 0;

        public ObservableCollection<SlottableSkillVm> AvailableRegularSkills { get; } = new();
        public ObservableCollection<SlottableSkillVm> AvailableCombinedSkills { get; } = new();
        public ObservableCollection<SlottableSkillVm> AvailableFusionSkills { get; } = new();
        public ObservableCollection<ActiveSlotVm> ActiveSlots { get; } = new();

        public ICommand SlotSkillCommand { get; }
        public ICommand UnslotSkillCommand { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand GoBackCommand { get; }

        private readonly MyriaLib.Entities.Players.Player _player;

        public SkillSlotViewModel()
        {
            _player = UserAccoundService.CurrentCharacter;

            SlotSkillCommand = new RelayCommand<SlottableSkillVm?>(SlotSkill);
            UnslotSkillCommand = new RelayCommand<ActiveSlotVm?>(UnslotSkill);
            MoveUpCommand = new RelayCommand<ActiveSlotVm?>(MoveUp);
            MoveDownCommand = new RelayCommand<ActiveSlotVm?>(MoveDown);
            GoBackCommand = new RelayCommand(() => Navigation.NavigateIngameWindow(new Page_Skills()));

            Refresh();
        }

        private void Refresh()
        {
            AvailableRegularSkills.Clear();
            AvailableCombinedSkills.Clear();
            AvailableFusionSkills.Clear();
            ActiveSlots.Clear();

            bool atCap = _player.SkillSlots.Count >= _player.SkillSlotCount;

            foreach (var s in _player.Skills)
            {
                bool slotted = _player.SkillSlots.Any(sl => sl.Source == SlottedSkillSource.Regular && sl.SkillId == s.Id);
                AvailableRegularSkills.Add(new SlottableSkillVm(s.Id, s.Name, s.Type.ToString(), s.Target.ToString(),
                    SlottedSkillSource.Regular, slotted, atCap));
            }

            foreach (var c in _player.CombinedSkills.Where(c => c.ResolvedSkill != null))
            {
                bool slotted = _player.SkillSlots.Any(sl => sl.Source == SlottedSkillSource.Combined && sl.SkillId == c.Id);
                var sk = c.ResolvedSkill!;
                AvailableCombinedSkills.Add(new SlottableSkillVm(c.Id, c.DisplayName, sk.Type.ToString(), sk.Target.ToString(),
                    SlottedSkillSource.Combined, slotted, atCap));
            }

            foreach (var f in _player.CompositeSkills.Where(f => f.ResolvedSkill != null))
            {
                bool slotted = _player.SkillSlots.Any(sl => sl.Source == SlottedSkillSource.CompositeFusion && sl.SkillId == f.Id);
                var sk = f.ResolvedSkill!;
                AvailableFusionSkills.Add(new SlottableSkillVm(f.Id, f.DisplayName, sk.Type.ToString(), sk.Target.ToString(),
                    SlottedSkillSource.CompositeFusion, slotted, atCap));
            }

            int idx = 1;
            foreach (var slot in _player.SkillSlots)
                ActiveSlots.Add(new ActiveSlotVm(idx++, slot));

            OnPropertyChanged(nameof(SlotCountText));
            OnPropertyChanged(nameof(HasCombinedSkills));
            OnPropertyChanged(nameof(HasFusionSkills));
        }

        private void SlotSkill(SlottableSkillVm? vm)
        {
            if (vm == null) return;
            SkillSlotService.TryAddSlot(_player, vm.Source, vm.SkillId);
            CharacterService.SaveCharacter(UserAccoundService.CurrentUser, _player);
            Refresh();
        }

        private void UnslotSkill(ActiveSlotVm? vm)
        {
            if (vm == null) return;
            SkillSlotService.RemoveSlot(_player, vm.Source, vm.SkillId);
            CharacterService.SaveCharacter(UserAccoundService.CurrentUser, _player);
            Refresh();
        }

        private void MoveUp(ActiveSlotVm? vm)
        {
            if (vm == null) return;
            int idx = _player.SkillSlots.FindIndex(s => s.Source == vm.Source && s.SkillId == vm.SkillId);
            SkillSlotService.ReorderSlots(_player, idx, idx - 1);
            CharacterService.SaveCharacter(UserAccoundService.CurrentUser, _player);
            Refresh();
        }

        private void MoveDown(ActiveSlotVm? vm)
        {
            if (vm == null) return;
            int idx = _player.SkillSlots.FindIndex(s => s.Source == vm.Source && s.SkillId == vm.SkillId);
            SkillSlotService.ReorderSlots(_player, idx, idx + 1);
            CharacterService.SaveCharacter(UserAccoundService.CurrentUser, _player);
            Refresh();
        }
    }

    public class SlottableSkillVm
    {
        public string SkillId { get; }
        public string Name { get; }
        public string TypeAndTarget { get; }
        public SlottedSkillSource Source { get; }
        public bool IsSlotted { get; }
        public bool CanSlot { get; }

        public SlottableSkillVm(string id, string name, string type, string target,
            SlottedSkillSource source, bool slotted, bool atCap)
        {
            SkillId = id;
            Name = name;
            TypeAndTarget = $"{type} · {target}";
            Source = source;
            IsSlotted = slotted;
            CanSlot = !slotted && !atCap;
        }
    }

    public class ActiveSlotVm
    {
        private readonly SkillSlot _slot;

        public int SlotNumber { get; }
        public string SkillId => _slot.SkillId;
        public SlottedSkillSource Source => _slot.Source;
        public string SkillName => _slot.ResolvedSkill?.Name ?? _slot.SkillId;
        public string SourceTag => _slot.Source switch
        {
            SlottedSkillSource.Combined => "Combined",
            SlottedSkillSource.CompositeFusion => "Fusion",
            _ => ""
        };
        public bool HasSourceTag => !string.IsNullOrEmpty(SourceTag);

        public ActiveSlotVm(int number, SkillSlot slot)
        {
            SlotNumber = number;
            _slot = slot;
        }
    }
}
