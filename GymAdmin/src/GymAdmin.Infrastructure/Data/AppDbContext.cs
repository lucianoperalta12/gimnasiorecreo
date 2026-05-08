using GymAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Routine> Routines => Set<Routine>();
    public DbSet<RoutineExercise> RoutineExercises => Set<RoutineExercise>();
    public DbSet<StudentRoutine> StudentRoutines => Set<StudentRoutine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
