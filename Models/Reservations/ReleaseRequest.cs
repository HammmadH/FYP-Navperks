namespace FYP_Navperks.Models.Reservations
{
    public class ReleaseRequest
    {
        public int UserId { get; set; }
        public int UserAId { get; set; }
        public string SlotCode { get; set; }
    }
}
