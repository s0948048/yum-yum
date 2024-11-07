using System;
using System.Collections.Generic;

namespace YumYum.Models;

public partial class RecipeBrief
{
    public short RecipeId { get; set; }

    public string RecipeName { get; set; } = null!;

    public short RecipeClassId { get; set; }

    public string RecipeShot { get; set; } = null!;

    public string? RecipeDescript { get; set; }

    public short FinishMinute { get; set; }

    public int CreateUserId { get; set; }

    public byte PersonQuantity { get; set; }

    public DateOnly CreateDate { get; set; }

    public short ClickTime { get; set; }

    public byte RecipeStateCode { get; set; }

    public byte? EditFieldId { get; set; }

    public string? EditDescript { get; set; }

    public virtual RecipeEditField? EditField { get; set; }

    public virtual RecipeClass RecipeClass { get; set; } = null!;

    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();

    public virtual RecipeState RecipeStateCodeNavigation { get; set; } = null!;

    public virtual ICollection<RecipeStep> RecipeSteps { get; set; } = new List<RecipeStep>();

    public virtual ICollection<UserSecretInfo> Users { get; set; } = new List<UserSecretInfo>();
}
