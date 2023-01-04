﻿using ManagementTool.Shared.Models.Database;
using ManagementTool.Shared.Models.Utils;
using Microsoft.EntityFrameworkCore;

namespace ManagementTool.Server.Services;

public class ManToolDbContext : DbContext {
    public ManToolDbContext(DbContextOptions<ManToolDbContext> options) : base(options) {
    }

    public virtual DbSet<User> User { get; set; }
    public virtual DbSet<Role> Role { get; set; }
    public virtual DbSet<Assignment> Assignment { get; set; }
    public virtual DbSet<Project> Project { get; set; }
    public virtual DbSet<UserRoleXRefs> UserRoleXRefs { get; set; }
    public virtual DbSet<UserProjectXRefs> UserProjectXRefs { get; set; }


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

        base.OnModelCreating(modelBuilder);

        /* todo do something like this
         
        var entryPoint = (from ep in dbContext.tbl_EntryPoint
            join e in dbContext.tbl_Entry on ep.EID equals e.EID
            join t in dbContext.tbl_Title on e.TID equals t.TID
            where e.OwnerID == user.UID
            select new
            {
                UID = e.OwnerID,
                TID = e.TID,
                Title = t.Title,
                EID = e.EID
            }).Take(10);
        */
    }


    private void SetupUserEntity(ModelBuilder modelBuilder) {
        modelBuilder.Entity<User>(entity => {
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
        });
    }


    private void SetupRoleEntity(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Role>(entity => {
            entity.ToTable("Role");
            entity.Property(e => e.Id)
                .IsRequired()
                .HasColumnName("id_role")
                .HasDefaultValueSql("nextval('account.item_id_seq'::regclass)");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnName("name");
            entity.Property(e => e.Type)
                .IsRequired()
                .HasColumnName("type")
                .HasConversion<ERoleType>();
            entity.Property(e => e.ProjectId)
                .HasColumnName("id_project");
        });
    }


    private void SetupAssignmentEntity(ModelBuilder modelBuilder) {
        //todo remove
        return;
        modelBuilder.Entity<Assignment>(entity => {
            entity.ToTable("Role");
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
            entity.Property(e => e.State)
                .HasColumnName("state")
                .HasConversion<EAssignmentState>();
        });
    }


    private void SetupProjectEntity(ModelBuilder modelBuilder) {
        //todo remove
        return;
        modelBuilder.Entity<Project>(entity => {
            entity.ToTable("Project");
            entity.Property(e => e.Id)
                .IsRequired()
                .HasColumnName("id_project")
                .HasDefaultValueSql("nextval('account.item_id_seq'::regclass)");
            entity.Property(e => e.ProjectName)
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
        modelBuilder.Entity<UserRoleXRefs>(entity => {
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
                //todo possible different more complex conversion needed
                .HasConversion<DateTime>();
        });
    }


    private void SetupUserProjectXRefsEntity(ModelBuilder modelBuilder) {
        modelBuilder.Entity<UserProjectXRefs>(entity => {
            entity.ToTable("UserRoleXRefs");
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
                //todo
                .HasConversion<DateTime>();
        });
    }
}