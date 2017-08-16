using System;

namespace FlightFilterApp.Entities
{
    public class Segment
    {
        public string ReadableDepartureDate {  get { return DepartureDate.ToString(); } }
        public string RedableArrivalDate { get { return ArrivalDate.ToString(); } }

        public DateTime DepartureDate { get; set; }
        public DateTime ArrivalDate { get; set; }
    }
}