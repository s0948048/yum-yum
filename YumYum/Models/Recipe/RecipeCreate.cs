namespace YumYum.Models
{
    public class RecipeCreate
    {
        public List<Ingredient>? Ingredients { get; set; }
        public List<RecipeClass>? className { get; set; }
        public List<Unit>? units { get; set; }
    }
}
