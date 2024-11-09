namespace YumYum.Models
{
    public class RecipeWatch
    {
        public string? recipeName { get; set; }

        public string? recipeClassName { get; set; }

        public string? recipeImage {  get; set; }
        public string? recipeDescription { get; set; }

        public short recipeFinishMinute { get; set; }

        public byte reciptPersonQuantity { get; set; }

        public int recipeStepNumber { get; set; }
        public string? recipeStepsImage { get; set; }

        public string? recipeStepDescription {  get; set; }



    }
}
