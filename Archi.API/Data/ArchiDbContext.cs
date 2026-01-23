using Microsoft.EntityFrameworkCore;
using Archi.API.Models;

namespace Archi.API.Data;

/// <summary>
/// Service 
/// </summary>
public class ArchiDbContext : DbContext
{
    public ArchiDbContext(DbContextOptions<ArchiDbContext> options) : base(options)
    {
    }

    public DbSet<TacosModel> Tacos { get; set; }
}