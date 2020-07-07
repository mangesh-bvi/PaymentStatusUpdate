using Microsoft.Extensions.Configuration;
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
                GetConnectionStrings();
               
                Thread.Sleep(delaytime);
            }
        }

        /// <summary>
        /// Get All ConnectionStrings for multitenancy
        /// </summary>
        public static void GetConnectionStrings()
        {
            string ServerName = string.Empty;
            string ServerCredentailsUsername = string.Empty;
            string ServerCredentailsPassword = string.Empty;
            string DBConnection = string.Empty;

            MySqlConnection con = null;
            try
            {
                DataTable dt = new DataTable();
                IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
                var constr = config.GetSection("ConnectionStrings").GetSection("HomeShop").Value;
                con = new MySqlConnection(constr);
                MySqlCommand cmd = new MySqlCommand("SP_HSGetAllConnectionstrings", con);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Connection.Open();
                MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                da.Fill(dt);
                cmd.Connection.Close();

                if (dt.Rows.Count > 0)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        DataRow dr = dt.Rows[i];
                        ServerName = Convert.ToString(dr["ServerName"]);
                        ServerCredentailsUsername = Convert.ToString(dr["ServerCredentailsUsername"]);
                        ServerCredentailsPassword = Convert.ToString(dr["ServerCredentailsPassword"]);
                        DBConnection = Convert.ToString(dr["DBConnection"]);

                        string ConString = "Data Source = " + ServerName + " ; port = " + 3306 + "; Initial Catalog = " + DBConnection + " ; User Id = " + ServerCredentailsUsername + "; password = " + ServerCredentailsPassword + "";
                        GetdataFromMySQL(ConString);
                    }
                }
            }
            catch
            {


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


        /// <summary>
        /// Get data FromMySQL where Payment Pending
        /// </summary>
        /// <param name="ConString"></param>
        public static void GetdataFromMySQL(string ConString)
        {
            int ID = 0;
            string InvoiceNo = string.Empty;
            string Date = string.Empty;
            string CustomerName = string.Empty;
            string MobileNumber = string.Empty;
            string TokenId = string.Empty;
            string Alias = string.Empty;
            string StoreCode = string.Empty;
            string CompanayName = string.Empty;
            string apiResponse = string.Empty;
            string apitokenRes = string.Empty;
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
                
                string ClientAPIURL = config.GetSection("MySettings").GetSection("ClientAPIURL").Value;
                string clientAPIUrlForGenerateToken = config.GetSection("MySettings").GetSection("clientAPIUrlForGenerateToken").Value;
                string TerminalId = config.GetSection("MySettings").GetSection("TerminalId").Value;
                string Client_Id = config.GetSection("MySettings").GetSection("Client_Id").Value;
                string Client_Secret = config.GetSection("MySettings").GetSection("Client_Secret").Value;
                string Grant_Type = config.GetSection("MySettings").GetSection("Grant_Type").Value;
                string Scope = config.GetSection("MySettings").GetSection("Scope").Value;

                con = new MySqlConnection(ConString);
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
                        Date = Convert.ToDateTime(dr["Date"]).ToString("yyyy-MM-dd HH:mm:ss");
                        CustomerName = Convert.ToString(dr["CustomerName"]);
                        MobileNumber = Convert.ToString(dr["MobileNumber"]);
                        TokenId = Convert.ToString(dr["TokenId"]);
                        Alias = Convert.ToString(dr["Alias"]);
                        StoreCode = Convert.ToString(dr["StoreCode"]);
                        CompanayName = Convert.ToString(dr["ProgramCode"]);
                        ShippingAddress = Convert.ToString(dr["ShippingAddress"]);
                        PinCode = Convert.ToString(dr["PinCode"]);
                        City = Convert.ToString(dr["City"]);
                        State = Convert.ToString(dr["State"]);
                        Country = Convert.ToString(dr["Country"]);
                        DeliveryType = Convert.ToString(dr["DeliveryType"]);

                        var dtOffset = DateTimeOffset.Parse(Date, CultureInfo.InvariantCulture);

                        string apiReq = "Client_Id=" + Client_Id + "&Client_Secret=" + Client_Secret + "&Grant_Type=" + Grant_Type + "&Scope=" + Scope;

                        apitokenRes = CommonService.SendApiRequestToken(clientAPIUrlForGenerateToken + "connect/token", apiReq);
                        HSResponseGenerateToken hSResponseGenerateToken = new HSResponseGenerateToken();
                        hSResponseGenerateToken = JsonConvert.DeserializeObject<HSResponseGenerateToken>(apitokenRes);

                        PaymentStatusRequest paymentStatus = new PaymentStatusRequest
                        {
                            tokenId = TokenId,
                            programCode = CompanayName,
                            storeCode = StoreCode,
                            //billDateTime = dtOffset.ToString("yyyy-MM-dd'T'HH:mm:ss.249'Z'"),
                            billDateTime = dtOffset.ToString("dd-MMM-yyyy hh:mm:ss"),
                            terminalId = TerminalId,
                            merchantTxnID = InvoiceNo,
                            mobile = MobileNumber.TrimStart('0')
                        };
                        string apiReqpayment = JsonConvert.SerializeObject(paymentStatus);

                        if (!string.IsNullOrEmpty(hSResponseGenerateToken.access_token))
                        {
                            apiResponse = CommonService.SendApiRequest(ClientAPIURL + "/api/GetPaymentStatus", apiReqpayment, hSResponseGenerateToken.access_token);
                            paymentapiResponse = JsonConvert.DeserializeObject<PaymentStatusResponse>(apiResponse);
                        }
                        if (paymentapiResponse.returnCode == "0" && paymentapiResponse.tokenStatus.Contains("Success"))
                        {
                            if (ShippingAddress != "" && PinCode != "" && City != "" && State != "" && Country != "" && DeliveryType != "Pickup")
                            {
        
                                UpdateResponse(ID, /*paymentapiResponse.status*/ "PaymentDetails", ConString);
                            }
                            else
                            {
                                UpdatePaymentResponse(ID, /*paymentapiResponse.status*/ "PaymentDetails", DeliveryType, ConString);
                            }

                        }
                        else
                        {
                            ExLogger(ID, InvoiceNo, Date, StoreCode, paymentapiResponse.returnMessage, paymentapiResponse.tokenStatus, ConString);
                        }

                    }

                }
            }
            catch (Exception ex)
            {

                ExLogger(ID, InvoiceNo, Date, StoreCode, ex.Message, ex.StackTrace, ConString);
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

        /// <summary>
        /// Update Response
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Status"></param>
        /// <param name="ConString"></param>
        public static void UpdateResponse(int ID, string Status,string ConString)
        {
            MySqlConnection con = null;
            try
            {
                DataTable dt = new DataTable();
                con = new MySqlConnection(ConString);
               
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
                if (con != null)
                {
                    con.Close();
                }
                GC.Collect();
            }

        }

        /// <summary>
        /// Update Payment Response
        /// </summary>
        /// <param name="ID"></param>
        /// <param name="Status"></param>
        /// <param name="DeliveryType"></param>
        /// <param name="ConString"></param>
        public static void UpdatePaymentResponse(int ID, string Status, string DeliveryType, string ConString)
        {
            MySqlConnection con = null;
            try
            {
                DataTable dt = new DataTable();

                con = new MySqlConnection(ConString);
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
            catch (Exception ex)
            {

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

        /// <summary>
        /// Exception Logger
        /// </summary>
        /// <param name="TransactionID"></param>
        /// <param name="BillNo"></param>
        /// <param name="BillDate"></param>
        /// <param name="StoreCode"></param>
        /// <param name="ErrorMessage"></param>
        /// <param name="ErrorDiscription"></param>
        /// <param name="ConString"></param>
        public static void ExLogger(int TransactionID, string BillNo, string BillDate, string StoreCode, string ErrorMessage, string ErrorDiscription, string ConString)
        {
            MySqlConnection con = null;
            try
            {
                DataTable dt = new DataTable();
                IConfiguration config = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
                var constr = config.GetSection("ConnectionStrings").GetSection("HomeShop").Value;
                con = new MySqlConnection(ConString);
                MySqlCommand cmd = new MySqlCommand("SP_PHYInsertErrorLog", con)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@_transactionID", TransactionID);
                cmd.Parameters.AddWithValue("@_billNo", BillNo);
                cmd.Parameters.AddWithValue("@_billDate", BillDate);
                cmd.Parameters.AddWithValue("@_storeCode", StoreCode);
                cmd.Parameters.AddWithValue("@_errorMessage", ErrorMessage);
                cmd.Parameters.AddWithValue("@_errorDiscription", ErrorDiscription);
                cmd.Parameters.AddWithValue("@_repeatCount", 0);
                cmd.Parameters.AddWithValue("@_functionName", "Payment Status");
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
                cmd.Connection.Close();
            }
            catch (Exception ex)
            {
                //write code for genral exception
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

    }
}
