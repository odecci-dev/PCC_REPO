﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using API_PCC.EntityModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace API_PCC.Models;

public partial class ABuffAnimal
{
    public int Id { get; set; }
    public string AnimalIdNumber { get; set; }
    public string AnimalName { get; set; }
    public String Photo { get; set; }
    public string HerdCode { get; set; }
    public string RfidNumber { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string Sex { get; set; }
    public string BreedCode { get; set; }
    public string BirthType { get; set; }
    public string CountryOfBirth { get; set; }
    public int? OriginOfAcquisition { get; set; }
    public DateTime? DateOfAcquisition { get; set; }
    public string Marking { get; set; }
    public string TypeOfOwnership { get; set; }
    public int? BloodCode { get; set; }
    public bool DeleteFlag { get; set; }
    public string Status { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? CreatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdateDate { get; set; }
    public DateTime? DateDeleted { get; set; }
    public string? DeletedBy { get; set; }
    public DateTime? DateRestored { get; set; }
    public string? RestoredBy { get; set; }
    public decimal? BloodResult { get; set; }
    public string? breedRegistryNumber { get; set; }
   
}