using MyriaLib.Entities.Players;
using MyriaLib.Entities.Skills;
using MyriaRPG.Utils;
using MyriaRPG.ViewModel.Windows;
using MyriaRPG.View.Windows;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow
{
    public class SkillPageViewModel : BaseViewModel
    {
        public ObservableCollection<SkillVm> Skills { get; } = new();

        public ICommand OpenDetailsCommand { get; }

        public string WindowTitle => "Skills";

        public string HeaderText => $"{Skills.Count} learned";

        public SkillPageViewModel(Player player, IEnumerable<Skill> skills)
        {
            foreach (var s in skills)
            {
                Skills.Add(new SkillVm(s));
            }

            OpenDetailsCommand = new RelayCommand<SkillVm?>(OpenDetails);
        }

        private void OpenDetails(SkillVm? skill)
        {
            if (skill == null) return;

            // open a detail window
            var win = new SkillDetailWindow
            {
                DataContext = new SkillDetailViewModel(skill)
            };
            win.Owner = System.Windows.Application.Current.MainWindow;
            win.ShowDialog();
        }
    }

    public class SkillVm : BaseViewModel
    {
        private readonly Skill _skill;

        public SkillVm(Skill skill)
        {
            _skill = skill;
        }

        public string Id => _skill.Id;
        public string Name => _skill.Name;
        public string Description => _skill.Description;
        public int ManaCost => _skill.ManaCost;
        public int MinLevel => _skill.MinLevel;
        public bool IsHealing => _skill.IsHealing;

        public string ClassName => _skill.Class.ToString();
        public string TypeText => _skill.Type.ToString();   // Physical / Magical
        public string TargetText => _skill.Target switch       // SingleEnemy / AllEnemies / Self
        {
            SkillTarget.SingleEnemy => "Single enemy",
            SkillTarget.AllEnemies => "All enemies",
            SkillTarget.Self => "Self",
            _ => _skill.Target.ToString()
        };

        public string ShortDescription =>
            string.IsNullOrWhiteSpace(_skill.Description)
                ? "No description."
                : _skill.Description.Length > 120
                    ? _skill.Description[..120] + "..."
                    : _skill.Description;

        public string TimingText
        {
            get
            {
                if (_skill.CastTime == 0 && _skill.RecoveryTime == 0) return "Instant";
                if (_skill.CastTime > 0 && _skill.RecoveryTime == 0) return $"Cast: {_skill.CastTime}t";
                if (_skill.CastTime == 0 && _skill.RecoveryTime > 0) return $"Recovery: {_skill.RecoveryTime}t";
                return $"Cast: {_skill.CastTime}t, Rec: {_skill.RecoveryTime}t";
            }
        }

        public float ScalingFactor => _skill.ScalingFactor;
        public string StatToScaleFrom => _skill.StatToScaleFrom;
    }
}
