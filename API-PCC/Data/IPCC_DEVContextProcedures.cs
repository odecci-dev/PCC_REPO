﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using API_PCC.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace API_PCC.Data
{
    public partial interface IPCC_DEVContextProcedures
    {
        Task<List<GetUserLogInResult>> GetUserLogInAsync(string Username, string Password, OutputParameter<int> returnValue = null, CancellationToken cancellationToken = default);
    }
}
