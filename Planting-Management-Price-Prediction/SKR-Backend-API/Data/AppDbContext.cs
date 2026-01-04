using Microsoft.EntityFrameworkCore;
using SKR_Backend_API.Models;

namespace SKR_Backend_API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<FarmRecord> Farms { get; set; } = null!;
    public DbSet<FarmTask> FarmTasks { get; set; } = null!;
    public DbSet<Season> HarvestSeasons { get; set; } = null!;
    public DbSet<Session> HarvestSessions { get; set; } = null!;
    public DbSet<AgronomyTemplate> AgronomyTemplates { get; set; } = null!;
    public DbSet<EmergencyTemplate> EmergencyTemplates { get; set; } = null!;
    public DbSet<BlackPepperVariety> PepperVarieties { get; set; } = null!;
    public DbSet<District> Districts { get; set; } = null!;
    public DbSet<SoilType> SoilTypes { get; set; } = null!;
    public DbSet<DistrictSoil> DistrictSoils { get; set; } = null!;
    public DbSet<AgronomyGuide> AgronomyGuides { get; set; } = null!;
    public DbSet<GuideStep> GuideSteps { get; set; } = null!;
    public DbSet<PepperKnowledge> PepperKnowledge { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure User entity
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasDefaultValueSql("gen_random_uuid()");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        // Configure FarmRecord entity
        modelBuilder.Entity<FarmRecord>(entity =>
        {
            entity.ToTable("farms");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            
            // Foreign key relationship to User
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .HasPrincipalKey(u => u.Id)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.UserId).IsRequired().HasColumnName("userid");
            entity.Property(e => e.FarmName).IsRequired().HasMaxLength(255).HasColumnName("farmname");
            entity.Property(e => e.DistrictId).HasColumnName("districtid");
            entity.Property(e => e.SoilTypeId).HasColumnName("soiltypeid");
            entity.Property(e => e.ChosenVarietyId).HasMaxLength(50).HasColumnName("chosenvarietyid");
            entity.Property(e => e.FarmStartDate).HasColumnName("farmstartdate").HasColumnType("timestamp with time zone");
            entity.Property(e => e.AreaHectares).HasColumnName("areahectares").HasColumnType("numeric");
            entity.Property(e => e.TotalVines).HasColumnName("totalvines");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat").HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            // Foreign key relationships for district and soil type
            // Configure navigation properties explicitly to prevent EF Core from accessing them as columns
            entity.HasOne(e => e.DistrictNavigation)
                .WithMany()
                .HasForeignKey(e => e.DistrictId)
                .HasPrincipalKey(d => d.Id)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.SoilTypeNavigation)
                .WithMany()
                .HasForeignKey(e => e.SoilTypeId)
                .HasPrincipalKey(s => s.Id)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.VarietyNavigation)
                .WithMany()
                .HasForeignKey(e => e.ChosenVarietyId)
                .HasPrincipalKey(v => v.Id)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure FarmTask entity
        modelBuilder.Entity<FarmTask>(entity =>
        {
            entity.ToTable("farmtasks");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            
            // Foreign key relationship to Farm
            entity.HasOne<FarmRecord>()
                .WithMany()
                .HasForeignKey(e => e.FarmId)
                .HasPrincipalKey(f => f.Id)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.FarmId).IsRequired().HasColumnName("farmid");
            entity.Property(e => e.TaskName).IsRequired().HasMaxLength(255).HasColumnName("taskname");
            entity.Property(e => e.Phase).HasMaxLength(50).HasColumnName("phase");
            entity.Property(e => e.TaskType).HasMaxLength(50).HasColumnName("tasktype");
            entity.Property(e => e.VarietyKey).HasMaxLength(50).HasColumnName("varietykey");
            entity.Property(e => e.DueDate).HasColumnName("duedate").HasColumnType("timestamp with time zone");
            entity.Property(e => e.Status).HasMaxLength(20).HasColumnName("status");
            entity.Property(e => e.DateCompleted).HasColumnName("datecompleted").HasColumnType("timestamp with time zone");
            entity.Property(e => e.InputDetailsJson).HasColumnName("inputdetails").HasColumnType("jsonb");
            entity.Property(e => e.DetailedStepsJson).HasColumnName("detailedsteps").HasColumnType("jsonb");
            entity.Property(e => e.ReasonWhy).HasColumnName("reasonwhy").HasColumnType("text");
            entity.Property(e => e.IsManual).HasColumnName("ismanual").HasDefaultValue(false);
            entity.Property(e => e.Priority).HasMaxLength(20).HasColumnName("priority");
            entity.Property(e => e.CreatedAt).HasColumnName("createdat").HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasIndex(e => e.FarmId);
        });

        // Configure Season entity
        modelBuilder.Entity<Season>(entity =>
        {
            entity.ToTable("HarvestSeasons");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            
            entity.HasOne<FarmRecord>()
                .WithMany()
                .HasForeignKey(e => e.FarmId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(e => e.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.Property(e => e.FarmId).IsRequired().HasColumnName("farmid");
            entity.Property(e => e.CreatedBy).IsRequired().HasColumnName("createdby");
            entity.Property(e => e.SeasonName).HasMaxLength(100).HasColumnName("seasonname");
            entity.Property(e => e.StartMonth).HasColumnName("startmonth");
            entity.Property(e => e.StartYear).HasColumnName("startyear");
            entity.Property(e => e.EndMonth).HasColumnName("endmonth");
            entity.Property(e => e.EndYear).HasColumnName("endyear");
        });

        // Configure Session entity
        modelBuilder.Entity<Session>(entity =>
        {
            entity.ToTable("harvestsessions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            
            entity.HasOne<Season>()
                .WithMany()
                .HasForeignKey(e => e.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(e => e.SeasonId).IsRequired().HasColumnName("seasonid");
            entity.Property(e => e.SessionName).IsRequired().HasMaxLength(255).HasColumnName("sessionname");
            entity.Property(e => e.Date).HasColumnName("date").HasColumnType("timestamp with time zone");
            entity.Property(e => e.YieldKg).HasColumnName("yieldkg").HasColumnType("numeric");
            entity.Property(e => e.AreaHarvested).HasColumnName("areaharvested").HasColumnType("numeric");
            entity.Property(e => e.Notes).HasColumnName("notes").HasColumnType("text");
        });

        // Configure AgronomyTemplate entity
        modelBuilder.Entity<AgronomyTemplate>(entity =>
        {
            entity.ToTable("agronomytemplates");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.TaskName).IsRequired().HasMaxLength(255).HasColumnName("taskname");
            entity.Property(e => e.Phase).HasMaxLength(50).HasColumnName("phase");
            entity.Property(e => e.TimingDaysAfterStartingOfFarm).HasColumnName("timingdaysafterstarting");
            entity.Property(e => e.InstructionalDetails).HasColumnName("instructionaldetails").HasColumnType("text");
            entity.Property(e => e.TaskType).HasMaxLength(50).HasColumnName("tasktype");
            entity.Property(e => e.VarietyKey).HasMaxLength(50).HasColumnName("varietykey");
        });

        // Configure EmergencyTemplate entity
        modelBuilder.Entity<EmergencyTemplate>(entity =>
        {
            entity.ToTable("emergencytemplates");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id").HasDefaultValueSql("gen_random_uuid()");
            entity.Property(e => e.IssueName).HasMaxLength(255).HasColumnName("issuename");
            entity.Property(e => e.Symptoms).HasColumnName("symptoms").HasColumnType("text");
            entity.Property(e => e.TreatmentTask).HasMaxLength(255).HasColumnName("treatmenttask");
            entity.Property(e => e.Priority).HasMaxLength(20).HasColumnName("priority");
            entity.Property(e => e.Instructions).HasColumnName("instructions").HasColumnType("text");
        });

        // Configure BlackPepperVariety entity
        modelBuilder.Entity<BlackPepperVariety>(entity =>
        {
            entity.ToTable("PepperVarieties");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.SpacingMeters).HasMaxLength(50);
            entity.Property(e => e.PitDimensionsCm).HasMaxLength(50);
        });

        // Configure District entity
        modelBuilder.Entity<District>(entity =>
        {
            entity.ToTable("Districts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Name).IsUnique();
        });

        // Configure SoilType entity
        modelBuilder.Entity<SoilType>(entity =>
        {
            entity.ToTable("SoilTypes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TypeName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.TypeName).IsUnique();
        });

        // Configure DistrictSoil entity (junction table)
        modelBuilder.Entity<DistrictSoil>(entity =>
        {
            entity.ToTable("DistrictSoils");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DistrictId).IsRequired();
            entity.Property(e => e.SoilTypeId).IsRequired();
            
            // Foreign key relationships
            entity.HasOne(ds => ds.District)
                .WithMany()
                .HasForeignKey(ds => ds.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(ds => ds.SoilType)
                .WithMany()
                .HasForeignKey(ds => ds.SoilTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Composite unique constraint: District + SoilType combination must be unique
            entity.HasIndex(e => new { e.DistrictId, e.SoilTypeId }).IsUnique();
        });

        // Configure AgronomyGuide entity (junction table)
        modelBuilder.Entity<AgronomyGuide>(entity =>
        {
            entity.ToTable("agronomyguides");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DistrictId).IsRequired().HasColumnName("districtid");
            entity.Property(e => e.SoilTypeId).IsRequired().HasColumnName("soiltypeid");
            entity.Property(e => e.VarietyId).IsRequired().HasMaxLength(50).HasColumnName("varietyid");
            // CreatedAt column doesn't exist in the database table, so we exclude it
            // entity.Property(e => e.CreatedAt).HasColumnName("createdat").IsRequired(false);
            
            // Foreign key relationships
            entity.HasOne(ag => ag.District)
                .WithMany()
                .HasForeignKey(ag => ag.DistrictId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(ag => ag.SoilType)
                .WithMany()
                .HasForeignKey(ag => ag.SoilTypeId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(ag => ag.Variety)
                .WithMany()
                .HasForeignKey(ag => ag.VarietyId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // One-to-many relationship with GuideSteps
            entity.HasMany(ag => ag.Steps)
                .WithOne(gs => gs.Guide)
                .HasForeignKey(gs => gs.GuideId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Composite unique constraint: District + SoilType + Variety combination must be unique
            entity.HasIndex(e => new { e.DistrictId, e.SoilTypeId, e.VarietyId }).IsUnique();
        });

        // Configure GuideStep entity
        modelBuilder.Entity<GuideStep>(entity =>
        {
            entity.ToTable("GuideSteps");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.GuideId).IsRequired();
            entity.Property(e => e.StepNumber).IsRequired();
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            
            // Foreign key relationship
            entity.HasOne(gs => gs.Guide)
                .WithMany(ag => ag.Steps)
                .HasForeignKey(gs => gs.GuideId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Unique constraint: GuideId + StepNumber combination must be unique
            // Unique constraint: GuideId + StepNumber combination must be unique
            entity.HasIndex(e => new { e.GuideId, e.StepNumber }).IsUnique();
        });

        modelBuilder.HasPostgresExtension("vector");

        // Configure PepperKnowledge entity
        modelBuilder.Entity<PepperKnowledge>(entity =>
        {
            entity.ToTable("pepperknowledge");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(255);
            entity.Property(e => e.Content).HasColumnType("text");
            entity.Property(e => e.Embedding).HasColumnType("vector(1536)");
        });
        
        // Configure PostgreSQL naming convention (lowercase) - Run AFTER all entity configurations
        // This ensures table names and column names match PostgreSQL's default behavior (lowercase)
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var tableName = entityType.GetTableName();
            if (tableName != null && !tableName.Contains("_Legacy")) // Skip legacy tables
            {
                entityType.SetTableName(tableName.ToLowerInvariant());
            }
            
            // Convert all column names to lowercase
            foreach (var property in entityType.GetProperties())
            {
                var columnName = property.GetColumnName();
                if (columnName != null)
                {
                    property.SetColumnName(columnName.ToLowerInvariant());
                }
            }
        }
    }
}

