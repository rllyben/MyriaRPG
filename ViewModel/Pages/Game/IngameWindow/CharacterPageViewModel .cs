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
        private Player _player;

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

        public BaseStatsVm Base { get; private set; }
        public DerivedStatsVm Derived { get; private set; }
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
            _player = player;
            CharacterName = player.Name;
            Level = player.Level;
            XpCurrent = player.Experience;
            XpToNextLevel = player.ExpForNextLvl;
            CharClass = player.Class.ToString();

            RefreshStats();

            IncreaseStatCommand = new RelayCommand<string>(IncreaseStat);
            DecreaseStatCommand = new RelayCommand<string>(DecreaseStat);
        }

        private void RefreshStats()
        {
            Base = new BaseStatsVm(
                _player.Stats.Strength, _player.Stats.StrengthAdded,
                _player.Stats.Dexterity, _player.Stats.DexterityAdded,
                _player.Stats.Endurance, _player.Stats.EnduranceAdded,
                _player.Stats.Intelligence, _player.Stats.IntelligenceAdded,
                _player.Stats.Spirit, _player.Stats.SpiritAdded,
                _player.Stats.UnusedPoints);

            Derived = new DerivedStatsVm(
                _player.CurrentHealth, _player.MaxHealth, _player.CurrentMana, _player.MaxMana,
                _player.TotalPhysicalAttack, _player.TotalPhysicalDefense, _player.TotalMagicAttack, _player.TotalMagicDefense,
                _player.TotalAim, _player.TotalEvasion, _player.CritChance, _player.BlockChance);
            
            OnPropertyChanged(nameof(Base));
            OnPropertyChanged(nameof(Derived));
        }

        private void IncreaseStat(string? statKey)
        {
            if (_player.Stats.UnusedPoints <= 0 || statKey is null) return;

            switch (statKey)
            {
                case "STR": _player.Stats.StrengthAdded++; break;
                case "DEX": _player.Stats.DexterityAdded++; break;
                case "END": _player.Stats.EnduranceAdded++; break;
                case "INT": _player.Stats.IntelligenceAdded++; break;
                case "SPR": _player.Stats.SpiritAdded++; break;
                default: return;
            }
            _player.Stats.UnusedPoints--;
            RefreshStats();
        }

        private void DecreaseStat(string? statKey)
        {
            if (statKey is null) return;

            switch (statKey)
            {
                case "STR":
                    if (_player.Stats.StrengthAdded > 0) { _player.Stats.StrengthAdded--; _player.Stats.UnusedPoints++; }
                    break;
                case "DEX":
                    if (_player.Stats.DexterityAdded > 0) { _player.Stats.DexterityAdded--; _player.Stats.UnusedPoints++; }
                    break;
                case "END":
                    if (_player.Stats.EnduranceAdded > 0) { _player.Stats.EnduranceAdded--; _player.Stats.UnusedPoints++; }
                    break;
                case "INT":
                    if (_player.Stats.IntelligenceAdded > 0) { _player.Stats.IntelligenceAdded--; _player.Stats.UnusedPoints++; }
                    break;
                case "SPR":
                    if (_player.Stats.SpiritAdded > 0) { _player.Stats.SpiritAdded--; _player.Stats.UnusedPoints++; }
                    break;
            }
            RefreshStats();
        }
    }

    public record BaseStatsVm(
        int STR, int STR_Added,
        int DEX, int DEX_Added,
        int END, int END_Added,
        int INT, int INT_Added,
        int SPR, int SPR_Added,
        int Unspent)
    {
        public string STR_Display => STR_Added > 0 ? $"{STR} (+{STR_Added})" : $"{STR}";
        public string DEX_Display => DEX_Added > 0 ? $"{DEX} (+{DEX_Added})" : $"{DEX}";
        public string END_Display => END_Added > 0 ? $"{END} (+{END_Added})" : $"{END}";
        public string INT_Display => INT_Added > 0 ? $"{INT} (+{INT_Added})" : $"{INT}";
        public string SPR_Display => SPR_Added > 0 ? $"{SPR} (+{SPR_Added})" : $"{SPR}";
    }

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
