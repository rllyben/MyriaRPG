using MyriaLib.Entities.Items;
using MyriaLib.Entities.NPCs;
using MyriaLib.Entities.Players;
using MyriaLib.Services.Builder;
using MyriaLib.Systems;
using MyriaRPG.Utils;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MyriaRPG.ViewModel.UserControls.IngameWindow
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
        public string IngredientsLabel => Localization.T("npc.craft.ingredients");

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
                StatusMessage = "";
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

        private string _statusMessage = "";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
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

            // Recipe 1: 3 iron_ore -> upgrade_stone
            var recipe = new RecipeVm
            {
                Id = "upgrade_stone",
                Name = Localization.T("item.upgrade_stone.name"),
                Ingredients = new ObservableCollection<IngredientVm>
                {
                    new IngredientVm 
                    { 
                        Id = "iron_ore", 
                        Name = "item.iron_ore.name", 
                        Amount = 3 
                    }
                }
            };
            
            Recipes.Add(recipe);

            // Add more recipes here as needed

            SelectedRecipe = Recipes.FirstOrDefault();
            if (SelectedRecipe == null)
            {
                StatusMessage = Localization.T("npc.craft.noRecipes");
            }
        }

        private void UpdateMaxCraftable()
        {
            if (SelectedRecipe == null)
            {
                MaxCraftable = 0;
                StatusMessage = Localization.T("npc.craft.selectRecipe");
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
            
            if (MaxCraftable == 0)
            {
                StatusMessage = Localization.T("npc.craft.notEnoughMaterials");
            }
            else
            {
                StatusMessage = "";
            }
            
            // Re-validate quantity
            if (Quantity > MaxCraftable) Quantity = MaxCraftable;
            if (Quantity == 0 && MaxCraftable > 0) Quantity = 1;
        }

        private void CraftSelected()
        {
            if (SelectedRecipe == null || Quantity <= 0)
            {
                StatusMessage = Localization.T("npc.craft.selectRecipe");
                return;
            }

            // Verify resources again
            foreach (IngredientVm item in SelectedRecipe.Ingredients)
            {
                var material = _player.Inventory.Items.FirstOrDefault(i => i.Id == item.Id);
                if (material == null || material.StackSize < item.Amount * Quantity)
                {
                    StatusMessage = Localization.T("npc.craft.notEnoughMaterials");
                    return;
                }
            }

            // Consume resources
            foreach (IngredientVm item in SelectedRecipe.Ingredients)
            {
                var material = _player.Inventory.Items.FirstOrDefault(i => i.Id == item.Id);
                
                int amountToRemove = item.Amount * Quantity;
                
                if (material.StackSize > amountToRemove)
                {
                    material.StackSize -= amountToRemove;
                }
                else
                {
                    _player.Inventory.RemoveItem(material);
                }
            }
            
            _player.Inventory.Restack();

            // Create result
            if (ItemFactory.TryCreateItem(SelectedRecipe.Id, out Item creation))
            {
                creation.StackSize = Quantity;
                if (_player.Inventory.AddItem(creation, _player))
                {
                    StatusMessage = Localization.T("npc.craft.success", Quantity, SelectedRecipe.Name);
                    UpdateMaxCraftable();
                    Quantity = 1;
                }
                else
                {
                    // Inventory full - need to return materials
                    foreach (IngredientVm item in SelectedRecipe.Ingredients)
                    {
                        var material = ItemFactory.CreateItem(item.Id, item.Amount * Quantity);
                        _player.Inventory.AddItem(material, _player);
                    }
                    StatusMessage = Localization.T("npc.craft.inventoryFull");
                }
            }
            else
            {
                StatusMessage = Localization.T("npc.craft.fail");
            }
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
        public bool HasEnough => PlayerHas >= Amount;
        
        public string DisplayString => $"{Localization.T(Name)}: {Amount} ({Localization.T("app.general.UI.owned")}: {PlayerHas})";

        public override string ToString() => Name;
    }

    public class RecipeVm : BaseViewModel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ObservableCollection<IngredientVm> Ingredients { get; set; } = new();
        
        public override string ToString() => Name;
    }
}
