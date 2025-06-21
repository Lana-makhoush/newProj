using System.Collections.Generic;
using System.Linq;
using l_hospital_mang.Data.Models;

namespace l_hospital_mang.Data
{
    public static class DbSeeder
    {
        public static void SeedAmbulanceCars(AppDbContext context)
        {
            if (!context.CAmbulance_Car.Any())
            {
                var cars = new List<CAmbulance_Car>
                {
                    new CAmbulance_Car { CarNumber = "AMB-001", IsAvailable = true },
                    new CAmbulance_Car { CarNumber = "AMB-002", IsAvailable = true },
                     new CAmbulance_Car { CarNumber = "AMB-003", IsAvailable = true },
                    new CAmbulance_Car { CarNumber = "AMB-004", IsAvailable = true }, new CAmbulance_Car { CarNumber = "AMB-009", IsAvailable = true },
                    new CAmbulance_Car { CarNumber = "AMB-005", IsAvailable = true }, new CAmbulance_Car { CarNumber = "AMB-010", IsAvailable = true },
                    new CAmbulance_Car { CarNumber = "AMB-006", IsAvailable = true }, new CAmbulance_Car { CarNumber = "AMB-011", IsAvailable = true },
                    new CAmbulance_Car { CarNumber = "AMB-007", IsAvailable = true },
                    new CAmbulance_Car { CarNumber = "AMB-008", IsAvailable = true }
                };

                context.CAmbulance_Car.AddRange(cars);
                context.SaveChanges();
            }
        }
    }
}
