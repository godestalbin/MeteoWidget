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

        public String dayTime;
        public String[] dayStart;
        public String weekEnd;
        public String temp;
        public String rain;
        public String wind;
        public String pressure;

        public TameteoApi()
        {
            minTemp = new int[5];
            maxTemp = new int[5];
            dayStart = new String[7];
            minTempStr = "";
            dayTime = "";
            weekEnd = "";
            temp = "";
            rain = "";
            wind = "";
            pressure = "";
        }
    }
}