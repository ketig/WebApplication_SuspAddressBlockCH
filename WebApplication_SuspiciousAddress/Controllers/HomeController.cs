using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using Transaction = WebApplication_SuspiciousAddress.Models.Transaction;


namespace WebApplication_SuspiciousAddress.Controllers
{
    public class HomeController : Controller
    {
        //Hosted web API REST Service base url
        string Baseurl = "https://api.trongrid.io/";

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        public async Task<ActionResult> Result(string address, int level, DateTime from, DateTime to)
        {
            // Result
            List<Transaction> transactions = new List<Transaction>();

            DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timestampfrom = (long)(from.ToUniversalTime() - unixEpoch).TotalMilliseconds;
            long timestampto = (long)(to.AddDays(1).ToUniversalTime() - unixEpoch).TotalMilliseconds;

            await GetAllLevels(transactions, address, timestampfrom, timestampto, level, 1);

            //returning the employee list to view
            return View(transactions);
        }

        public async Task GetAllLevels(List<Transaction> transactions, string address, long timestampfrom, long timestampto,
            int level, int currentLevel)
        {
            if (currentLevel > level)
            {
                return;
            }

            using (var client = new HttpClient())
            {
                //Passing service base url
                client.BaseAddress = new Uri(Baseurl);
                client.DefaultRequestHeaders.Clear();

                //Define request data format
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                //Sending request to find web api REST service resource Get transaction using HttpClient
                HttpResponseMessage Res = await client.GetAsync($"v1/accounts/{address}/transactions");

                //Checking the response is successful or not which is sent using HttpClient
                if (Res.IsSuccessStatusCode)
                {
                    // Read the response content as a string.
                    var responseContent = Res.Content.ReadAsStringAsync().Result;

                    // Deserialize the JSON data into a dynamic object.
                    dynamic responseData = JsonConvert.DeserializeObject(responseContent);

                    // Loop through the transaction data and add it to the list.
                    foreach (var dataItem in responseData.data)
                    {
                        var rawHexData = dataItem.raw_data;
                        try
                        {
                            var timestamp = (long)rawHexData["timestamp"];
                            var amount = (long?)rawHexData["contract"][0]["parameter"]["value"]["amount"];
                            var ownerAddress = (string)rawHexData["contract"][0]["parameter"]["value"]["owner_address"];
                            var toAddress = (string)rawHexData["contract"][0]["parameter"]["value"]["to_address"];
                            var transferContract = (string)rawHexData["contract"][0]["type"];

                            if (transferContract == "TransferContract" && ownerAddress == address &&
                                timestampfrom <= timestamp && timestamp <= timestampto)
                            {
                                transactions.Add(new Transaction()
                                {
                                    Amount = amount,
                                    OwnerAddress = ownerAddress,
                                    ToAddress = toAddress,
                                    Time = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timestamp).ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss"),
                                    Distance = currentLevel
                                });

                                await GetAllLevels(transactions, toAddress, timestampfrom, timestampto, level, currentLevel + 1);
                            }
                        }
                        catch (Exception ex)
                        {

                        }
                    }
                }
            }
        }
    }
}