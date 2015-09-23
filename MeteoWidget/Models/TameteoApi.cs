using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MeteoWidget.Models
{
    public class TameteoApi
    {
        //public int[] minTemp;
        //public int[] maxTemp;
        //public String minTempStr;

        public String dayTime;
        public String[] dayStart;
        public String[] weekDay;
        public String[] weekEnd;
        public String symbolNumber; //A number between 100 & 999 which represents the icon
        public String symbolName; //Description of the weather
        public String temp;
        public String rain;
        public String wind;
        public String windDirection;
        public String pressure;

        public TameteoApi()
        {
            //minTemp = new int[5];
            //maxTemp = new int[5];
            //minTempStr = "";
            dayStart = new String[7] { "", "", "", "", "", "", "" };
            weekDay = new String[7];
            dayTime = "";
            weekEnd = new String[2] { "", "" };
            symbolNumber = "";
            symbolName = "";
            temp = "";
            rain = "";
            wind = "";
            windDirection = "";
            pressure = "";
        }
    }
}