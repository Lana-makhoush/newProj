namespace l_hospital_mang.DTOs
{
    public class RoomDto
    {
        public int Id { get; set; }
        public int? RoomNumber { get; set; }
        public int? FloorNumber { get; set; }
        public int? bedsNumber { get; set; }
        public decimal ?Price { get; set; }
    }

}
