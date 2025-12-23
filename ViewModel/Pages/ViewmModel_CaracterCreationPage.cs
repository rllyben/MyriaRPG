using MyriaLib.Entities;
using MyriaLib.Entities.Players;
using MyriaLib.Models;
using MyriaLib.Services;
using MyriaLib.Systems.Enums;
using MyriaRPG.Services;
using MyriaRPG.Utils;
using MyriaRPG.View.Pages;
using MyriaRPG.View.Pages.Game;
using MyriaRPG.View.Pages.Game.IngameWindow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages
{
    class ViewmModel_CaracterCreationPage : BaseViewModel
    {
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
                ValidationText = "Name must be at least 2 characters.";
            else if (name.Any(ch => !char.IsLetterOrDigit(ch) && ch != '_' && ch != '-'))
                ValidationText = "Name may only contain letters, numbers, '_' or '-'.";
            else if (SelectedClass is null)
                ValidationText = "Please choose a class.";

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
