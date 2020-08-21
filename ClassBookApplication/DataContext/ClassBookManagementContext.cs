using ClassBookApplication.Domain.CareerExpert;
using ClassBookApplication.Domain.Classes;
using ClassBookApplication.Domain.Common;
using ClassBookApplication.Domain.School;
using ClassBookApplication.Domain.Student;
using ClassBookApplication.Domain.Teacher;
using Microsoft.EntityFrameworkCore;

namespace ClassBookApplication.DataContext
{
    public class ClassBookManagementContext : DbContext
    {
        public ClassBookManagementContext(DbContextOptions<ClassBookManagementContext> options)
            : base(options)
        {
        }

        #region Common

        public DbSet<Board> Board { get; set; }
        public DbSet<States> States { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Standards> Standards { get; set; }
        public DbSet<Medium> Medium { get; set; }
        public DbSet<Subjects> Subjects { get; set; }
        public DbSet<SubjectSpeciality> SubjectSpeciality { get; set; }
        public DbSet<SubjectSpecialityMapping> SubjectSpecialityMapping { get; set; }
        public DbSet<CourseCategory> CourseCategory { get; set; }
        public DbSet<CourseCategoryMapping> CourseCategoryMapping { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<BoardMapping> BoardMapping { get; set; }
        public DbSet<StandardMediumBoardMapping> StandardMediumBoardMapping { get; set; }

        #endregion

        #region ClassBookMain

        public DbSet<Student> Student { get; set; }
        public DbSet<Classes> Classes { get; set; }
        public DbSet<Teacher> Teacher { get; set; }
        public DbSet<CareerExpert> CareerExpert { get; set; }
        public DbSet<School> School { get; set; }

        #endregion

        #region OnModelCreating

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Classes>().Ignore(t => t.MappingRequestModel);
            modelBuilder.Entity<Teacher>().Ignore(t => t.MappingRequestModel);
            modelBuilder.Entity<CareerExpert>().Ignore(t => t.MappingRequestModel);
            modelBuilder.Entity<School>().Ignore(t => t.MappingRequestModel);
            base.OnModelCreating(modelBuilder);
        }

        #endregion
    }
}
