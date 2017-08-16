using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using FlightFilterApp.Builders;
using FlightFilterApp.Entities;

namespace FlightFilterApp
{
    class Program
    {
        private static FlightBuilder _flightBuilder;

        static void Main(string[] args)
        {
            //Flight builder internally stores state around dates - this means it should probably be treated as a singleton so that dates do not change if run over date barriers
            _flightBuilder = new FlightBuilder();

            GetFilteredFlights();
            
        }

        public static void GetFilteredFlights()
        {
            var flights = _flightBuilder.GetFlights();

            var nowTime = DateTime.Now;

            if(flights.Any(f => !f.Segments.Any())) throw new InvalidOperationException("Could not operate - flights found with no segments");

            /*
            1. Depart before the current date/time.
            2. Have a segment with an arrival date before the departure date.
            3. Spend more than 2 hours on the ground. i.e those with a total gap of over two hours between the arrival
                date of one segment and the departure date of the next. */


            var filteredFlights =
                flights.Where(flight =>
                    {
                        var orderedSegments = flight.Segments.OrderBy(s => s.DepartureDate).ToList();

                        return flight.Segments.All(fs => fs.DepartureDate > nowTime) &&
                               flight.Segments.Any(fs => fs.ArrivalDate > fs.DepartureDate) &&
                               flight.Segments.All(seg => CheckGroundStopTimeBetweenSegments(orderedSegments, seg));
                    }).ToList();

            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(filteredFlights);

            Console.WriteLine(json);

            File.WriteAllText(@"..\..\result\result.json", json);

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
    }
}
