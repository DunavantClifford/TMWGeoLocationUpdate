using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoogleMapsApi;
using GoogleMapsApi.Entities.Common;
using GoogleMapsApi.Entities.Directions.Request;
using GoogleMapsApi.Entities.Directions.Response;
using GoogleMapsApi.Entities.Geocoding.Request;
using GoogleMapsApi.Entities.Geocoding.Response;
using GoogleMapsApi.StaticMaps;
using GoogleMapsApi.StaticMaps.Entities;
using GenericParsing;
using System.Data;
using System.IO;


namespace CSharpGeoUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
            GenericParserAdapter gpa = new GenericParserAdapter("C:\\project\\DTG\\TMS Reviews\\TMW\\ClientsMissingGC.csv");
            //GenericParser gp = new GenericParser("C:\\project\\DTG\\TMS Reviews\\TMW\\NewClients.csv");
            gpa.FirstRowHasHeader = true;
            DataSet dsoriginal = new DataSet();
            StreamReader sr;
            StreamWriter sw;
            string prefix = "UPDATE CLIENT SET POSLAT = '";
            StringBuilder sb = new StringBuilder();
            string strLAT = "";
            string strLONG = "";
            if (System.IO.File.Exists("C:\\project\\DTG\\TMS Reviews\\TMW\\ClientsGEOUpdateScript2.sql"))
            {
                sr = new StreamReader("C:\\project\\DTG\\TMS Reviews\\TMW\\ClientsGEOUpdateScript2.sql");
                sb.Append(sr.ReadToEnd().ToString());
                sr.Close();
            }
            if (System.IO.File.Exists("C:\\project\\DTG\\TMS Reviews\\TMW\\WorkingClients2.xml"))
            {
                dsoriginal.ReadXml("C:\\project\\DTG\\TMS Reviews\\TMW\\WorkingClients2.xml");
            }
            else
            {
                dsoriginal = gpa.GetDataSet();
                dsoriginal.WriteXml("C:\\project\\DTG\\TMS Reviews\\TMW\\WorkingClients2.xml");
            }
            //DataSet ds = new DataSet();
            //ds.ReadXml("C:\\project\\DTG\\TMS Reviews\\TMW\\NewClientsShortLat2.xml");
            DataRow[] arRows = dsoriginal.Tables[0].Select("POSLAT = '' OR POSLONG = ''");
            

            
            GeocodingResponse geocode;
            GeocodingRequest geocodeRequest = new GeocodingRequest();
            double degrees;
            double minutes;
            double seconds;
            string strDeg;
            string strMin;
            string strSec;
            double lat;
            double lng;

            var geocodingEngine = GoogleMaps.Geocode;
            
            foreach (DataRow dr in arRows)
            {
                /*
                strLAT = dr["POSLAT"].ToString();
                strLONG = dr["POSLONG"].ToString();
                if (strLAT.Substring(0,1) == "N")
                {
                    strLAT = strLAT.Substring(1, strLAT.Length - 1);
                    if (strLAT.LastIndexOf("."))
                }
                */
                
                geocodeRequest.Address = dr["ADDRESS_1"] + ", " + dr["CITY"] + ", " + dr["PROVINCE"] + " " + dr["POSTAL_CODE"];
                geocode = geocodingEngine.Query(geocodeRequest);
                if (geocode.Results.Count() > 0)
                {
                    lat = geocode.Results.First().Geometry.Location.Latitude;
                    degrees = Math.Floor(lat);
                    strDeg = "00" + degrees.ToString();
                    minutes = Math.Floor((lat - degrees) * 60);
                    strMin = "0" + minutes.ToString();
                    seconds = Math.Round((((lat - degrees) * 60) - minutes) * 60, 0);
                    strSec = "0" + seconds.ToString();
                    //dr["POSLAT"] = "N" + strDeg.Substring(Math.Max(0, strDeg.Length - 2)) + strMin.Substring(Math.Max(0, strMin.Length - 2)) + strSec.Substring(Math.Max(0, strSec.Length - 2));
                    strLAT = strDeg.Substring(strDeg.Length - 3, 3) + strMin.Substring(Math.Max(0, strMin.Length - 2),2) + strSec.Substring(Math.Max(0, strSec.Length - 2),2) + "N";
                    dr["POSLAT"] = strLAT;
                    lng = geocode.Results.First().Geometry.Location.Longitude;
                    degrees = Math.Floor(Math.Abs(lng));
                    strDeg = "00" + degrees.ToString();
                    minutes = Math.Floor((Math.Abs(lng) - degrees) * 60);
                    strMin = "0" + minutes.ToString();
                    seconds = Math.Round((((Math.Abs(lng) - degrees) * 60) - minutes) * 60, 0);
                    strSec = "0" + seconds.ToString();
                    strLONG = strDeg.Substring(Math.Max(0, strDeg.Length - 3),3) + strMin.Substring(Math.Max(0, strMin.Length - 2),2) + strSec.Substring(Math.Max(0, strSec.Length - 2),2) + "W";
                    dr["POSLONG"] = strLONG;
                    sb.AppendLine(prefix + strLAT + "',POSLONG='" + strLONG + "' WHERE CLIENT_ID='" + dr["CLIENT_ID"].ToString() + "'@");

                }
            }
            
            dsoriginal.WriteXml("C:\\project\\DTG\\TMS Reviews\\TMW\\WorkingClients2.xml");
            File.WriteAllText(@"C:\project\DTG\TMS Reviews\TMW\ClientsGEOUpdateScript2.sql",sb.ToString());
        }
    }
}
