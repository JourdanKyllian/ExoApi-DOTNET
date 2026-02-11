using Microsoft.EntityFrameworkCore;
using Archi.API.Models;
using Archi.Api.Models;

namespace Archi.API.Data;

/// <summary>
/// Service 
/// </summary>
public class ArchiDbContext : BaseDbContext
{
    public ArchiDbContext(DbContextOptions<ArchiDbContext> options) : base(options) { }


    public DbSet<TacosModel> Tacos { get; set; }
    public DbSet<PizzaModel> Pizza { get; set; }
}

