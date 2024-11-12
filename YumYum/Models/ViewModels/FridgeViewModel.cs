namespace YumYum.Models.ViewModels
{
    public class FridgeViewModel
    {
        public List<FridgeItemViewModel>? RefrigeratorData { get; set; }
        public List<IngredientViewModel>? IngredientData { get; set; }
    }

    public class FridgeItemViewModel
    {
        public int UserID { get; set; }
        public string? IngredientName { get; set; }
        public string? IngredientIcon { get; set; }
        public string? Quantity { get; set; }
        public string? UnitName { get; set; }
        public DateOnly ValidDate { get; set; }
    }

    public class IngredientViewModel
    {
        public string? IngredientName { get; set; }
        public string? IngredientIcon { get; set; }
    }
}
