namespace YumYum.Models
{
    public class RecipeWatch_Brief
    {
        //食譜名稱
        public string? recipeName { get; set; }
        //完成時間
        public short recipeFinishTime { get; set; }
        //食用人數
        public byte recipePeople { get; set; }
        //食譜類別
        public string? className { get; set; }

    }
}
