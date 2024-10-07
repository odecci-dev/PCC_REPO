﻿using API_PCC.ApplicationModels.Common;
using API_PCC.DtoModels;
using API_PCC.Models;
using static API_PCC.Controllers.HerdFarmerController;

namespace API_PCC.ApplicationModels
{
    public class HerdFarmerPageModel : PaginationModel
    {
        public List<BreedRegistryHerd> items { get; set; }
    }
}
