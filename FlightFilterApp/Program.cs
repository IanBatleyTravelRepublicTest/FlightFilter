using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using FlightFilterApp.Builders;
using FlightFilterApp.Entities;
using FlightFilterApp.Extensions;

namespace FlightFilterApp
{
    class Program
    {
        private static FlightBuilder _flightBuilder;

        static void Main(string[] args)
        {
            //Flight builder internally stores state around dates - this means it should probably be treated as a singleton so that dates do not change if run over date barriers
            _flightBuilder = new FlightBuilder();

            var filters = GetFilters();

            GetFilteredFlights(filters);
            
        }

        private static List<Func<Flight, bool>> GetFilters()
        {
            DateTime nowTime = DateTime.Now;

            var filters = new List<Func<Flight, bool>>()
            {
                //1. Depart before the current date/time.
                flight => flight.Segments.Any(seg => seg.DepartureDate < nowTime), 
                //2. Have a segment with an arrival date before the departure date.
                flight => flight.Segments.Any(seg => seg.ArrivalDate < seg.DepartureDate), 
                //3. Spend more than 2 hours on the ground. i.e those with a total gap of over two hours between the arrival date of one segment and the departure date of the next.
                flight => flight.Segments.Any(seg => !CheckGroundStopTimeBetweenSegments(flight.Segments.OrderBy(s => s.DepartureDate).ToList(), seg)) 
            };

            return filters;
        }


        public static void GetFilteredFlights(List<Func<Flight, bool>> filters)
        {
            if (filters == null || !filters.Any()) return;

            var flights = _flightBuilder.GetFlights();            

            if(flights.Any(f => !f.Segments.Any())) throw new InvalidOperationException("Could not operate - flights found with no segments");

            var filteredFlights = flights.GetFilteredItems(filters);
            
            PersistResults(filteredFlights);
        }

        private static bool CheckGroundStopTimeBetweenSegments(List<Segment> orderedSegments , Segment currentSegment)
        {
            var currentIndex = orderedSegments.IndexOf(currentSegment);

            //take next
            var nextSegment = orderedSegments.Skip(currentIndex + 1).FirstOrDefault();

            //No next segment, assume true
            if (nextSegment == null) return true;

            TimeSpan timeSpentOnGround = nextSegment.DepartureDate - currentSegment.ArrivalDate;

            return timeSpentOnGround.TotalHours < 2;
        }

        private static void PersistResults(IEnumerable<Flight> filteredFlights)
        {
            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(filteredFlights);

            File.WriteAllText(@"..\..\result\result.json", json);

            Console.WriteLine(json);
        }
    }
}
