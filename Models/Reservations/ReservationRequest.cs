namespace FYP_Navperks.Models.Reservations
{
    public class ReservationRequest
    {
        public int UserId { get; set; }
        public int UserAId { get; set; }
        public string SlotCode { get; set; }
        public string CarType { get; set; }
    }
}
