using l_hospital_mang.Data.Models;
using Microsoft.EntityFrameworkCore;
using Type = l_hospital_mang.Data.Models.Type;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace l_hospital_mang.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }


        public DbSet<Medical_Health> Medical_Healths { get; set; }
        public DbSet<Clinicscs> Clinicscss { get; set; }
        public DbSet<patient> Patients { get; set; }
        public DbSet<Resident_patients> Resident_patientss { get; set; }
        public DbSet<Dates> Datess { get; set; }
        public DbSet<Consulting_reservation> Consulting_reservations { get; set; }
        public DbSet<Doctors> Doctorss { get; set; }
        public DbSet<Shifts> Shiftss { get; set; }
        public DbSet<AmbulanceRequest> AmbulanceRequests { get; set; }

        public DbSet<CAmbulance_Car> CAmbulance_Car { get; set; }
        public DbSet<Employees> Employeess { get; set; }
        public DbSet<Analysis> Analysiss { get; set; }
        public DbSet<Radiography> Radiographyies { get; set; }
        public DbSet<Advertisments> Advertismentss { get; set; }
        public DbSet<invoice> invoices { get; set; }
        public DbSet<Type> Types { get; set; }
        public DbSet<surgery_reservations> surgery_reservationss { get; set; }
        public DbSet<Rooms> Room { get; set; }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // علاقة one to many بين المريض والحجوزات
            modelBuilder.Entity<Consulting_reservation>()
                .HasOne(r => r.Patient)
                .WithMany(p => p.Consulting_reservations)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AmbulanceRequest>()
         .HasOne(r => r.Car)
         .WithMany(c => c.Requests)
         .HasForeignKey(r => r.CarId)
         .OnDelete(DeleteBehavior.SetNull); 

            modelBuilder.Entity<CAmbulance_Car>()
                .HasIndex(c => c.CarNumber)
                .IsUnique();
            modelBuilder.Entity<Consulting_reservation>()
                .HasOne(r => r.Analysis)
                .WithOne(a => a.Consulting_reservation)
                .HasForeignKey<Analysis>(a => a.Consulting_reservationId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة one to one بين الحجوزات والمواعيد
            modelBuilder.Entity<Consulting_reservation>()
       .HasOne(cr => cr.Date)                 
       .WithMany()                             
       .HasForeignKey(cr => cr.DateId)         
       .OnDelete(DeleteBehavior.Restrict);


            // علاقة one to one بين الحجوزات والتصوير الشعاعي
            modelBuilder.Entity<Consulting_reservation>()
                .HasOne(r => r.Radiography)
                .WithOne(rg => rg.Consulting_reservation)
                .HasForeignKey<Radiography>(rg => rg.Consulting_reservationId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة one to one بين الموظف والنوع
            modelBuilder.Entity<Employees>()
                .HasOne(e => e.Type)
                .WithOne(t => t.Employee)
                .HasForeignKey<Employees>(e => e.TypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة one to many بين الطبيب والمواعيد
            modelBuilder.Entity<Dates>()
                .HasOne(d => d.Doctor)
                .WithMany(d => d.Dates)
                .HasForeignKey(d => d.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة one to many بين العيادات والأطباء
            modelBuilder.Entity<Doctors>()
                .HasOne(d => d.Clinic)
                .WithMany(c => c.Doctors)
                .HasForeignKey(d => d.ClinicId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة one to one بين العيادات والإعلانات
            modelBuilder.Entity<Advertisments>()
                .HasOne(a => a.Clinic)
                .WithOne(c => c.Advertisments)
                .HasForeignKey<Advertisments>(a => a.ClinicId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة one to many بين الإقامات والفواتير
            modelBuilder.Entity<invoice>()
                .HasOne(i => i.ResidentPatient)
                .WithMany(r => r.Invoices)
                .HasForeignKey(i => i.ResidentPatientId)
                .OnDelete(DeleteBehavior.Cascade);

           

            // علاقة many to many بين الطبيب والمناوبات
            modelBuilder.Entity<doctors_shifts>()
                .HasKey(ds => new { ds.DoctorId, ds.ShiftId });

            modelBuilder.Entity<doctors_shifts>()
                .HasOne(ds => ds.Doctor)
                .WithMany(d => d.DoctorShifts)
                .HasForeignKey(ds => ds.DoctorId);

            modelBuilder.Entity<doctors_shifts>()
                .HasOne(ds => ds.Shift)
                .WithMany(s => s.DoctorShifts)
                .HasForeignKey(ds => ds.ShiftId);

            // علاقة one to one بين الغرف والمقيمين
            modelBuilder.Entity<Resident_patients>()
                .HasOne(rp => rp.Room)
                .WithOne(r => r.Resident_patient)
                .HasForeignKey<Resident_patients>(rp => rp.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // علاقة one to many بين المريض والسجل الطبي
            modelBuilder.Entity<patient>()
                .HasMany(p => p.Medical_Healths)
                .WithOne(mh => mh.Patient)
                .HasForeignKey(mh => mh.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة one to many بين الحجز الجراحي والمريض
            modelBuilder.Entity<surgery_reservations>()
                .HasOne(sr => sr.Patient)
                .WithMany(p => p.SurgeryReservations)
                .HasForeignKey(sr => sr.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            // علاقة one to many بين الحجز الجراحي والطبيب
            modelBuilder.Entity<surgery_reservations>()
                .HasOne(sr => sr.Doctor)
                .WithMany(d => d.SurgeryReservations)
                .HasForeignKey(sr => sr.DoctorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AmbulanceRequest>()
        .HasOne(ar => ar.AcceptedByEmployee)
        .WithMany() 
        .HasForeignKey(ar => ar.AcceptedByEmployeeId)
        .OnDelete(DeleteBehavior.SetNull);
            // one to many patient و AmbulanceRequest
            modelBuilder.Entity<AmbulanceRequest>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.AmbulanceRequests)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict);


        }
    }
}
