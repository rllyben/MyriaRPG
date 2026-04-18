using MyriaLib.Services;
using MyriaRPG.Services;
using MyriaRPG.View.Pages;
using MyriaRPG.View.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace MyriaRPG.ViewModel.Pages
{
    public class LoadingStepVm : INotifyPropertyChanged
    {
        private string _status = "...";
        private Brush _statusColor = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x99));

        public string Label { get; set; } = "";

        public string Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); }
        }

        public Brush StatusColor
        {
            get => _statusColor;
            set { _statusColor = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public static readonly Brush ColorPending = new SolidColorBrush(Color.FromRgb(0x88, 0x88, 0x99));
        public static readonly Brush ColorDone    = new SolidColorBrush(Color.FromRgb(0x55, 0xCC, 0x88));
        public static readonly Brush ColorFailed  = new SolidColorBrush(Color.FromRgb(0xE0, 0x55, 0x55));
    }

    public class ViewModel_LoadingPage : BaseViewModel
    {
        private static readonly Dictionary<string, string> StepLabels = new()
        {
            ["game_status"] = "Loading game status",
            ["items"]       = "Loading items",
            ["monsters"]    = "Loading monsters",
            ["npcs"]        = "Loading NPCs",
            ["rooms"]       = "Loading rooms",
            ["connections"] = "Connecting world",
            ["day_cycle"]   = "Initializing day cycle",
            ["quests"]      = "Loading quests",
            ["skills"]      = "Loading skills",
            ["registries"]  = "Loading registries",
            ["skill_systems"] = "Loading skill systems",
        };

        public ObservableCollection<LoadingStepVm> Steps { get; } = new();

        public ViewModel_LoadingPage()
        {
            foreach (var label in StepLabels.Values)
                Steps.Add(new LoadingStepVm { Label = label });

            _ = RunInitAsync();
        }

        private async Task RunInitAsync()
        {
            var stepKeys = StepLabels.Keys.ToList();
            int nextIndex = 0;

            var progress = new Progress<string>(key =>
            {
                int i = stepKeys.IndexOf(key);
                if (i >= 0 && i < Steps.Count)
                {
                    Steps[i].Status = "Done";
                    Steps[i].StatusColor = LoadingStepVm.ColorDone;
                }
                nextIndex = i + 1;
            });

            try
            {
                await Task.Run(() => GameService.InitializeGame(progress));
                Navigation.NavigateMain(new Page_StartupMenue());
            }
            catch (Exception ex)
            {
                int failedIndex = nextIndex < Steps.Count ? nextIndex : Steps.Count - 1;
                Steps[failedIndex].Status = "Failed";
                Steps[failedIndex].StatusColor = LoadingStepVm.ColorFailed;

                string failedLabel = Steps[failedIndex].Label;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    var errorWindow = new Window_InitError(failedLabel, ex.Message);
                    errorWindow.Show();
                });
            }
        }
    }
}
