using System.Collections.Generic;

namespace FlightFilterApp.Entities
{
    public class Flight
    {
        public IList<Segment> Segments { get; set; }
    }
}