using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace smart_home_server.Home.ResourceModels;

public class SearchOptionsQuery
{
    [FromQuery]
    [RangeAttribute(1, 20)]
    public int? RecordPerPage { get; set; }

    [FromQuery]
    [RangeAttribute(1, int.MaxValue)]
    public int? Page { get; set; }

    [FromQuery]
    public string? Name { get; set; }
}