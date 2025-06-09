using l_hospital_mang.Data.Models;

//using l_hospital_mang.Migrations;
using Microsoft.EntityFrameworkCore;
using CAmbulance_Car = l_hospital_mang.Data.Models.CAmbulance_Car;
using Consulting_reservation = l_hospital_mang.Data.Models.Consulting_reservation;
using Dates = l_hospital_mang.Data.Models.Dates;
using Doctors = l_hospital_mang.Data.Models.Doctors;
using doctors_shifts = l_hospital_mang.Data.Models.doctors_shifts;
using Employees = l_hospital_mang.Data.Models.Employees;
using invoice = l_hospital_mang.Data.Models.invoice;
using Medical_Health = l_hospital_mang.Data.Models.Medical_Health;
using Radiography = l_hospital_mang.Data.Models.Radiography;
//using Resident_patients = l_hospital_mang.Data.Models.Resident_patients;
//using Rooms = l_hospital_mang.Data.Models.Rooms;


namespace l_hospital_mang.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Medical_Health> Medical_Healths { get; set; }
        public DbSet<Models.Clinicscs> Clinicscss { get; set; }
        public DbSet<Models.patient> Patients { get; set; }
        public DbSet<Resident_patients> Resident_patientss { get; set; }
        public DbSet<Models.Dates> Datess { get; set; }
        public DbSet<Models.Consulting_reservation> Consulting_reservations { get; set; }
        public DbSet<Models.Doctors> Doctorss { get; set; }
        public DbSet<Models.Shifts> Shiftss { get; set; }
        public DbSet<Models.Requests> Requestss { get; set; }
        public DbSet<Models.CAmbulance_Car> CAmbulance_Carس { get; set; }
        public DbSet<Models.Employees> Employeess { get; set; }
        public DbSet<Models.Analysis> Analysiss { get; set; }
        public DbSet<Models.Radiography> adiographyies { get; set; }
        public DbSet<Models.Advertisments> Advertismentss { get; set; }
        public DbSet<Models.invoice> invoices { get; set; }
        public DbSet<Models.Type> Types { get; set; 
 }
        public DbSet<Models.surgery_reservations> surgery_reservationss {  get; set; }

        public DbSet<Rooms> Room { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // علاقة one to many بين المريض والحجوزات
            modelBuilder.Entity<Models.Consulting_reservation>()
                .HasOne(r => r.Patient)
                .WithMany(p => p.Consulting_reservations)
                .HasForeignKey(r => r.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

           
            // علاقة one to one بين المريض وطلب سيارة الإسعاف
            modelBuilder.Entity<Requests>()
                .HasOne(r => r.Patient)
                .WithOne(p => p.Requests)
                .HasForeignKey<Requests>(r => r.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            //     // علاقة بين التحاليل والحجوزات one to one
            modelBuilder.Entity<Consulting_reservation>()
               .HasOne(r => r.Analysis)
               .WithOne(a => a.Consulting_reservation)
               .HasForeignKey<Analysis>(a => a.Consulting_reservationId)
               .OnDelete(DeleteBehavior.Cascade);
            //     // one to one  بين المواعيد والحجوزات

            modelBuilder.Entity<Consulting_reservation>()
            .HasOne(r => r.Dates)
            .WithOne(d => d.Consulting_reservation)
            .HasForeignKey<Dates>(d => d.Consulting_reservationId)
            .OnDelete(DeleteBehavior.Cascade);



            //     //one to one  بين الحجوزات والتصوير الشعاعي

            modelBuilder.Entity<Consulting_reservation>()
            .HasOne(r => r.Radiography)
            .WithOne(rg => rg.Consulting_reservation)
            .HasForeignKey<Radiography>(rg => rg.Consulting_reservationId)
            .OnDelete(DeleteBehavior.Cascade);

            // one to one  بين الموظف والنوع
            modelBuilder.Entity<Employees>()
           .HasOne(e => e.Type)
           .WithOne(t => t.Employee)
           .HasForeignKey<Employees>(e => e.TypeId)
           .OnDelete(DeleteBehavior.Restrict);




            //     //one to many  بين الطبيب و المواعيد 

            modelBuilder.Entity<Dates>()
           .HasOne(d => d.Doctor)
           .WithMany(d => d.Dates)
           .HasForeignKey(d => d.DoctorId)
           .OnDelete(DeleteBehavior.Cascade);

            //     // one to many  بين العيادات والاطباء
            modelBuilder.Entity<Doctors>()
           .HasOne(d => d.Clinic)
           .WithMany(c => c.Doctors)
           .HasForeignKey(d => d.ClinicId)
           .OnDelete(DeleteBehavior.Cascade);



            // one to many  بين العيادات والاعلانات 
            modelBuilder.Entity<Advertisments>()
           .HasOne(a => a.Clinic)
           .WithOne(c => c.Advertisments)
           .HasForeignKey<Advertisments>(a => a.ClinicId)
           .OnDelete(DeleteBehavior.Cascade);


            // one to many  بين الاقامات والفاتورة
            modelBuilder.Entity<invoice>()
           .HasOne(i => i.ResidentPatient)
           .WithMany(r => r.Invoices)
           .HasForeignKey(i => i.ResidentPatientId)
           .OnDelete(DeleteBehavior.Cascade);


            //     //one to one  بين سيارة الساعاف والطلب
            modelBuilder.Entity<CAmbulance_Car>()
                 .HasOne(a => a.Request)
                 .WithOne(r => r.AmbulanceCar)
                 .HasForeignKey<Requests>(r => r.AmbulanceCarId);






            // many to many  بين الطبيب والمناوبات

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
            //     //one to one  بين الموظف و النوع 
            modelBuilder.Entity<Employees>()
           .HasOne(e => e.Type)
           .WithOne(t => t.Employee)
           .HasForeignKey<Employees>(e => e.TypeId);
            //one to one  بين الغرف والمقيمين
          modelBuilder.Entity<Resident_patients>()
    .HasOne(rp => rp.Room)
    .WithOne(r => r.Resident_patient)
    .HasForeignKey<Resident_patients>(rp => rp.RoomId)
    .OnDelete(DeleteBehavior.Restrict);

            //one to many  بين المريض والسجل الطبي
            modelBuilder.Entity<patient>()
       .HasMany(p => p.Medical_Healths)    
       .WithOne(mh => mh.Patient)          
       .HasForeignKey(mh => mh.PatientId) 
       .OnDelete(DeleteBehavior.Cascade);
            //one to many  بين  الحجز الجراحي والمريض
            modelBuilder.Entity<surgery_reservations>()
                .HasOne(sr => sr.Patient)
                .WithMany(p => p.SurgeryReservations)
                .HasForeignKey(sr => sr.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            //one to many  بين الحجز الجراحي والطبيب
            modelBuilder.Entity<surgery_reservations>()
                .HasOne(sr => sr.Doctor)
                .WithMany(d => d.SurgeryReservations)
                .HasForeignKey(sr => sr.DoctorId)
                .OnDelete(DeleteBehavior.Restrict); 



        }








    }
}
