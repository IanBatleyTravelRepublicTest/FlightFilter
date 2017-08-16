using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightFilterApp.Extensions
{
    public static class Extensions
    {
        public static IEnumerable<T> GetFilteredItems<T>(this IEnumerable<T> listToFilter, IEnumerable<Func<T, bool>> filters)
        {
            //This is a bit cryptic - but, basically, return anything from the list that matchs all of the filters. 
            return listToFilter.Where(item => !filters.Any(filterTest => filterTest(item)));
        }
    }
}
