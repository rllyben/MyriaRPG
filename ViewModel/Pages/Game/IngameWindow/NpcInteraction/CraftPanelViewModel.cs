using MyriaLib.Entities.Items;
using MyriaLib.Entities.NPCs;
using MyriaLib.Entities.Players;
using MyriaLib.Services.Builder;
using MyriaLib.Systems;
using MyriaRPG.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.Pages.Game.IngameWindow.NpcInteraction
{
    public class CraftPanelViewModel : BaseViewModel
    {
        private readonly Player _player;
        private readonly Action _goBack;

        public string Title => Localization.T("npc.craft.title");
        public string BtnBack => Localization.T("app.general.UI.back");
        public string BtnCraft => Localization.T("npc.craft.craft");

        public ObservableCollection<RecipeVm> Recipes { get; } = new();

        private RecipeVm _selectedRecipe;
        public RecipeVm SelectedRecipe
        {
            get => _selectedRecipe;
            set
            {
                _selectedRecipe = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedName));
                OnPropertyChanged(nameof(CanCraft));
            }

        }

        public string SelectedName => SelectedRecipe?.Name ?? "";
        public ObservableCollection<IngredientVm> SelectedIngredients => SelectedRecipe?.Ingredients ?? new();

        public bool CanCraft => SelectedRecipe != null; // later: check materials

        public ICommand BackCommand { get; }
        public ICommand CraftCommand { get; }

        public CraftPanelViewModel(Npc npc, Player player, Action goBack)
        {
            _player = player;
            _goBack = goBack;

            BackCommand = new RelayCommand(_goBack);
            CraftCommand = new RelayCommand(CraftSelected);

            LoadRecipes();
        }

        private void LoadRecipes()
        {
            Recipes.Clear();

            // first stub recipe: 3 iron_ore -> upgrade_stone
            Recipes.Add(new RecipeVm
            {
                Id = "upgrade_stone",
                Name = Localization.T("item.upgrade_stone.name"),
                Ingredients = new ObservableCollection<IngredientVm>
                {
                    new IngredientVm { Id = "iron_ore", Name = "item.iron_ore.name", Amount = 3 }
                }

            });

            SelectedRecipe = Recipes.FirstOrDefault();
        }

        private void CraftSelected()
        {
            if (SelectedRecipe == null) return;

            foreach (IngredientVm item in SelectedRecipe.Ingredients)
            {
                if (!_player.Inventory.Items.Any(i => i.Id == item.Id))
                    return;
                Item material = _player.Inventory.Items.Where(i => i.Id == item.Id).FirstOrDefault();
                if (item.Amount > material.StackSize)
                    return;
                Item used = ItemFactory.CreateItem(material.Id, item.Amount);
                _player.Inventory.RemoveItem(used);
            }
            ItemFactory.TryCreateItem(SelectedRecipe.Id, out Item creation);
            _player.Inventory.AddItem(creation, _player);
        }

    }
    public class IngredientVm : BaseViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
        public string AmountText => Amount.ToString();
        public override string ToString()
        {
            return Name;
        }
    }
    public class RecipeVm : BaseViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ObservableCollection<IngredientVm> Ingredients { get; set; } = new();
        public override string ToString()
        {
            return Localization.T(Name);
        }
    }

}
