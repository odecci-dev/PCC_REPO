﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace API_PCC.EntityModels;
public partial class UserUserTypeAccessTable
{
    public int Id { get; set; }

    public int? UserTypeId { get; set; }

    public int? Module { get; set; }

    public DateTime? DateCreated { get; set; }

    public int? ActionId { get; set; }
}