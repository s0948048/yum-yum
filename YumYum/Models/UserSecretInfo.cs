using System;
using System.Collections.Generic;

namespace YumYum.Models;

public partial class UserSecretInfo
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int? GoogleId { get; set; }

    public bool EmailChecked { get; set; }

    public string? EmailValidCode { get; set; }

    public virtual CherishDefaultInfo? CherishDefaultInfo { get; set; }

    public virtual ICollection<CherishOrderApplicant> CherishOrderApplicants { get; set; } = new List<CherishOrderApplicant>();

    public virtual ICollection<CherishOrder> CherishOrders { get; set; } = new List<CherishOrder>();

    public virtual ICollection<RefrigeratorStore> RefrigeratorStores { get; set; } = new List<RefrigeratorStore>();

    public virtual UserBio? UserBio { get; set; }

    public virtual UserDetail? UserDetail { get; set; }

    public virtual ICollection<RecipeBrief> Recipes { get; set; } = new List<RecipeBrief>();
}
