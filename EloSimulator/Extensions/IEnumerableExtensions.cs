using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EloSimulator
{
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Perform Fisher-Yates shuffle on a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static IEnumerable<T> RandomOrdering<T>( this IEnumerable<T> list )
        {
            List<T> array = new List<T>( list );
            //I don't like doing this, but there isn't a better way that I see for an extension method
            Random r = new Random();

            for ( int i = array.Count; i > 1; i-- )
            {
                int j = r.Next( i );
                T tmp = array[j];
                array[j] = array[i - 1];
                array[i - 1] = tmp;
            }

            return array;
        }

        /// <summary>
        /// Perform Fisher-Yates shuffle on a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static IEnumerable<T> RandomOrdering<T>( this IEnumerable<T> list, Random r )
        {
            List<T> array = new List<T>( list );

            for ( int i = array.Count; i > 1; i-- )
            {
                int j = r.Next( i );
                T tmp = array[j];
                array[j] = array[i - 1];
                array[i - 1] = tmp;
            }

            return array;
        }

        /// <summary>
        /// Perform Fisher-Yates shuffle on numSubset random elements of a list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="r"></param>
        /// <param name="numSubset"></param>
        /// <returns></returns>
        public static IEnumerable<T> RandomOrdering<T>(this IEnumerable<T> list, Random r, int numSubset )
        {
            List<T> array = new List<T>();
            array.AddRange( list );

            int limit = (numSubset < array.Count) ? array.Count - numSubset : 1;

            for ( int i = array.Count; i > limit; i-- )
            {
                int j = r.Next( i );
                T tmp = array[j];
                array[j] = array[i - 1];
                array[i - 1] = tmp;
            }

            if ( numSubset < array.Count )
                return array.GetRange( limit, numSubset );

            return array;
        }
    }
}
