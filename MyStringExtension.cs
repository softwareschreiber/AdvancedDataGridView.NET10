using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Zuby
{
    public static class MyStringExtension
    {
        public static int IndexOfLower(this string str, string search)
        {
            if (!String.IsNullOrEmpty(str) && !String.IsNullOrEmpty(search))
            {
                return str.ToLower().IndexOf(search.ToLower());
            }
            return -1;
        }
    }
}
