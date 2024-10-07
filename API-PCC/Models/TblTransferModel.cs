﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace API_PCC.Models;

public partial class TblTransferModel
{
    public int Id { get; set; }

    public string TransferNumber { get; set; }

    public int? Animal { get; set; }

    public int? Owner { get; set; }

    public string Address { get; set; }

    public string TelephoneNumber { get; set; }

    public string MobileNumber { get; set; }

    public string Email { get; set; }

    public byte[] TransferFile { get; set; }

    public string CreatedBy { get; set; }

    public DateTime? DateCreated { get; set; }

    public string UpdatedBy { get; set; }

    public DateTime? DateUpdated { get; set; }

    public string DeletedBy { get; set; }

    public DateTime? DateDeleted { get; set; }

    public string RestoredBy { get; set; }

    public DateTime? DateRestored { get; set; }

    public bool? DeleteFlag { get; set; }

    public int? Status { get; set; }
}