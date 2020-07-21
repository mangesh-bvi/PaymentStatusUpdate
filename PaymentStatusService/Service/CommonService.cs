﻿using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PaymentStatusService.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace PaymentStatusService.Service
{
    public class CommonService
    {
        static MySqlConnection conn = new MySqlConnection();
        static string apiResponse = string.Empty;

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


        /// <summary>
        /// SmsWhatsUpDataSend
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="userId"></param>
        /// <param name="ProgramCode"></param>
        /// <param name="orderId"></param>
        /// <param name="ClientAPIURL"></param>
        /// <param name="sMSWhtappTemplate"></param>
        /// <param name="ConString"></param>
        /// <returns></returns>
        public static int SmsWhatsUpDataSend(int tenantId, int userId, string ProgramCode, int orderId, string ClientAPIURL, string sMSWhtappTemplate, string ConString, string token)
        {
            int result = 0;
            string Message = "";
            DataSet ds = new DataSet();
            OrdersSmsWhatsUpDataDetails ordersSmsWhatsUpDataDetails = new OrdersSmsWhatsUpDataDetails();
            conn = new MySqlConnection(ConString);
            try
            {

                GetWhatsappMessageDetailsResponse getWhatsappMessageDetailsResponse = new GetWhatsappMessageDetailsResponse();
                List<GetWhatsappMessageDetailsResponse> getWhatsappMessageDetailsResponseList = new List<GetWhatsappMessageDetailsResponse>();

                string whatsapptemplate = GetWhatsupTemplateName(tenantId, userId, sMSWhtappTemplate);

                string strpostionNumber = "";
                string strpostionName = "";
                string additionalInfo = "";
                try
                {
                    GetWhatsappMessageDetailsModal getWhatsappMessageDetailsModal = new GetWhatsappMessageDetailsModal()
                    {
                        ProgramCode = ProgramCode
                    };

                    string apiBotReq = JsonConvert.SerializeObject(getWhatsappMessageDetailsModal);
                    string apiBotResponse = CommonService.SendApiRequest(ClientAPIURL + "api/ChatbotBell/GetWhatsappMessageDetails", apiBotReq, token);

                    //if (!string.IsNullOrEmpty(apiBotResponse.Replace("[]", "").Replace("[", "").Replace("]", "")))
                    //{
                    //    getWhatsappMessageDetailsResponse = JsonConvert.DeserializeObject<GetWhatsappMessageDetailsResponse>(apiBotResponse.Replace("[", "").Replace("]", ""));
                    //}

                    if (!string.IsNullOrEmpty(apiBotResponse.Replace("[]", "").Replace("[", "").Replace("]", "")))
                    {
                        getWhatsappMessageDetailsResponseList = JsonConvert.DeserializeObject<List<GetWhatsappMessageDetailsResponse>>(apiBotResponse);
                    }

                    if (getWhatsappMessageDetailsResponseList != null)
                    {
                        if (getWhatsappMessageDetailsResponseList.Count > 0)
                        {
                            getWhatsappMessageDetailsResponse = getWhatsappMessageDetailsResponseList.Where(x => x.TemplateName == whatsapptemplate).FirstOrDefault();
                        }
                    }

                    if (getWhatsappMessageDetailsResponse != null)
                    {
                        if (getWhatsappMessageDetailsResponse.Remarks != null && getWhatsappMessageDetailsResponse.Remarks != "")
                        {
                            string ObjRemark = getWhatsappMessageDetailsResponse.Remarks.Replace("\r\n", "");
                            string[] ObjSplitComma = ObjRemark.Split(',');

                            if (ObjSplitComma.Length > 0)
                            {
                                for (int i = 0; i < ObjSplitComma.Length; i++)
                                {
                                    strpostionNumber += ObjSplitComma[i].Split('-')[0].Trim().Replace("{", "").Replace("}", "") + ",";
                                    strpostionName += ObjSplitComma[i].Split('-')[1].Trim() + ",";
                                }
                            }

                            strpostionNumber = strpostionNumber.TrimEnd(',');
                            strpostionName = strpostionName.TrimEnd(',');
                        }
                    }
                }
                catch (Exception)
                {
                    getWhatsappMessageDetailsResponse = new GetWhatsappMessageDetailsResponse();
                }

                MySqlCommand cmd = new MySqlCommand("SP_PHYGetSmsWhatsUpDataDetails", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@_TenantID", tenantId);
                cmd.Parameters.AddWithValue("@_UserID", userId);
                cmd.Parameters.AddWithValue("@_OrderID", orderId);
                cmd.Parameters.AddWithValue("@_strpostionNumber", strpostionNumber);
                cmd.Parameters.AddWithValue("@_strpostionName", strpostionName);
                cmd.Parameters.AddWithValue("@_sMSWhtappTemplate", sMSWhtappTemplate);

                MySqlDataAdapter da = new MySqlDataAdapter
                {
                    SelectCommand = cmd
                };
                da.Fill(ds);

                if (ds != null && ds.Tables[0] != null)
                {
                    ordersSmsWhatsUpDataDetails = new OrdersSmsWhatsUpDataDetails()
                    {
                        OderID = ds.Tables[0].Rows[0]["OderID"] == DBNull.Value ? 0 : Convert.ToInt32(ds.Tables[0].Rows[0]["OderID"]),
                        AlertCommunicationviaWhtsup = ds.Tables[0].Rows[0]["AlertCommunicationviaWhtsup"] == DBNull.Value ? false : Convert.ToBoolean(ds.Tables[0].Rows[0]["AlertCommunicationviaWhtsup"]),
                        AlertCommunicationviaSMS = ds.Tables[0].Rows[0]["AlertCommunicationviaSMS"] == DBNull.Value ? false : Convert.ToBoolean(ds.Tables[0].Rows[0]["AlertCommunicationviaSMS"]),
                        SMSSenderName = ds.Tables[0].Rows[0]["SMSSenderName"] == DBNull.Value ? string.Empty : Convert.ToString(ds.Tables[0].Rows[0]["SMSSenderName"]),
                        IsSend = ds.Tables[0].Rows[0]["IsSend"] == DBNull.Value ? false : Convert.ToBoolean(ds.Tables[0].Rows[0]["IsSend"]),
                        MessageText = ds.Tables[0].Rows[0]["MessageText"] == DBNull.Value ? string.Empty : Convert.ToString(ds.Tables[0].Rows[0]["MessageText"]),
                        InvoiceNo = ds.Tables[0].Rows[0]["InvoiceNo"] == DBNull.Value ? string.Empty : Convert.ToString(ds.Tables[0].Rows[0]["InvoiceNo"]),
                        MobileNumber = ds.Tables[0].Rows[0]["MobileNumber"] == DBNull.Value ? string.Empty : Convert.ToString(ds.Tables[0].Rows[0]["MobileNumber"]),
                        AdditionalInfo = ds.Tables[1].Rows[0]["additionalInfo"] == DBNull.Value ? string.Empty : Convert.ToString(ds.Tables[1].Rows[0]["additionalInfo"]),
                    };
                    // result = ds.Tables[0].Rows[0]["ChatID"] == DBNull.Value ? 0 : Convert.ToInt32(ds.Tables[0].Rows[0]["ChatID"]);
                    // Message = ds.Tables[0].Rows[0]["Message"] == DBNull.Value ? String.Empty : Convert.ToString(ds.Tables[0].Rows[0]["Message"]);
                    // additionalInfo = ds.Tables[0].Rows[0]["additionalInfo"] == DBNull.Value ? String.Empty : Convert.ToString(ds.Tables[0].Rows[0]["additionalInfo"]);
                }


                if (ordersSmsWhatsUpDataDetails.IsSend)
                {
                    if (ordersSmsWhatsUpDataDetails.AlertCommunicationviaWhtsup)
                    {
                        try
                        {
                            List<string> additionalList = new List<string>();
                            if (additionalInfo != null)
                            {
                                additionalList = ordersSmsWhatsUpDataDetails.AdditionalInfo.Split(",").ToList();
                            }
                            SendFreeTextRequest sendFreeTextRequest = new SendFreeTextRequest
                            {
                                To = ordersSmsWhatsUpDataDetails.MobileNumber.TrimStart('0').Length > 10 ? ordersSmsWhatsUpDataDetails.MobileNumber : "91" + ordersSmsWhatsUpDataDetails.MobileNumber.TrimStart('0'),
                                ProgramCode = ProgramCode,
                                TemplateName = getWhatsappMessageDetailsResponse.TemplateName,
                                AdditionalInfo = additionalList
                            };

                            string apiReq = JsonConvert.SerializeObject(sendFreeTextRequest);
                            apiResponse = CommonService.SendApiRequest(ClientAPIURL + "api/ChatbotBell/SendCampaign", apiReq, "");


                            //if (apiResponse.Equals("true"))
                            //{
                            //    UpdateResponseShare(objRequest.CustomerID, "Contacted Via Chatbot");
                            //}
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    else if (ordersSmsWhatsUpDataDetails.AlertCommunicationviaSMS)
                    {
                        Message = ordersSmsWhatsUpDataDetails.MessageText;

                        //else if (sMSWhtappTemplate == "AWBAssigned" & ordersSmsWhatsUpDataDetails.AWBAssigned)
                        //{
                        //    Message = ordersSmsWhatsUpDataDetails.AWBAssignedText;
                        //}
                        //else if (sMSWhtappTemplate == "PickupScheduled" & ordersSmsWhatsUpDataDetails.PickupScheduled)
                        //{
                        //    Message = ordersSmsWhatsUpDataDetails.PickupScheduledText;
                        //}
                        //else if (sMSWhtappTemplate == "Shipped" & ordersSmsWhatsUpDataDetails.Shipped)
                        //{
                        //    Message = ordersSmsWhatsUpDataDetails.ShippedText;
                        //}
                        //else if (sMSWhtappTemplate == "Delivered" & ordersSmsWhatsUpDataDetails.Delivered)
                        //{
                        //    Message = ordersSmsWhatsUpDataDetails.DeliveredText;
                        //}


                        ChatSendSMS chatSendSMS = new ChatSendSMS
                        {
                            MobileNumber = ordersSmsWhatsUpDataDetails.MobileNumber.TrimStart('0').Length > 10 ? ordersSmsWhatsUpDataDetails.MobileNumber : "91" + ordersSmsWhatsUpDataDetails.MobileNumber.TrimStart('0'),
                            SenderId = ordersSmsWhatsUpDataDetails.SMSSenderName,
                            SmsText = Message
                        };

                        string apiReq = JsonConvert.SerializeObject(chatSendSMS);
                        apiResponse = CommonService.SendApiRequest(ClientAPIURL + "api/ChatbotBell/SendSMS", apiReq, token);

                        ChatSendSMSResponse chatSendSMSResponse = new ChatSendSMSResponse();

                        chatSendSMSResponse = JsonConvert.DeserializeObject<ChatSendSMSResponse>(apiResponse);

                        if (chatSendSMSResponse != null)
                        {
                            result = chatSendSMSResponse.Id;
                        }

                        //if (result > 0)
                        //{
                        //    UpdateResponseShare(objRequest.CustomerID, "Contacted Via SMS");
                        //}
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
                if (ds != null)
                {
                    ds.Dispose();
                }
            }

            return result;
        }


        public static string GetWhatsupTemplateName(int TenantID, int UserID, string MessageName)
        {
            string WhatsupTemplateName = "";
            DataSet ds = new DataSet();
            try
            {
                if (conn != null)
                {
                    conn.Close();
                }
                conn.Open();

                MySqlCommand cmd = new MySqlCommand("SP_PHYGetWhatsupTemplate", conn)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@_TenantId", TenantID);
                cmd.Parameters.AddWithValue("@_UserID", UserID);
                cmd.Parameters.AddWithValue("@_MessageName", MessageName);


                MySqlDataAdapter da = new MySqlDataAdapter
                {
                    SelectCommand = cmd
                };
                da.Fill(ds);
                if (ds != null && ds.Tables[0] != null)
                {
                    WhatsupTemplateName = ds.Tables[0].Rows[0]["TemplateName"] == DBNull.Value ? String.Empty : Convert.ToString(ds.Tables[0].Rows[0]["TemplateName"]);
                }
            }
            catch (Exception ex)
            {

            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }

            return WhatsupTemplateName;
        }

    }
}
