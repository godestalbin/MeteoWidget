using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Net;
using MeteoWidget.Models;
using System.Globalization;

namespace MeteoWidget.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        //https://msdn.microsoft.com/en-us/library/hh304484.aspx
        public static XmlDocument GetXmlResponse(string requestUrl)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(requestUrl) as HttpWebRequest;
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(response.GetResponseStream());
                return (xmlDoc);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                Console.Read();
                return null;
            }
        }

        public ActionResult Meteo()
        {
            //Query Open Weather api with city code for Wattignies
            XmlDocument response = GetXmlResponse("http://api.openweathermap.org/data/2.5/forecast?id=6454427&mode=xml");

            TameteoApi tameteoApi = new TameteoApi();
            XmlNodeList dataElements = response.GetElementsByTagName("time");
            DateTime previousDate = DateTime.Parse("1900-01-01", new CultureInfo("en-US"));
            System.Text.StringBuilder dayTime = new System.Text.StringBuilder();
            int dayCounter = 0;
            for (int i = 0; i <= dataElements.Count - 1; i++)
            {
                //dataElements[i].Attributes[0].Value
                //Convert the datetime string to Datetime
                DateTime convertedDate = DateTime.Parse(dataElements[i].Attributes[0].Value);
                String weekDay = convertedDate.ToString("dddd", new System.Globalization.CultureInfo("fr-FR"));
                //Initialize the first day
                if (i == 0)
                {
                    //tameteoApi.weekDay[dayCounter] = weekDay.First().ToString().ToUpper() + weekDay.Substring(1, weekDay.Length - 1);
                }
                //Set begining of weekend if not set yet
                if (weekDay == "samedi" && tameteoApi.weekEnd[0] == "")
                    tameteoApi.weekEnd[0] = (i - 0.5).ToString(new System.Globalization.CultureInfo("en-US"));
                //Set end of weekend if not set yet
                if (weekDay == "lundi" && tameteoApi.weekEnd[1] == "")
                    tameteoApi.weekEnd[1] = (i - 0.5).ToString(new System.Globalization.CultureInfo("en-US"));

                dayTime.Append("'");
                //We moved to a new day
                if (previousDate.Day != convertedDate.Day || previousDate.Month != convertedDate.Month || previousDate.Year != convertedDate.Year) {
                    tameteoApi.weekDay[dayCounter + 1] = weekDay.First().ToString().ToUpper() + weekDay.Substring(1, weekDay.Length - 1);
                    weekDay = weekDay.First().ToString().ToUpper() + weekDay.Substring(1, 1);
                    dayTime.Append(weekDay + " ");
                    //Register day to show a separation (plotLines)
                    tameteoApi.dayStart[dayCounter] = (i - 0.5m).ToString(new System.Globalization.CultureInfo("en-US"));
                    dayCounter += 1;
                    //Register the weekend to be able to display them with grey (plotBands)
                    //if (weekDay == "Sa")
                    //    tameteoApi.weekEnd = (i-0.5).ToString(new System.Globalization.CultureInfo("en-US"));
                    //if (weekDay == "Di" && tameteoApi.weekEnd == "")
                    //    tameteoApi.weekEnd = (i - 10.5).ToString(new System.Globalization.CultureInfo("en-US"));
                }
                //Add time and minutes with leading zeroes
                dayTime.Append(convertedDate.Hour + ":" + convertedDate.Minute.ToString("D2") + "', ");
                previousDate = convertedDate;
                XmlNodeList forecastElements = dataElements[i].ChildNodes;

                //Symbol used to display weather icon
                if ((convertedDate.Hour - 3) % 12 == 0)
                {
                    String weatherIcon = getWeatherIcon(Convert.ToInt32(forecastElements[0].Attributes[0].Value));
                    tameteoApi.symbolName += "{y:32, weather:'" + forecastElements[0].Attributes[0].Value + "=" + forecastElements[0].Attributes[1].Value + "', marker:{symbol:'url(/Content/Images/" + weatherIcon + ")'}}, ";
                }
                else
                    tameteoApi.symbolName += "{y:32, marker: { enabled: false}}, ";

                //Rain
                if (forecastElements[1].Attributes.Count > 0)
                    tameteoApi.rain = tameteoApi.rain + forecastElements[1].Attributes[1].Value + ", ";
                else
                    tameteoApi.rain = tameteoApi.rain + "0, ";
                //Pressure???
                Decimal pressure = System.Convert.ToDecimal(forecastElements[3].Attributes[0].Value, new CultureInfo("en-US")) * 3.6m;
                //Wind direction
                String windDirection = getWindDirection(forecastElements[2].Attributes[1].Value);
                tameteoApi.windDirection += "{y:5, windDirection:'" + forecastElements[2].Attributes[1].Value + "', marker:{symbol:'url(/Content/Images/" + windDirection + ")'}}, ";
                //Wind
                tameteoApi.wind = tameteoApi.wind + pressure.ToString(new CultureInfo("en-US")) + ", ";
                //Temp is in Kelvin we need to convert by substracting -272.15 - Now temp is in Celsius ???
                //We also round to 2 decimal
                Decimal temp = Math.Round(System.Convert.ToDecimal(forecastElements[4].Attributes[1].Value, new CultureInfo("en-US")) - 0.0m, 0);
                tameteoApi.temp = tameteoApi.temp + "{y:" + temp.ToString(new CultureInfo("en-US")) + ", symbolName:'" + forecastElements[0].Attributes[1].Value + "'}, ";
                //Pressure
                tameteoApi.pressure = tameteoApi.pressure + forecastElements[5].Attributes[1].Value + ", ";
                //        //0 = < symbol number = "500" name = "light rain" var = "10d" />
                //        //1 = < precipitation unit = "3h" value = "2.96" type = "rain" />
                //        //2 = < windDirection deg = "216" code = "SW" name = "Southwest" />
                //        //3 = < windSpeed mps = "8.72" name = "Fresh Breeze" />
                //        //4 = < temperature unit = "celsius" value = "292.03" min = "290.833" max = "292.03" />
                //        //5 = < pressure unit = "hPa" value = "995.31" />
                //        //6 = < humidity value = "89" unit = "%" />
                //        //7 = < clouds value = "scattered clouds" all = "44" unit = "%" />
            }
            //Register last day to show a separation (plotLines)
            if (tameteoApi.dayStart[dayCounter] == "")
                tameteoApi.dayStart[dayCounter] = (39.5m).ToString(new System.Globalization.CultureInfo("en-US"));
            tameteoApi.dayStart[6] = (39.5).ToString(new System.Globalization.CultureInfo("en-US"));
            //Remove last semicolon+space
            tameteoApi.symbolName = tameteoApi.symbolName.Remove(tameteoApi.symbolName.Length - 2);
            tameteoApi.rain = tameteoApi.rain.Remove(tameteoApi.rain.Length - 2);
            tameteoApi.wind = tameteoApi.wind.Remove(tameteoApi.wind.Length - 2);
            tameteoApi.temp = tameteoApi.temp.Remove(tameteoApi.temp.Length - 2);
            tameteoApi.pressure = tameteoApi.pressure.Remove(tameteoApi.pressure.Length - 2);

            tameteoApi.dayTime = dayTime.ToString();
            tameteoApi.dayTime = tameteoApi.dayTime.Remove(tameteoApi.dayTime.Length - 2);
            //In case there is no weekend to display
            if (tameteoApi.weekEnd[0] == "")
                if (tameteoApi.weekEnd[1] == "")
                    tameteoApi.weekEnd[0] = "50"; //Avoid to display weekend in visible part of the graph
                else
                    tameteoApi.weekEnd[0] = "-20";
            if (tameteoApi.weekEnd[1] == "")
                tameteoApi.weekEnd[1] = "50"; //Avoid to display weekend in visible part of the graph
            ViewBag.Title = "Home Page";

            return View(tameteoApi);
        }

        //Determine the weather icon to use base on the symbol number
        //In this document we defined what we assigned: https://drive.google.com/open?id=1qXgqMQTOSEgDi_FNXmWR_T3BXVBw27rjtBbjI2rW1H0
        //Currently we use basic settings
        private String getWeatherIcon(int symbolNumber)
        {
            //String weatherIcon = "";
            //if (symbolNumber <= 500)
            //    return "1.png"; //Sun
            //else return "10.png"; //Rain
            //return "~/Content/Images/" + weatherIcon;

            switch (symbolNumber)
            {
                case 800:
                    return "1.png"; //Sun
                case 801:
                    return "2.png";
                case 802:
                case 803:
                    return "3.png";
                case 804:
                    return "4.png";
                case 300:
                case 310:
                case 500:
                    return "5.png";
                case 301:
                case 311:
                case 321:
                case 501:
                    return "6.png";
                case 302:
                case 312:
                case 502:
                    return "7.png";
                case 520:
                    return "8.png";
                case 521:
                    return "9.png";
                case 503:
                case 504:
                case 522:
                    return "10.png";
                case 200:
                case 210:
                case 230:
                    return "11.png";
                case 201:
                case 211:
                case 221:
                case 231:
                    return "12.png";
                case 202:
                case 212:
                case 232:
                    return "13.png";
                case 511:
                    return "16.png";
                default:
                    return "602.png";
            }
        }

        //Convert the string cardinal direction into as wind direction icon
        private String getWindDirection(String windDirection)
        {
            if (windDirection.Length == 3)
                windDirection = windDirection.Substring(1, 2);
            return windDirection + ".png";
        }

    }
}
