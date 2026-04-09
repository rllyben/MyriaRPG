using MyriaLib.Entities;
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
        private string _strTooltip;
        private string _dexTooltip;
        private string _endTooltip;
        private string _intTooltip;
        private string _sprTooltip;
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

        [LocalizedKey("pg.character.tooltip.str")]
        public string StrTooltip
        {
            get { return _strTooltip; }
            private set { _strTooltip = value; OnPropertyChanged(); }
        }
        [LocalizedKey("pg.character.tooltip.dex")]
        public string DexTooltip
        {
            get { return _dexTooltip; }
            private set { _dexTooltip = value; OnPropertyChanged(); }
        }
        [LocalizedKey("pg.character.tooltip.end")]
        public string EndTooltip
        {
            get { return _endTooltip; }
            private set { _endTooltip = value; OnPropertyChanged(); }
        }
        [LocalizedKey("pg.character.tooltip.int")]
        public string IntTooltip
        {
            get { return _intTooltip; }
            private set { _intTooltip = value; OnPropertyChanged(); }
        }
        [LocalizedKey("pg.character.tooltip.spr")]
        public string SprTooltip
        {
            get { return _sprTooltip; }
            private set { _sprTooltip = value; OnPropertyChanged(); }
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

            Derived = new DerivedStatsVm(_player);

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
        public string STR_Display => Fmt(STR, STR_Added);
        public string DEX_Display => Fmt(DEX, DEX_Added);
        public string END_Display => Fmt(END, END_Added);
        public string INT_Display => Fmt(INT, INT_Added);
        public string SPR_Display => Fmt(SPR, SPR_Added);

        private static string Fmt(int @base, int added)
        {
            int total = @base + added;
            if (added > 0)             return $"{total} (+{added})";
            return $"{@base}";
        }
    }

    public class DerivedStatsVm
    {
        public string HP { get; }
        public string MaxHP { get; }
        public string MP { get; }
        public string MaxMP { get; }

        public string ATK { get; }
        public string DEF { get; }
        public string MATK { get; }
        public string MDEF { get; }
        public string Aim { get; }
        public string Evasion { get; }

        public double Crit { get; }
        public double Block { get; }

        private readonly int _gearHP,  _gearMP;
        private readonly int _gearATK, _gearDEF, _gearMATK, _gearMDEF, _gearAim, _gearEva;
        private readonly int _statHP,  _statMP;
        private readonly int _statATK, _statDEF, _statMATK, _statMDEF, _statAim, _statEva;

        public string CritPercent  => $"{Crit  * 100:0.#}%";
        public string BlockPercent => $"{Block * 100:0.#}%";

        public string Crit_Hint  => Crit  > 0 ? "(from gear)" : "";
        public string Block_Hint => Block > 0 ? "(from gear)" : "";

        public string HP_Hint   => Hint(_statHP,   _gearHP);
        public string MP_Hint   => Hint(_statMP,   _gearMP);
        public string ATK_Hint  => Hint(_statATK,  _gearATK);
        public string DEF_Hint  => Hint(_statDEF,  _gearDEF);
        public string MATK_Hint => Hint(_statMATK, _gearMATK);
        public string MDEF_Hint => Hint(_statMDEF, _gearMDEF);
        public string Aim_Hint  => Hint(_statAim,  _gearAim);
        public string Eva_Hint  => Hint(_statEva,  _gearEva);

        public DerivedStatsVm(Player player)
        {
            HP    = player.CurrentHealth.ToString();
            MaxHP = player.MaxHealth.ToString();
            MP    = player.CurrentMana.ToString();
            MaxMP = player.MaxMana.ToString();

            ATK     = player.TotalPhysicalAttack.ToString();
            DEF     = player.TotalPhysicalDefense.ToString();
            MATK    = player.TotalMagicAttack.ToString();
            MDEF    = player.TotalMagicDefense.ToString();
            Aim     = player.TotalAim.ToString();
            Evasion = player.TotalEvasion.ToString();

            Crit  = player.CritChance;
            Block = player.BlockChance;

            _gearHP   = player.GetBonusFromGear(g => g.BonusHP);
            _gearMP   = player.GetBonusFromGear(g => g.BonusMP);
            _gearATK  = player.GetBonusFromGear(g => g.BonusATK);
            _gearDEF  = player.GetBonusFromGear(g => g.BonusDEF);
            _gearMATK = player.GetBonusFromGear(g => g.BonusMATK);
            _gearMDEF = player.GetBonusFromGear(g => g.BonusMDEF);
            _gearAim  = player.GetBonusFromGear(g => g.BonusAim);
            _gearEva  = player.GetBonusFromGear(g => g.BonusEvasion);

            _statHP   = player.Stats.GetAddedStatBonus(DerivedStatType.MaxHealth);
            _statMP   = player.Stats.GetAddedStatBonus(DerivedStatType.MaxMana);
            _statATK  = player.Stats.GetAddedStatBonus(DerivedStatType.PhysicalAttack);
            _statDEF  = player.Stats.GetAddedStatBonus(DerivedStatType.PhysicalDefense);
            _statMATK = player.Stats.GetAddedStatBonus(DerivedStatType.MagicAttack);
            _statMDEF = player.Stats.GetAddedStatBonus(DerivedStatType.MagicDefense);
            _statAim  = player.Stats.GetAddedStatBonus(DerivedStatType.HitChance);
            _statEva  = player.Stats.GetAddedStatBonus(DerivedStatType.DodgeChance);
        }

        private static string Hint(int fromStats, int fromGear)
        {
            if (fromStats > 0 && fromGear > 0) return $"(+{fromStats} stats, +{fromGear} gear)";
            if (fromStats > 0) return $"(+{fromStats} stats)";
            if (fromGear  > 0) return $"(+{fromGear} gear)";
            return "";
        }
    }
}
