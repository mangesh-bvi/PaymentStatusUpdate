﻿using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using PaymentStatusService.Model;
using PaymentStatusService.Service;
using System;
using System.Data;
using System.Globalization;
using System.Threading;

namespace PaymentStatusService
{
    class Program
    {
        public static int delaytime = 0;

        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
            delaytime = Convert.ToInt32(config.GetSection("MySettings").GetSection("IntervalInMinutes").Value);
           
            Thread _Individualprocessthread = new Thread(new ThreadStart(InvokeMethod));
            _Individualprocessthread.Start();
        }
        public static void InvokeMethod()
        {
            while (true)
            {
                GetdataFromMySQL();
                Thread.Sleep(delaytime);
            }
        }
        public static void GetdataFromMySQL()
        {
            int ID = 0;
            string InvoiceNo = string.Empty;
            string Date = string.Empty;
            string CustomerName = string.Empty;
            string MobileNumber = string.Empty;
            string TokenId = string.Empty;
            string Alias = string.Empty;
            string StoreName = string.Empty;
            string CompanayName = string.Empty;
            string apiResponse = string.Empty;
            string ShippingAddress = string.Empty;
            string PinCode = string.Empty;
            string City = string.Empty;
            string State = string.Empty;
            string Country = string.Empty;
            string DeliveryType = string.Empty;

            PaymentStatusResponse paymentapiResponse = new PaymentStatusResponse();

            MySqlConnection con = null;
            try
            {
                DataTable dt = new DataTable();

                IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
                var constr = config.GetSection("ConnectionStrings").GetSection("HomeShop").Value;
                string ClientAPIURL = config.GetSection("MySettings").GetSection("ClientAPIURL").Value;
                string TerminalId = config.GetSection("MySettings").GetSection("TerminalId").Value;

                con = new MySqlConnection(constr);
                MySqlCommand cmd = new MySqlCommand("SP_PHYGetPaymentDetails", con)
                {
                    CommandType = System.Data.CommandType.StoredProcedure
                };
                cmd.Connection.Open();
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(dt);
                cmd.Connection.Close();
                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        ID = Convert.ToInt32(dr["ID"]);
                        InvoiceNo = Convert.ToString(dr["InvoiceNo"]);
                        Date = Convert.ToString(dr["Date"]);
                        CustomerName = Convert.ToString(dr["CustomerName"]);
                        MobileNumber = Convert.ToString(dr["MobileNumber"]);
                        TokenId = Convert.ToString(dr["TokenId"]);
                        Alias = Convert.ToString(dr["Alias"]);
                        StoreName = Convert.ToString(dr["StoreName"]);
                        CompanayName = Convert.ToString(dr["CompanayName"]);
                        ShippingAddress = Convert.ToString(dr["ShippingAddress"]);
                        PinCode = Convert.ToString(dr["PinCode"]);
                        City = Convert.ToString(dr["City"]);
                        State = Convert.ToString(dr["State"]);
                        Country = Convert.ToString(dr["Country"]);
                        DeliveryType = Convert.ToString(dr["DeliveryType"]);

                        var dtOffset = DateTimeOffset.Parse(Date, CultureInfo.InvariantCulture);
                        

                        PaymentStatusRequest paymentStatus = new PaymentStatusRequest
                        {
                            tokenId = TokenId,
                            programCode = CompanayName,
                            storeCode = StoreName,
                            billDateTime = dtOffset.ToString("yyyy-MM-dd'T'HH:mm:ss.249'Z'"),
                            terminalId = TerminalId,
                            merchantTxnID = InvoiceNo,
                            mobile = MobileNumber.Length > 10 ? MobileNumber : "91" + MobileNumber.TrimStart('0')
                        };
                        string apiReq = JsonConvert.SerializeObject(paymentStatus);
                        apiResponse = CommonService.SendApiRequest(ClientAPIURL + "/api/GetPaymentStatus", apiReq);
                        paymentapiResponse = JsonConvert.DeserializeObject<PaymentStatusResponse>(apiResponse);

                        if (paymentapiResponse.returnCode == "0" && paymentapiResponse.returnMessage == "Success")
                        {
                            if (ShippingAddress !="" && PinCode !="" && City!=""  && State!="" && Country!="" && DeliveryType!= "SelfPickedup")
                            {
                                UpdateResponse(ID, /*paymentapiResponse.status*/ "PaymentDetails");
                            }
                            else
                            {
                                UpdatePaymentResponse(ID, /*paymentapiResponse.status*/ "PaymentDetails", DeliveryType);
                            }
                            
                        }
                        else
                        {
                            //logged to Elastic
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (con != null)
                {
                    con.Close();
                }
                GC.Collect();
            }
        }

        public static void UpdateResponse(int ID, string Status)
        {

            try
            {
                DataTable dt = new DataTable();
                IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
                var constr = config.GetSection("ConnectionStrings").GetSection("HomeShop").Value;
                MySqlConnection con = new MySqlConnection(constr);
                MySqlCommand cmd = new MySqlCommand("SP_PHYUpdatePaymentStatus", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@_alias", Status);
                cmd.Parameters.AddWithValue("@_iD", ID);
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch
            {

            }
            finally
            {
                GC.Collect();
            }

        }


        public static void UpdatePaymentResponse(int ID, string Status,string DeliveryType)
        {

            try
            {
                DataTable dt = new DataTable();
                IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
                var constr = config.GetSection("ConnectionStrings").GetSection("HomeShop").Value;
                MySqlConnection con = new MySqlConnection(constr);
                MySqlCommand cmd = new MySqlCommand("SP_PHYUpdatePaymentStatusAction", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@_alias", Status);
                cmd.Parameters.AddWithValue("@_iD", ID);
                cmd.Parameters.AddWithValue("@_deliverytype", DeliveryType);

                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch(Exception ex)
            {

            }
            finally
            {
                GC.Collect();
            }

        }


    }
}
