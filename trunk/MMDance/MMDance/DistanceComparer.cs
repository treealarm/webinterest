using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MMDance
{
    class DistanceComparer : IComparer<KeyValuePair<int, int>>
    {
        public int Compare(KeyValuePair<int, int> p1, KeyValuePair<int, int> p2)
        {
            // Compare y and x in reverse order. 
            return 0;
        }
    }
}
