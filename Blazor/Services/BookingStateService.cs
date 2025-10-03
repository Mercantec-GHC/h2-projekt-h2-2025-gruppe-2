namespace Blazor.Services
{
    public class BookingStateService
    {
        public DateOnly? SelectedStartDate { get; set; }
        public string PreSelectedRoomId { get; set; } = string.Empty;

        public void SetBookingData(DateOnly startDate, string roomId)
        {
            SelectedStartDate = startDate;
            PreSelectedRoomId = roomId;
        }

        public void Clear()
        {
            SelectedStartDate = null;
            PreSelectedRoomId = string.Empty;
        }
    }
}