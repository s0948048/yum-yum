namespace YumYum.Models.Customer
{
    public class RefrigratorView
    {
        public int UserID { get; set; }
        public string IngredientName { get; set; }
        public string Quantity { get; set; }
        public string UnitName { get; set; }
        public DateOnly ValidDate { get; set; }
    }
}
