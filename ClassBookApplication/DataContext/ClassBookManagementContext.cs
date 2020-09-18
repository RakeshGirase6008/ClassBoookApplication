using ClassBookApplication.Domain.CareerExpert;
using ClassBookApplication.Domain.Classes;
using ClassBookApplication.Domain.Common;
using ClassBookApplication.Domain.School;
using ClassBookApplication.Domain.Student;
using ClassBookApplication.Domain.Teacher;
using ClassBookApplication.Domain.Topics;
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

        public DbSet<Settings> Settings { get; set; }
        public DbSet<Board> Board { get; set; }
        public DbSet<States> States { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Pincode> Pincode { get; set; }
        public DbSet<Standards> Standards { get; set; }
        public DbSet<Medium> Medium { get; set; }
        public DbSet<Subjects> Subjects { get; set; }
        public DbSet<SubjectSpeciality> SubjectSpeciality { get; set; }
        public DbSet<SubjectSpecialityMapping> SubjectSpecialityMapping { get; set; }
        public DbSet<CourseCategory> CourseCategory { get; set; }
        public DbSet<CourseCategoryMapping> CourseCategoryMapping { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<StandardMediumBoardMapping> StandardMediumBoardMapping { get; set; }
        public DbSet<ShoppingCartSubjects> ShoppingCartSubjects { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderSubjects> OrderSubjects { get; set; }
        public DbSet<Topic> Topic { get; set; }
        public DbSet<SubTopic> SubTopic { get; set; }
        public DbSet<AuthorizeDeviceData> AuthorizeDeviceData { get; set; }
        public DbSet<Ratings> Ratings { get; set; }

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
            base.OnModelCreating(modelBuilder);
        }
        #endregion
    }
}
