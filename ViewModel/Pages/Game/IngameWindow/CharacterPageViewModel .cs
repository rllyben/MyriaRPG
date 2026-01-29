using MyriaLib.Entities.Players;
using MyriaLib.Systems;
using MyriaRPG.Model;
using MyriaRPG.Utils;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow
{
    public class CharacterPageViewModel : BaseViewModel
    {
        private string tbl_Level;
        private string tbl_Base;
        private string tbl_STR;
        private string tbl_DEX;
        private string tbl_END;
        private string tbl_INT;
        private string tbl_SPR;
        private string tblUnspent;
        private string tbl_Derived;
        private string tbl_HP;
        private string tbl_MP;
        private string tbl_ATK;
        private string tbl_DEF;
        private string tbl_MATK;
        private string tbl_MDEF;
        private string tbl_Aim;
        private string tbl_Crit;
        private string tbl_Evasion;
        private string tbl_Block;
        private string _charClass;
        [LocalizedKey("pg.character.info.level")]
        public string TblLevel
        {
            get { return tbl_Level; }
            private set
            {
                tbl_Level = value + " ";
                OnPropertyChanged(nameof(TblLevel));
            }

        }
        [LocalizedKey("pg.character.info.base")]
        public string TblBase
        {
            get { return tbl_Base; }
            private set
            {
                tbl_Base = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.str")]
        public string TblSTR
        {
            get { return tbl_STR; }
            private set
            {
                tbl_STR = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.dex")]
        public string TblDEX
        {
            get { return tbl_DEX; }
            private set
            {
                tbl_DEX = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.end")]
        public string TblEND
        {
            get { return tbl_END; }
            private set
            {
                tbl_END = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.int")]
        public string TblINT
        {
            get { return tbl_INT; }
            private set
            {
                tbl_INT = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.spr")]
        public string TblSPR
        {
            get { return tbl_SPR; }
            private set
            {
                tbl_SPR = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.hp")]
        public string TblHP
        {
            get { return tbl_HP; }
            private set
            {
                tbl_HP = value + " ";
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.mp")]
        public string TblMP
        {
            get { return tbl_MP; }
            private set
            {
                tbl_MP = value + " ";
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.atk")]
        public string TblATK
        {
            get { return tbl_ATK; }
            private set
            {
                tbl_ATK = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.def")]
        public string TblDEF
        {
            get { return tbl_DEF; }
            private set
            {
                tbl_DEF = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.matk")]
        public string TblMATK
        {
            get { return tbl_MATK; }
            private set
            {
                tbl_MATK = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.mdef")]
        public string TblMDEF
        {
            get { return tbl_MDEF; }
            private set
            {
                tbl_MDEF = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.aim")]
        public string TblAim
        {
            get { return tbl_Aim; }
            private set
            {
                tbl_Aim = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.eva")]
        public string TblEvasion
        {
            get { return tbl_Evasion; }
            private set
            {
                tbl_Evasion = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.crit")]
        public string TblCrit
        {
            get { return tbl_Crit; }
            private set
            {
                tbl_Crit = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.block")]
        public string TblBlock
        {
            get { return tbl_Block; }
            private set
            {
                tbl_Block = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.unspent")]
        public string TblUnspent
        {
            get { return tblUnspent; }
            private set
            {
                tblUnspent = value;
                OnPropertyChanged();
            }

        }
        [LocalizedKey("pg.character.info.derived")]
        public string TblDerived
        {
            get { return tbl_Derived; }
            private set
            {
                tbl_Derived = value;
                OnPropertyChanged();
            }

        }
        public string CharClass
        {
            get => _charClass;
            set
            {
                _charClass = value;
                OnPropertyChanged();
            }

        }
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
        public string WindowTitle
        {
            get => _windowTitle;
            set { _windowTitle = value; OnPropertyChanged(); }
        }
        private string _windowTitle = Localization.T("app.general.UI.character");

        public CharacterPageViewModel(Player player) // pass your real models
        {
            CharacterName = player.Name;
            Level = player.Level;
            XpCurrent = player.Experience;
            XpToNextLevel = player.ExpForNextLvl;
            CharClass = player.Class.ToString();

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
