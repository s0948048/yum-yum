using System;
using System.Collections.Generic;

namespace YumYum.Models;

public partial class CherishOrder
{
    public int CherishId { get; set; }

    public int GiverUserId { get; set; }

    public DateOnly EndDate { get; set; }

    public short IngredientId { get; set; }

    public short Quantity { get; set; }

    public string CherishPhoto { get; set; } = null!;

    public byte TradeStateCode { get; set; }

    public DateOnly SubmitDate { get; set; }

    public DateOnly? UpdateDate { get; set; }

    public virtual ICollection<CherishOrderApplicant> CherishOrderApplicants { get; set; } = new List<CherishOrderApplicant>();

    public virtual CherishOrderInfo? CherishOrderInfo { get; set; }

    public virtual UserSecretInfo GiverUser { get; set; } = null!;

    public virtual Ingredient Ingredient { get; set; } = null!;

    public virtual TradeState TradeStateCodeNavigation { get; set; } = null!;
}
