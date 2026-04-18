using MyriaLib.Entities.Monsters;
using MyriaLib.Entities.Players;
using MyriaLib.Entities.Skills;
using MyriaLib.Services;
using MyriaLib.Systems;
using MyriaLib.Systems.Enums;
using MyriaLib.Systems.Events;
using MyriaRPG.Model;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game
{
    public class ViewModel_PageFight : BaseViewModel
    {
        private readonly CombatEncounter _encounter;

        // Localized UI labels
        [LocalizedKey("pg.fight.title")]
        public string Title { get; set; }

        [LocalizedKey("pg.fight.btn.attack")]
        public string BtnAttack { get; set; }

        [LocalizedKey("pg.fight.btn.run")]
        public string BtnRun { get; set; }

        [LocalizedKey("pg.fight.skills")]
        public string SkillsLabel { get; set; }

        // Enemy / player display
        public string ActiveMonsterName => _encounter.Enemy.Name;
        public int PlayerHp => _encounter.Player.CurrentHealth;
        public int PlayerHpMax => _encounter.Player.MaxHealth;
        public int PlayerMp => _encounter.Player.CurrentMana;
        public int PlayerMpMax => _encounter.Player.MaxMana;

        public int EnemyHp => _encounter.Enemy.CurrentHealth;
        public int EnemyHpMax => _encounter.Enemy.MaxHealth;

        public bool CanAct => _encounter.Phase != CombatPhase.EnemyTurn && _encounter.Phase != CombatPhase.Recovery && _encounter.Phase != CombatPhase.Finished;

        // Combat log
        public ObservableCollection<string> LogLines { get; } = new();

        // Skill bar — wraps Skill with a source tag for display
        public ObservableCollection<FightSkillVm> Skills { get; } = new();

        // Commands
        public ICommand AttackCommand { get; }
        public ICommand RunCommand { get; }
        public ICommand CastSkillCommand { get; }

        private Monster _monster;

        public ViewModel_PageFight()
        {
            _monster = MonsterService.GetMonsterById(1);
            var player = UserAccoundService.CurrentCharacter;

            if (player.CurrentRoom.HasMonsters)
                _monster = MonsterService.PickMonsterForFight(player.CurrentRoom.Monsters, player.CurrentRoom.EncounterableMonsters);

            _encounter = new CombatEncounter(player, _monster);

            // Slotted combat skills (order determined by player's slot configuration)
            foreach (var (skill, source) in SkillSlotService.GetCombatSkills(player))
                Skills.Add(new FightSkillVm(skill, DetermineTag(player, skill, source)));

            AttackCommand = new RelayCommand(Attack, CanActMethod);
            CastSkillCommand = new RelayCommand<FightSkillVm>(CastSkill, _ => CanAct);
            RunCommand = new RelayCommand(Run);

            _encounter.MonsterKilled += OnMonsterKilled;

            FlushNewLogEntries();
            RaiseAll();
        }

        private static string DetermineTag(Player player, Skill skill, SlottedSkillSource source) =>
            source switch
            {
                SlottedSkillSource.Combined => "Combined",
                SlottedSkillSource.CompositeFusion => "Fusion",
                _ => ""
            };

        private bool CanActMethod() => CanAct;

        private void Attack()
        {
            _encounter.PlayerAttack();
            FlushNewLogEntries();
            RaiseAll();
        }

        private void CastSkill(FightSkillVm? vm)
        {
            if (vm == null) return;
            _encounter.PlayerBeginCast(vm.Skill);
            FlushNewLogEntries();
            RaiseAll();
        }

        private void Run()
        {
            Navigation.SetFightState(false);
            ViewModel_PageRoom.WriteLog(Localization.T("msg.fight.run.success"));
            Navigation.NavigateGamePageToRegister(GamePageType.Room);
        }

        private int _lastLogIndex = 0;
        private void FlushNewLogEntries()
        {
            var log = _encounter.Log;
            while (_lastLogIndex < log.Count)
            {
                var entry = log[_lastLogIndex++];
                LogLines.Add(Localization.T(entry.Key, entry.Args));
            }
        }

        private void OnMonsterKilled(object? sender, MonsterKilledEventArgs e)
        {
            ViewModel_PageRoom.WriteLog($"{_monster.Name} {Localization.T("msg.fight.won")}");
        }

        private void RaiseAll()
        {
            OnPropertyChanged(nameof(ActiveMonsterName));
            OnPropertyChanged(nameof(PlayerHp));
            OnPropertyChanged(nameof(PlayerHpMax));
            OnPropertyChanged(nameof(PlayerMp));
            OnPropertyChanged(nameof(PlayerMpMax));
            OnPropertyChanged(nameof(EnemyHp));
            OnPropertyChanged(nameof(EnemyHpMax));
            OnPropertyChanged(nameof(CanAct));
            if (EnemyHp < 1)
            {
                Navigation.SetFightState(false);
                Navigation.NavigateGamePageToRegister(GamePageType.Room);
            }
        }
    }

    /// <summary>
    /// Wraps a <see cref="Skill"/> for display in the fight skill bar.
    /// Provides a short button label and a source tag ("Rune", "Fusion", or empty).
    /// </summary>
    public class FightSkillVm
    {
        public Skill Skill { get; }

        /// <summary>"Rune", "Fusion", or empty string for regular class skills.</summary>
        public string Tag { get; }
        public bool HasTag => !string.IsNullOrEmpty(Tag);

        /// <summary>Abbreviated label that fits in the 44×44 skill button.</summary>
        public string ButtonLabel { get; }

        public string Name => Skill.Name;
        public string Description => Skill.Description;
        public int ManaCost => Skill.ManaCost;

        public FightSkillVm(Skill skill, string tag = "")
        {
            Skill = skill;
            Tag = tag;
            // Use up to the first 5 characters of the first word so it fits the button
            string firstWord = skill.Name.Split(' ')[0];
            ButtonLabel = firstWord.Length <= 5 ? firstWord : firstWord[..5];
        }
    }
}
