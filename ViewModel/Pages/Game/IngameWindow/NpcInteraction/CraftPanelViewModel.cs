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
        private readonly Npc _npc;
        private readonly Player _player;
        private readonly Action _goBack;

        public string Title => Localization.T("npc.craft.title");
        public string BtnBack => Localization.T("app.general.UI.back");
        public string BtnCraft => Localization.T("npc.craft.craft");
        public string QuantityLabel => Localization.T("npc.shop.quantity");
        public string MaxLabel => Localization.T("app.general.UI.max");

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
                OnPropertyChanged(nameof(SelectedIngredients));
                UpdateMaxCraftable();
                Quantity = 1; // Reset quantity when recipe changes
            }
        }

        public string SelectedName => SelectedRecipe?.Name ?? "";
        public ObservableCollection<IngredientVm> SelectedIngredients => SelectedRecipe?.Ingredients ?? new();

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set
            {
                if (value < 1) value = 1;
                if (value > MaxCraftable && MaxCraftable > 0) value = MaxCraftable;
                if (MaxCraftable == 0) value = 0;
                
                _quantity = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanCraft));
            }
        }

        private int _maxCraftable;
        public int MaxCraftable
        {
            get => _maxCraftable;
            set
            {
                _maxCraftable = value;
                OnPropertyChanged();
            }
        }

        public bool CanCraft => SelectedRecipe != null && Quantity > 0 && Quantity <= MaxCraftable;

        public ICommand BackCommand { get; }
        public ICommand CraftCommand { get; }
        public ICommand IncreaseQuantityCommand { get; }
        public ICommand DecreaseQuantityCommand { get; }
        public ICommand MaxQuantityCommand { get; }

        public CraftPanelViewModel(Npc npc, Player player, Action goBack)
        {
            _npc = npc;
            _player = player;
            _goBack = goBack;

            BackCommand = new RelayCommand(_goBack);
            CraftCommand = new RelayCommand(CraftSelected);
            IncreaseQuantityCommand = new RelayCommand(() => Quantity++);
            DecreaseQuantityCommand = new RelayCommand(() => Quantity--);
            MaxQuantityCommand = new RelayCommand(() => Quantity = MaxCraftable);

            LoadRecipes();
        }

        private void LoadRecipes()
        {
            Recipes.Clear();

            // first stub recipe: 3 iron_ore -> upgrade_stone
            var recipe = new RecipeVm
            {
                Id = "upgrade_stone",
                Name = Localization.T("item.upgrade_stone.name"),
                Ingredients = new ObservableCollection<IngredientVm>
                {
                    new IngredientVm { Id = "iron_ore", Name = "item.iron_ore.name", Amount = 3 }
                }
            };
            
            // Update player inventory counts for ingredients
            foreach(var ing in recipe.Ingredients)
            {
                var playerItem = _player.Inventory.Items.FirstOrDefault(i => i.Id == ing.Id);
                ing.PlayerHas = playerItem?.StackSize ?? 0;
            }

            Recipes.Add(recipe);
            SelectedRecipe = Recipes.FirstOrDefault();
        }

        private void UpdateMaxCraftable()
        {
            if (SelectedRecipe == null)
            {
                MaxCraftable = 0;
                return;
            }

            int max = int.MaxValue;

            foreach (var ingredient in SelectedRecipe.Ingredients)
            {
                var playerItem = _player.Inventory.Items.FirstOrDefault(i => i.Id == ingredient.Id);
                int playerAmount = playerItem?.StackSize ?? 0;
                ingredient.PlayerHas = playerAmount; // Update display
                
                if (ingredient.Amount > 0)
                {
                    int canMake = playerAmount / ingredient.Amount;
                    if (canMake < max)
                    {
                        max = canMake;
                    }
                }
            }

            MaxCraftable = max == int.MaxValue ? 0 : max;
            OnPropertyChanged(nameof(CanCraft));
            
            // Re-validate quantity
            if (Quantity > MaxCraftable) Quantity = MaxCraftable;
            if (Quantity == 0 && MaxCraftable > 0) Quantity = 1;
        }

        private void CraftSelected()
        {
            if (SelectedRecipe == null || Quantity <= 0) return;

            // Verify resources again
            foreach (IngredientVm item in SelectedRecipe.Ingredients)
            {
                var material = _player.Inventory.Items.FirstOrDefault(i => i.Id == item.Id);
                if (material == null || material.StackSize < item.Amount * Quantity)
                    return;
            }

            // Consume resources
            foreach (IngredientVm item in SelectedRecipe.Ingredients)
            {
                var material = _player.Inventory.Items.FirstOrDefault(i => i.Id == item.Id);
                
                // RemoveItem logic in Inventory.cs removes the object reference if stacksize matches.
                // We need to be careful. The safest way with current Inventory.cs is to reduce stack size manually or create a dummy item to remove.
                // Since Inventory.RemoveItem(Item item) just does Items.Remove(item), it expects the EXACT instance from the list if we want to remove the whole stack.
                // But here we want to reduce quantity.
                
                // Let's implement a safer consumption logic here that respects the Inventory implementation
                int amountToRemove = item.Amount * Quantity;
                
                if (material.StackSize > amountToRemove)
                {
                    material.StackSize -= amountToRemove;
                }
                else
                {
                    // Exact match or more (though we checked before), remove the item entirely
                    _player.Inventory.RemoveItem(material);
                }
            }
            
            // Force inventory restack/cleanup just in case
            _player.Inventory.Restack();

            // Create result
            if (ItemFactory.TryCreateItem(SelectedRecipe.Id, out Item creation))
            {
                creation.StackSize = Quantity;
                _player.Inventory.AddItem(creation, _player);
            }

            // Refresh UI
            UpdateMaxCraftable();
        }

    }
    public class IngredientVm : BaseViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
        
        private int _playerHas;
        public int PlayerHas
        {
            get => _playerHas;
            set { _playerHas = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayString)); }
        }

        public string AmountText => Amount.ToString();
        
        public string DisplayString => $"{Localization.T(Name)}: {Amount} ({Localization.T("app.general.UI.owned")}: {PlayerHas})";

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
