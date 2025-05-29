namespace FYP_Navperks.Models.Reservations
{
    public class ReservationDetails
    {
        public int ReservationId { get; set; }
        public string SlotCode { get; set; }
        public string CarType { get; set; }
        public float? CarSpeed { get; set; }
        public DateTime ReservedTime { get; set; }
        public DateTime? ReleasedTime { get; set; }
    }
}
