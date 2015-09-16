using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeteoWidget.Models
{
    public class TameteoApi
    {
        public int[] minTemp;
        public int[] maxTemp;
        public String minTempStr;

        public TameteoApi()
        {
            minTemp = new int[5];
            maxTemp = new int[5];
            minTempStr = "";
        }
    }
}