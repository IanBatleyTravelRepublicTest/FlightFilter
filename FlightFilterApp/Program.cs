using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;
using FlightFilterApp.Builders;
using FlightFilterApp.Entities;
using FlightFilterApp.Extensions;

namespace FlightFilterApp
{
    class Program
    {
        //Flight builder internally stores state around dates - this means it should probably be treated as a singleton so that dates do not change if run over date barriers
        private static readonly FlightBuilder FlightBuilder = new FlightBuilder();


        static void Main(string[] args)
        {
            //Get the list of filters - prehaps one day this could come from a non-hardcoded source or be injected in somehow?
            var filters = GetFilters();
            var flights = FlightBuilder.GetFlights();

            //Run filters against list
            var filteredFlights = GetFilteredFlights(filters, flights);

            //Persist the flights for evaluation / review
            PersistResults(filteredFlights);
        }

        /// <summary>
        /// function to fetch the filters
        /// </summary>
        /// <returns>list of tests to run when filtering</returns>
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


        /// <summary>
        /// Filters flights using passed in filters
        /// </summary>
        /// <param name="filters"></param>
        /// <param name="flights"></param>
        public static IEnumerable<Flight> GetFilteredFlights(IList<Func<Flight, bool>> filters, IList<Flight> flights)
        {
            if (filters == null || flights == null) throw new InvalidOperationException("Invalid or null arguements passed in to GetFilteredFlights");
            if (!flights.Any()) return new List<Flight>();
            if (!filters.Any()) return flights;
            if (flights.Any(f => !f.Segments.Any())) throw new InvalidOperationException("Could not operate - flights found with no segments");

            return flights.GetFilteredItems(filters);
        }


        /// <summary>
        /// Checks to ensure ground stop time is satisfactory
        /// </summary>
        /// <param name="orderedSegments"></param>
        /// <param name="currentSegment"></param>
        /// <returns>If ground stop time is satisfactory</returns>
        private static bool CheckGroundStopTimeBetweenSegments(List<Segment> orderedSegments, Segment currentSegment)
        {
            var currentIndex = orderedSegments.IndexOf(currentSegment);

            //take next
            var nextSegment = orderedSegments.Skip(currentIndex + 1).FirstOrDefault();

            //No next segment, assume true
            if (nextSegment == null) return true;

            TimeSpan timeSpentOnGround = nextSegment.DepartureDate - currentSegment.ArrivalDate;

            return timeSpentOnGround.TotalHours < 2;
        }

        /// <summary>
        /// persists results for evaluation
        /// </summary>
        /// <param name="filteredFlights"></param>
        private static void PersistResults(IEnumerable<Flight> filteredFlights)
        {
            var jsonSerialiser = new JavaScriptSerializer();
            var json = jsonSerialiser.Serialize(filteredFlights);

            File.WriteAllText(@"..\..\result\result.json", json);

            Console.WriteLine(json);
        }
    }
}
