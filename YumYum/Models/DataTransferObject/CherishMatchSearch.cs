using Microsoft.AspNetCore.Mvc;

namespace YumYum.Models.DataTransferObject
{
    public class CherishMatchSearch
    {
        [FromForm(Name = "RegionSelect")]
        public short RegionId { get; set; }

        [FromForm(Name = "CitySelect")]
        public string? CityKey { get; set; }

        [FromForm(Name = "IngredientSelect")]
        public string? IngredientName { get; set; }
    }
}
