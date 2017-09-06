using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.IO;

using System.Xml;

namespace ega.Consoles.Test
{
    class Program : ICertificatePolicy
    {

        public bool CheckValidationResult(ServicePoint srvPoint,
        X509Certificate certificate, WebRequest request,
        int certificateProblem)
        {
            //Return True to force the certificate to be accepted.
            return true;
        }

        static void SendNotification(string argMsg)
        {
            var request = (HttpWebRequest)WebRequest.Create("https://notify-api.line.me/api/notify");
            var postData = string.Format("message={0}", argMsg);
            var data = Encoding.UTF8.GetBytes(postData);

            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = data.Length;
            request.Headers.Add("Authorization", "Bearer TRp6byyCsJG7S2poh5ON3zdH88SSm3LMffZ1fXy8o1H"); //KlPhgOKMqBYSuLsZBLAY7uUCXD1s0jEjwHfbUPbQE0I

            using (var stream = request.GetRequestStream())
            {
                stream.Write(data, 0, data.Length);
            }

            var response = (HttpWebResponse)request.GetResponse();
            var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            Console.WriteLine("\r\n{0} \r\n",responseString);
        }
        static void Main(string[] args)
        {
            string stoken = string.Empty;
            System.Net.ServicePointManager.CertificatePolicy = new Program();
            StringBuilder xml = new StringBuilder();
            xml.Append("<?xml version=\"1.0\" encoding=\"utf-8\" ?>");
            xml.Append("<soap:Envelope xmlns:soap=\"http://www.w3.org/2003/05/soap-envelope\">");
            xml.Append("<soap:Header>");
            xml.Append("<context xmlns=\"urn:zimbra\">");
            xml.Append("<format type=\"xml\"/>");
            xml.Append("</context>");
            xml.Append("</soap:Header>");
            xml.Append("<soap:Body>");
            xml.Append("<AuthRequest xmlns=\"urn:zimbraAccount\">");
            xml.Append("<account by=\"name\">mailchecker@training.mail.go.th</account>");    //@@ thaiairways_admin@api.mail.go.th
            xml.Append("<password>Ega@2017</password>");                                      //@@ QR1kcD3
            xml.Append("</AuthRequest>");
            xml.Append("</soap:Body>");
            xml.Append("</soap:Envelope>");
            Console.WriteLine(xml);
            HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create("https://accounts.mail.go.th/service/soap");
            httpRequest.ContentType = "application/soapxml";

            byte[] byteArray = Encoding.UTF8.GetBytes(xml.ToString());

            httpRequest.Method = "POST";
            httpRequest.ContentLength = byteArray.Length;
            try
            {
                using (var stream = httpRequest.GetRequestStream())
                {
                    stream.Write(byteArray, 0, byteArray.Length);
                }
                var response = (HttpWebResponse)httpRequest.GetResponse();

                var responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();
                Console.WriteLine(responseString);

                //Console.ReadLine();

                XmlDocument responseDoc = new XmlDocument();
                responseDoc.LoadXml(responseString);
                string authtoken = responseDoc.GetElementsByTagName("authToken").Item(0).InnerXml;
                stoken = authtoken;
                Console.WriteLine("\r\nauthToken:{0}\r\n", stoken);
                // retPreAuthTokenValue = authtoken;
                if (!string.IsNullOrEmpty(stoken))
                {
                    Console.WriteLine("not need to send notification..");
                    SendNotification("[MailGoThai]: สามารถเชื่อมต่อ MailGoThai ได้ (เทสโดย : mailchecker@training.mail.go.th)");
                }
                else
                {
                    SendNotification("[MailGoThai]: ไม่สามารถเก็ทค่า token ได้");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error: {0}",ex.Message);
                Console.WriteLine("send notification to LINE..");
                SendNotification("[MailGoThai]: ไม่สามารถเชื่อมต่อ MailGoThai ได้");
            }
        }
    }
}
