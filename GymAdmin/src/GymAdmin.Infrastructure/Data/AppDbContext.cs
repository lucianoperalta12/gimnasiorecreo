using GymAdmin.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GymAdmin.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Gym> Gyms => Set<Gym>();
    public DbSet<GymUser> GymUsers => Set<GymUser>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Routine> Routines => Set<Routine>();
    public DbSet<RoutineExercise> RoutineExercises => Set<RoutineExercise>();
    public DbSet<StudentRoutine> StudentRoutines => Set<StudentRoutine>();
    public DbSet<MembershipPlan> MembershipPlans => Set<MembershipPlan>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<MembershipPayment> MembershipPayments => Set<MembershipPayment>();
    public DbSet<Ingreso> Ingresos => Set<Ingreso>();
    public DbSet<Egreso> Egresos => Set<Egreso>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
