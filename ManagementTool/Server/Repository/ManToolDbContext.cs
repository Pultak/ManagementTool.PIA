using ManagementTool.Server.Models.Database;
using ManagementTool.Shared.Models.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ManagementTool.Server.Repository;

public class ManToolDbContext : DbContext {
    public ManToolDbContext(DbContextOptions<ManToolDbContext> options) : base(options) {
    }

    public virtual DbSet<UserDAL>? User { get; set; }
    public virtual DbSet<RoleDAL>? Role { get; set; }
    public virtual DbSet<AssignmentDAL>? Assignment { get; set; }
    public virtual DbSet<ProjectDAL>? Project { get; set; }
    public virtual DbSet<UserRoleXRefsDAL>? UserRoleXRefs { get; set; }
    public virtual DbSet<UserProjectXRefsDAL>? UserProjectXRefs { get; set; }
    public virtual DbSet<UserSuperiorXRefsDAL>? UserSuperiorXRefs { get; set; }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        var dbConnectionString = configuration.GetValue<string>("DBPosgreSQL");
        optionsBuilder.UseNpgsql(dbConnectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        //setup default scheme
        modelBuilder.HasDefaultSchema("manTool");

        SetupUserEntity(modelBuilder);
        SetupAssignmentEntity(modelBuilder);
        SetupProjectEntity(modelBuilder);
        SetupRoleEntity(modelBuilder);
        SetupUserRoleXRefsEntity(modelBuilder);
        SetupUserProjectXRefsEntity(modelBuilder);
        SetupUserSuperiorXRefsEntity(modelBuilder);

        base.OnModelCreating(modelBuilder);
    }


    private void SetupUserEntity(ModelBuilder modelBuilder) {
        modelBuilder.Entity<UserDAL>(entity => {
            entity.ToTable("User");
            entity.Property(e => e.Id)
                .IsRequired()
                .HasColumnName("id_user")
                .HasDefaultValueSql("nextval('account.item_id_seq'::regclass)");
            entity.Property(e => e.Username).HasColumnName("username");
            entity.Property(e => e.Pwd)
                .IsRequired()
                .HasColumnName("pwd");
            entity.Property(e => e.FullName)
                .IsRequired()
                .HasColumnName("full_name");
            entity.Property(e => e.PrimaryWorkplace)
                .IsRequired()
                .HasColumnName("primary_workplace");
            entity.Property(e => e.EmailAddress)
                .IsRequired()
                .HasColumnName("email_address");
            entity.Property(e => e.Username)
                .IsRequired()
                .HasColumnName("username");
            entity.Property(e => e.PwdInit)
                .IsRequired()
                .HasColumnName("pwd_changed");
            entity.Property(e => e.Salt)
                .IsRequired()
                .HasColumnName("salt");
        });
    }


    private void SetupRoleEntity(ModelBuilder modelBuilder) {
        modelBuilder.Entity<RoleDAL>(entity => {
            entity.ToTable("Role");
            entity.Property(e => e.Id)
                .IsRequired()
                .HasColumnName("id_role")
                .HasDefaultValueSql("nextval('id_seq'::regclass)");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name");

            var converter = new ValueConverter<ERoleType, string>(
                v => v.ToString(),
                v => (ERoleType)Enum.Parse(typeof(ERoleType), v));
            entity.Property(e => e.Type)
                .IsRequired()
                .HasColumnName("type")
                .HasConversion(converter);
            entity.Property(e => e.ProjectId)
                .HasColumnName("id_project");
        });
    }


    private void SetupAssignmentEntity(ModelBuilder modelBuilder) {
        modelBuilder.Entity<AssignmentDAL>(entity => {
            entity.ToTable("Assignment");
            entity.Property(e => e.Id)
                .IsRequired()
                .HasColumnName("id_assignment")
                .HasDefaultValueSql("nextval('account.item_id_seq'::regclass)");
            entity.Property(e => e.ProjectId)
                .HasColumnName("id_project");
            entity.Property(e => e.UserId)
                .HasColumnName("id_user");
            entity.Property(e => e.AllocationScope)
                .HasColumnName("allocation_scope");
            entity.Property(e => e.Name)
                .HasColumnName("name");
            entity.Property(e => e.Note)
                .HasColumnName("note");
            entity.Property(e => e.FromDate)
                .HasColumnName("from_date");
            entity.Property(e => e.ToDate)
                .HasColumnName("to_date");

            var converter = new ValueConverter<EAssignmentState, string>(
                v => v.ToString(),
                v => (EAssignmentState)Enum.Parse(typeof(EAssignmentState), v));
            entity.Property(e => e.State)
                .HasColumnName("state")
                .HasConversion(converter);
        });
    }


    private void SetupProjectEntity(ModelBuilder modelBuilder) {
        modelBuilder.Entity<ProjectDAL>(entity => {
            entity.ToTable("Project");
            entity.Property(e => e.Id)
                .IsRequired()
                .HasColumnName("id_project")
                .HasDefaultValueSql("nextval('account.item_id_seq'::regclass)");
            entity.Property(e => e.ProjectName)
                .IsRequired()
                .HasColumnName("project_name");
            entity.Property(e => e.FromDate)
                .HasColumnName("from_date")
                .HasConversion<DateTime>();
            entity.Property(e => e.ToDate)
                .HasColumnName("to_date")
                .HasConversion<DateTime>();
            entity.Property(e => e.Description)
                .IsRequired()
                .HasColumnName("description");
        });
    }


    private void SetupUserRoleXRefsEntity(ModelBuilder modelBuilder) {
        modelBuilder.Entity<UserRoleXRefsDAL>(entity => {
            entity.ToTable("UserRoleXRefs");
            entity.Property(e => e.Id)
                .IsRequired()
                .HasColumnName("id");
            entity.Property(e => e.IdUser)
                .IsRequired()
                .HasColumnName("id_user");
            entity.Property(e => e.IdRole)
                .IsRequired()
                .HasColumnName("id_role");
            entity.Property(e => e.AssignedDate)
                .IsRequired()
                .HasColumnName("assigned_date")
                .HasConversion<DateTime>();
        });
    }


    private void SetupUserProjectXRefsEntity(ModelBuilder modelBuilder) {
        modelBuilder.Entity<UserProjectXRefsDAL>(entity => {
            entity.ToTable("UserProjectXRefs");
            entity.Property(e => e.Id)
                .IsRequired()
                .HasColumnName("id");
            entity.Property(e => e.IdUser)
                .IsRequired()
                .HasColumnName("id_user");
            entity.Property(e => e.IdProject)
                .IsRequired()
                .HasColumnName("id_project");
            entity.Property(e => e.AssignedDate)
                .IsRequired()
                .HasColumnName("assigned_date")
                .HasConversion<DateTime>();
        });
    }

    private void SetupUserSuperiorXRefsEntity(ModelBuilder modelBuilder) {
        modelBuilder.Entity<UserSuperiorXRefsDAL>(entity => {
            entity.ToTable("UserSuperiorXRefs");
            entity.Property(e => e.Id)
                .IsRequired()
                .HasColumnName("id");
            entity.Property(e => e.IdUser)
                .IsRequired()
                .HasColumnName("id_user");
            entity.Property(e => e.IdSuperior)
                .IsRequired()
                .HasColumnName("id_superior_user");
            entity.Property(e => e.AssignedDate)
                .IsRequired()
                .HasColumnName("assigned_date")
                .HasConversion<DateTime>();
        });
    }
}