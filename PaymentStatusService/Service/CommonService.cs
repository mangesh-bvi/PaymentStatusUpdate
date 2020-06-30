using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace PaymentStatusService.Service
{
    public class CommonService
    {
        /// <summary>
        ///SEND api request
        /// </summary>
        /// 
        public static string SendApiRequest(string url, string Request,string token)
        {
            string strresponse = "";
            try
            {
                var httpWebRequest = (System.Net.HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Headers.Add("Authorization", "Bearer " + token);

                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    if (!string.IsNullOrEmpty(Request))
                        streamWriter.Write(Request);
                }
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    strresponse = streamReader.ReadToEnd();
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;

                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        strresponse = reader.ReadToEnd();

                    }
                }
            }
            catch (Exception ex) 
            {
               
            }

            return strresponse;

        }


        public static string SendApiRequestToken(string url, string Request, string token = "")
        {
            string strresponse = "";
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
                httpWebRequest.Method = "POST";
                //httpWebRequest.Accept = "text/plain";

                httpWebRequest.ContentType = "application/x-www-form-urlencoded";


                ASCIIEncoding encoding = new ASCIIEncoding();
                byte[] byte1 = encoding.GetBytes(Request);

                //using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                //{
                // if (!string.IsNullOrEmpty(Request))
                // streamWriter.Write(Request);
                //}
                Stream newStream = httpWebRequest.GetRequestStream();

                newStream.Write(byte1, 0, byte1.Length);
                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    strresponse = streamReader.ReadToEnd();
                }

            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;

                    using (Stream data = response.GetResponseStream())
                    using (var reader = new StreamReader(data))
                    {
                        strresponse = reader.ReadToEnd();

                    }
                }
            }
            catch (Exception)
            {
                throw;
            }

            return strresponse;

        }

    }
}
