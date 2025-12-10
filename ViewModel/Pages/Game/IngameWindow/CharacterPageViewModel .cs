using MyriaLib.Entities.Players;
using MyriaRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow
{
    public class CharacterPageViewModel : BaseViewModel
    {
        public string CharacterName { get; set; }
        public int Level { get; set; }
        public long XpCurrent { get; set; }
        public long XpToNextLevel { get; set; }  // total needed for current->next

        public string XpProgressText => $"{XpCurrent:N0} / {XpToNextLevel:N0}  ({XpPercent:0}%)";
        public double XpPercent => Math.Clamp(XpToNextLevel > 0 ? (XpCurrent * 100.0) / XpToNextLevel : 0, 0, 100);

        public BaseStatsVm Base { get; }
        public DerivedStatsVm Derived { get; }
        public ICommand IncreaseStatCommand { get; }
        public ICommand DecreaseStatCommand { get; }

        // Optional: expose a title so your window label can bind to it (like other pages)
        public string WindowTitle => $"{CharacterName} — Level {Level}";

        public CharacterPageViewModel(Player player) // pass your real models
        {
            CharacterName = player.Name;
            Level = player.Level;
            XpCurrent = player.Experience;
            XpToNextLevel = player.ExpForNextLvl;

            Base = new BaseStatsVm(
                player.Stats.Strength, player.Stats.Dexterity, player.Stats.Endurance,
                player.Stats.Intelligence, player.Stats.Spirit);

            Derived = new DerivedStatsVm(
                player.CurrentHealth, player.MaxHealth, player.CurrentMana, player.MaxMana,
                player.TotalPhysicalAttack, player.TotalPhysicalDefense, player.TotalMagicAttack, player.TotalMagicDefense,
                player.TotalAim, player.TotalEvasion, player.CritChance, player.BlockChance);

            IncreaseStatCommand = new RelayCommand<string>(IncreaseStat);
            DecreaseStatCommand = new RelayCommand<string>(DecreaseStat);
        }

        private void IncreaseStat(string? statKey)
        {
            // TODO: implement when stat point system is ready
            // Example later:
            // if (Base.Unspent <= 0 || statKey is null) return;
            // switch (statKey) { case "STR": Base = Base with { STR = Base.STR + 1, Unspent = Base.Unspent - 1 }; break; ... }
        }

        private void DecreaseStat(string? statKey)
        {
            // TODO: implement rules for decreasing (allow only points spent this level? etc.)
        }
    }

    public record BaseStatsVm(int STR, int DEX, int END, int INT, int SPR);
    public class DerivedStatsVm
    {
        public int HP { get; }
        public int MaxHP { get; }
        public int MP { get; }
        public int MaxMP { get; }

        public int ATK { get; }
        public int DEF { get; }
        public int MATK { get; }
        public int MDEF { get; }
        public int Aim { get; }
        public int Evasion { get; }

        public double Crit { get; }
        public double Block { get; }

        public string CritPercent => $"{Crit * 100:0.#}%";
        public string BlockPercent => $"{Block * 100:0.#}%";

        public DerivedStatsVm(int hp, int maxHp, int mp, int maxMp,
                              int atk, int def, int matk, int mdef,
                              int aim, int evasion, double crit, double block)
        {
            HP = hp; MaxHP = maxHp; MP = mp; MaxMP = maxMp;
            ATK = atk; DEF = def; MATK = matk; MDEF = mdef;
            Aim = aim; Evasion = evasion; Crit = crit; Block = block;
        }
    }
}
