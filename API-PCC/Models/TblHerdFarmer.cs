﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
namespace API_PCC.EntityModels;


public partial class TblHerdFarmer
{
    public int? Id { get; set; }

    public int? HerdId { get; set; }

    public int? FarmerId { get; set; }

    public int? CreatedBy { get; set; }

    public DateTime? CreatedAt { get; set; }

    public int? DeletedBy { get; set; }

    public DateTime? DeletedAt { get; set; }

    public bool IsDeleted { get; set; }
}