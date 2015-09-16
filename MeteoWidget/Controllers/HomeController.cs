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

namespace MeteoWidget.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        public static XDocument GetMeteoApi()
        {
            Uri uri = new Uri("http://api.tameteo.com/index.php?api_lang=fr&localidad=25339&affiliate_id=fu7tkep336jx");
            WebRequest request = WebRequest.Create(uri);
            //request.Method = "GET";
            //request.Method = "PUT";
            Stream stream = request.GetRequestStream();
            //var stream = response.GetResponseStream();
            XmlReader xmlReader = XmlReader.Create(stream);
            return XDocument.Load(xmlReader);
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
            //Test XML serialization
            //XDocument xDoc = GetMeteoApi();
            XmlDocument response = GetXmlResponse("http://api.tameteo.com/index.php?api_lang=fr&localidad=25339&affiliate_id=fu7tkep336jx");

            //Read the content of the API return XML
            String allValues = "";
            XmlNodeList dataElements = response.GetElementsByTagName("data");
            for (int i = 0; i <= dataElements.Count - 1; i++)
            {
                XmlNodeList forecastElements = dataElements[i].ChildNodes;
                for (int j = 0; j <= forecastElements.Count - 1; j++) {
                    allValues += forecastElements[j].Attributes[1].Value + "; ";
                }
            }

            TameteoApi tameteoApi = new TameteoApi();
            //tameteoApi.minTemp = new int[3];
            //tameteoApi.maxTemp = new int[3];
            tameteoApi.minTemp[0] = 1;
            tameteoApi.minTemp[1] = 2;
            tameteoApi.minTemp[2] = 3;
            tameteoApi.maxTemp[0] = 4;
            tameteoApi.maxTemp[1] = 5;
            tameteoApi.maxTemp[2] = 6;

            ViewBag.Title = "Home Page";

            return View(tameteoApi);
        }

        protected T FromXml<T>(String xml)
        {
            T returnedXmlClass = default(T);

            try
            {
                using (TextReader reader = new StringReader(xml))
                {
                    try
                    {
                        returnedXmlClass =
                            (T)new XmlSerializer(typeof(T)).Deserialize(reader);
                    }
                    catch (InvalidOperationException)
                    {
                        // String passed is not XML, simply return defaultXmlClass
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return returnedXmlClass;
        }

    }
}
