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
                if (weekDay == "Sa" && tameteoApi.weekEnd[0] == "")
                    tameteoApi.weekEnd[0] = (i - 0.5).ToString(new System.Globalization.CultureInfo("en-US"));
                //Set end of weekend if not set yet
                if (weekDay == "Lu" && tameteoApi.weekEnd[1] == "")
                    tameteoApi.weekEnd[1] = (i + 0.5).ToString(new System.Globalization.CultureInfo("en-US"));

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
                //for (int j = 0; j <= forecastElements.Count - 1; j++)
                //{
                //allValues += forecastElements[j].Attributes[1].Value + ", ";
                if (forecastElements[1].Attributes.Count > 0)
                    tameteoApi.rain = tameteoApi.rain + forecastElements[1].Attributes[1].Value + ", ";
                else
                    tameteoApi.rain = tameteoApi.rain + "0, ";
                Decimal pressure = System.Convert.ToDecimal(forecastElements[3].Attributes[0].Value, new CultureInfo("en-US")) * 3.6m;
                tameteoApi.wind = tameteoApi.wind + pressure.ToString(new CultureInfo("en-US")) + ", ";
                //Temp is in Kelvin we need to convert by substracting -272.15
                //We also round to 2 decimal
                Decimal temp = Math.Round(System.Convert.ToDecimal(forecastElements[4].Attributes[1].Value, new CultureInfo("en-US")) - 272.15m, 0);
                tameteoApi.temp = tameteoApi.temp + temp.ToString(new CultureInfo("en-US")) + ", ";
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
            tameteoApi.rain = tameteoApi.rain.Remove(tameteoApi.rain.Length - 2);
            tameteoApi.wind = tameteoApi.wind.Remove(tameteoApi.wind.Length - 2);
            tameteoApi.temp = tameteoApi.temp.Remove(tameteoApi.temp.Length - 2);
            tameteoApi.pressure = tameteoApi.pressure.Remove(tameteoApi.pressure.Length - 2);

            tameteoApi.dayTime = dayTime.ToString();
            tameteoApi.dayTime = tameteoApi.dayTime.Remove(tameteoApi.dayTime.Length - 2);
            //In case there is no weekend to display
            if (tameteoApi.weekEnd[0] == "")
                tameteoApi.weekEnd[0] = "-20"; //Avoid to display weekend in visible part of the graph
            if (tameteoApi.weekEnd[1] == "")
                tameteoApi.weekEnd[1] = "-20"; //Avoid to display weekend in visible part of the graph
            ViewBag.Title = "Home Page";

            return View(tameteoApi);
        }

        //public ActionResult Meteov0()
        //{
        //    //Test XML serialization
        //    //XDocument xDoc = GetMeteoApi();
        //    XmlDocument response = GetXmlResponse("http://api.tameteo.com/index.php?api_lang=fr&localidad=25339&affiliate_id=fu7tkep336jx");

        //    //Read the content of the API return XML
        //    String allValues = "";
        //    TameteoApi tameteoApi = new TameteoApi();
        //    XmlNodeList dataElements = response.GetElementsByTagName("data");
        //    for (int i = 0; i <= dataElements.Count - 1; i++)
        //    {
        //        XmlNodeList forecastElements = dataElements[i].ChildNodes;
        //        for (int j = 0; j <= forecastElements.Count - 1; j++) {
        //            allValues += forecastElements[j].Attributes[1].Value + ", ";
        //        }
        //        //Remove last semicolon+space
        //        allValues = allValues.Remove(allValues.Length - 2);
        //        switch (i)
        //        {
        //            case 0:
        //                //Min temp
        //                tameteoApi.minTempStr = allValues;
        //                break;
        //            case 1:
        //                //Max temp
        //                break;
        //            default:
        //                Console.WriteLine("Default case");
        //                break;
        //        }
        //        allValues = "";

        //    }

        //    //tameteoApi.minTemp = new int[3];
        //    //tameteoApi.maxTemp = new int[3];
        //    tameteoApi.minTemp[0] = 1;
        //    tameteoApi.minTemp[1] = 2;
        //    tameteoApi.minTemp[2] = 3;
        //    tameteoApi.maxTemp[0] = 4;
        //    tameteoApi.maxTemp[1] = 5;
        //    tameteoApi.maxTemp[2] = 6;

        //    ViewBag.Title = "Home Page";

        //    return View(tameteoApi);
        //}


    }
}
