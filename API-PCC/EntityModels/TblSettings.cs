﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace API_PCC.Models;

public partial class TblSettings
{
    public int Id { get; set; }

    public string BusinessName { get; set; }

    public string Address { get; set; }

    public string ContactNumber { get; set; }

    public string Email { get; set; }

    public string HerdCodeLength { get; set; }

    public string IdNumberLength { get; set; }

    public string BreedRegistryNumberLength { get; set; }

    public string PedigreeCertSignatoryFirstName { get; set; }

    public string PedigreeCertSignatoryLastName { get; set; }

    public string PedigreeCertSignatorySignature { get; set; }

    public string PedigreeSignatoryFirstName { get; set; }

    public string PedigreeSignatoryLastName { get; set; }

    public string PedigreeSignatorySignature { get; set; }

    public string Watermark { get; set; }

    public DateTime? DateUpdated { get; set; }

    public string UpdatedBy { get; set; }
}