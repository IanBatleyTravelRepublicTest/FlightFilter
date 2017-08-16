using System.Collections.Generic;

namespace FlightFilterApp.Entities
{
    public class Flight
    {
        public string Name { get; set; }
        public IList<Segment> Segments { get; set; }
    }
}