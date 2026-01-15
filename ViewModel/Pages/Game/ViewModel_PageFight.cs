using MyriaLib.Entities.Monsters;
using MyriaLib.Entities.Skills;
using MyriaLib.Services;
using MyriaLib.Systems;
using MyriaLib.Systems.Enums;
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
        public int PlayerHpMax => _encounter.Player.Stats.MaxHealth;
        public int PlayerMp => _encounter.Player.CurrentMana;
        public int PlayerMpMax => _encounter.Player.Stats.MaxMana;

        public int EnemyHp => _encounter.Enemy.CurrentHealth;
        public int EnemyHpMax => _encounter.Enemy.Stats.MaxHealth;

        public bool CanAct => _encounter.Phase != CombatPhase.EnemyTurn && _encounter.Phase != CombatPhase.Recovery && _encounter.Phase != CombatPhase.Finished;

        // Combat log
        public ObservableCollection<string> LogLines { get; } = new();

        // Skill bar (bottom)
        public ObservableCollection<Skill> Skills { get; } = new();

        // Commands
        public ICommand AttackCommand { get; }
        public ICommand RunCommand { get; }
        public ICommand CastSkillCommand { get; }
        private Monster _monster;
        public ViewModel_PageFight()
        {
            _monster = MonsterService.GetMonsterById(1);
            if (UserAccoundService.CurrentCharacter.CurrentRoom.HasMonsters)
            {
                _monster = MonsterService.PickMonsterForFight(UserAccoundService.CurrentCharacter.CurrentRoom.Monsters, UserAccoundService.CurrentCharacter.CurrentRoom.EncounterableMonsters);
                _encounter = new CombatEncounter(UserAccoundService.CurrentCharacter, _monster);
            }
            else
                _encounter = new CombatEncounter(UserAccoundService.CurrentCharacter, _monster);
            // load skills from player
            foreach (var s in UserAccoundService.CurrentCharacter.Skills)
                Skills.Add(s);

            AttackCommand = new RelayCommand(Attack, CanActMethod);
            CastSkillCommand = new RelayCommand<Skill>(CastSkill, _ => CanAct);
            RunCommand = new RelayCommand(Run);

            FlushNewLogEntries();
            RaiseAll();
        }
        private bool CanActMethod()
        {
            return CanAct;
        }

        private void Attack()
        {
            _encounter.PlayerAttack();
            FlushNewLogEntries();
            RaiseAll();
        }

        private void CastSkill(Skill? skill)
        {
            if (skill == null) return;
            _encounter.PlayerBeginCast(skill);
            FlushNewLogEntries();
            RaiseAll();
        }

        private void Run()
        {
            ViewModel_PageRoom.WriteLog(Localization.T("msg.fight.run.success"));
            Navigation.NavigateGamePageToRegister(0);
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
            CharacterHeaderVm.Refresh();
            if (EnemyHp < 1)
            {
                ViewModel_PageRoom.WriteLog($"{_monster.Name} {Localization.T("msg.fight.won")}");
                foreach ( var entry in _encounter.GetDropNames())
                {
                    ViewModel_PageRoom.WriteLog($"{Localization.T("msg.fight.recieved")} {Localization.T(entry.Key)} [{_encounter.GetDropNames()[entry.Key]}]");
                }
                ViewModel_PageRoom.WriteLog($"{Localization.T("msg.fight.gainxp")} {_monster.Exp}{Localization.T("app.general.xp")}");

                Navigation.NavigateGamePageToRegister(0);
            }

        }

    }

}
