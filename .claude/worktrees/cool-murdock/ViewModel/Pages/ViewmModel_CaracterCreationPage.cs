using MyriaLib.Entities;
using MyriaLib.Entities.Players;
using MyriaLib.Models;
using MyriaLib.Services;
using MyriaLib.Systems;
using MyriaLib.Systems.Enums;
using MyriaRPG.Model;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages;
using MyriaRPG.View.Pages.Game;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages
{
    class ViewmModel_CaracterCreationPage : BaseViewModel
    {
        private string tbl_Name;
        private string tbl_Class;
        private string tbl_ShowName;
        private string tbl_ShowClass;
        private string tbl_Preview;
        private string tbl_Title;
        private string btn_Back;
        private string btn_Create;
        [LocalizedKey("app.general.UI.name")]
        public string TblName
        {
            get { return tbl_Name; }
            private set
            {
                tbl_Name = value;
                OnPropertyChanged(nameof(TblName));
            }

        }
        [LocalizedKey("app.general.UI.name")]
        public string TblShowName
        {
            get { return tbl_ShowName; }
            private set
            {
                tbl_ShowName = value + ": ";
                OnPropertyChanged(nameof(TblShowName));
            }

        }
        [LocalizedKey("pg.character.create.class")]
        public string TblClass
        {
            get { return tbl_Class; }
            private set
            {
                tbl_Class = value;
                OnPropertyChanged(nameof(TblClass));
            }

        }
        [LocalizedKey("pg.character.create.class")]
        public string TblShowClass
        {
            get { return tbl_ShowClass; }
            private set
            {
                tbl_Class = value + ": ";
                OnPropertyChanged(nameof(TblShowClass));
            }

        }
        [LocalizedKey("pg.character.create.preview")]
        public string TblPreview
        {
            get { return tbl_Preview; }
            private set
            {
                tbl_Preview = value;
                OnPropertyChanged(nameof(TblPreview));
            }

        }
        [LocalizedKey("pg.character.create.title")]
        public string TblTitle
        {
            get { return tbl_Title; }
            private set
            {
                tbl_Title = value;
                OnPropertyChanged(nameof(TblTitle));
            }

        }
        [LocalizedKey("app.general.UI.back")]
        public string BtnBack
        {
            get { return btn_Back; }
            private set
            {
                btn_Back = value;
                OnPropertyChanged(nameof(BtnBack));
            }

        }
        [LocalizedKey("app.general.UI.create")]
        public string BtnCreate
        {
            get { return btn_Create; }
            private set
            {
                btn_Create = value;
                OnPropertyChanged(nameof(BtnCreate));
            }

        }
        public ObservableCollection<PlayerClass> Classes { get; } =
            new(Enum.GetValues(typeof(PlayerClass)).Cast<PlayerClass>());

        private PlayerClass? _selectedClass;
        public PlayerClass? SelectedClass
        {
            get => _selectedClass;
            set { _selectedClass = value; OnPropertyChanged(); Revalidate(); }
        }

        private string _characterName = "";
        public string CharacterName
        {
            get => _characterName;
            set { _characterName = value; OnPropertyChanged(); Revalidate(); }
        }

        private string _validationText = "";
        public string ValidationText
        {
            get => _validationText;
            set { _validationText = value; OnPropertyChanged(); }
        }

        private bool _canCreate;
        public bool CanCreate
        {
            get => _canCreate;
            set { _canCreate = value; OnPropertyChanged(); }
        }

        public ICommand CreateCommand { get; }
        public ICommand BackCommand { get; }

        // You likely already have these services in your WPF project:
        private readonly UserAccount _user; // current logged-in user

        public ViewmModel_CaracterCreationPage()
        {
            _user = UserAccoundService.CurrentUser;

            CreateCommand = new RelayCommand(Create);
            BackCommand = new RelayCommand(() => Navigation.NavigateMain(new Page_CharacterSelection()));

            Revalidate();
        }

        private void Revalidate()
        {
            ValidationText = "";

            var name = (CharacterName ?? "").Trim();

            if (name.Length < 2)
                ValidationText = Localization.T("pg.character.create.validation.name.short");
            else if (name.Any(ch => !char.IsLetterOrDigit(ch) && ch != '_' && ch != '-'))
                ValidationText = Localization.T("pg.character.create.validation.name.invalid");
            else if (SelectedClass is null)
                ValidationText = Localization.T("pg.character.create.validation.class.missing");

            CanCreate = string.IsNullOrWhiteSpace(ValidationText);
        }

        private void Create()
        {
            if (!CanCreate) return;

            var name = CharacterName.Trim();
            var chosenClass = SelectedClass!.Value;

            // TODO: Use your real stat initialization rules.
            // If you have a "StatsBuilder/ClassProfile" later, call it here instead.
            var stats = new Stats(); // adjust if your Stats needs args

            var player = new Player(name, stats) { Class = chosenClass }; // Player requires (name, stats) :contentReference[oaicite:6]{index=6}

            // Set starting room (must be non-null for SaveCharacter)
            // Choose the correct start room for your game (first room is a safe default).
            player.CurrentRoom = RoomService.GetRoomById(1);
            player.CurrentRoomId = player.CurrentRoom.Id;

            // Save to Data/saves/<user>-<name>.json :contentReference[oaicite:7]{index=7}
            CharacterService.SaveCharacter(_user, player);
            

            // Optional: navigate back to character selection
            UserAccoundService.CurrentCharacter = player;
            UserAccoundService.CurrentUser.CharacterNames.Add(player.Name);
            UserAccoundService.SaveUser();
            Navigation.NavigateMain(new Page_Game());
        }

    }

}
