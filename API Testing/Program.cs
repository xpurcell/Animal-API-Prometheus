using Prometheus;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace API_Testing
{
    class Program 
    {

        // different metrics
        private static readonly Gauge ApiCatUp =
      Metrics.CreateGauge("Ping_Animal_1_API_up", "Ping Animal 1 API is up");

        private static readonly Gauge ApiCatDown =
      Metrics.CreateGauge("Ping_Animal_1_API_down", "Ping Animal 1 API is down");

        private static readonly Gauge ApiDogUp =
     Metrics.CreateGauge("Ping_Animal_2_API_up", "Ping Animal 2 API is up");

        private static readonly Gauge ApiDogDown =
      Metrics.CreateGauge("Ping_Animal_2_API_down", "Ping Animal 2 API is down");

        private static readonly Gauge ApiHorseUp =
     Metrics.CreateGauge("Horse_Animal_3_API_up", "Ping Animal 3 API is up");

        private static readonly Gauge ApiHorseDown =
      Metrics.CreateGauge("Horse_Animal_3_API_down", "Ping Animal 3 API is down");

        private static readonly Gauge ApiHansardUp =
   Metrics.CreateGauge("Horse_Animal_3_API_down", "Ping Animal 3 API is down");

        private static readonly Gauge ApiHansardDown =
   Metrics.CreateGauge("Horse_Animal_3_API_down", "Ping Animal 3 API is down");

        // class made with onine c#json tool http://json2csharp.com/

        public class animal
        {
            public bool used { get; set; }
            public string source { get; set; }
            public string type { get; set; }
            public bool deleted { get; set; }
            public string id { get; set; }
            public string user { get; set; }
            public string text { get; set; }
            public int __v { get; set; }
            public DateTime updatedAt { get; set; }
            public DateTime createdAt { get; set; }
        }

        // 
        public static string[] dictAnimalType = { "cat", "dog", "horse" };
        static void Main(string[] args)
        {
            var server = new MetricServer(hostname: "localhost", port: 1237); // need to scrap this port
            server.Start();

            // this is for a pushgateway
            /*var pusher = new MetricPusher(new MetricPusherOptions
            {
                Endpoint = "http://grafanaserver.westus2.cloudapp.azure.com:9091/metrics",
                Job = "123"
            });

            pusher.Start();*/


            Console.WriteLine("Start Server and API!");
            int x = 0;
            int y = 1;
            int animalNumber = 0;
          

            // get the write url/params
            string[] dictUrl = { "https://cat-fact.herokuapp.com/facts/random?animal_type="+dictAnimalType[0]+"&amount=2", "https://cat-fact.herokuapp.com/facts/random?animal_type=" + dictAnimalType[1] + "&amount=2", "https://cat-fact.herokuapp.com/facts/random?animal_type=" + dictAnimalType[2] + "&amount=2" };
            Gauge[] dictGauge = { ApiCatUp, ApiCatDown, ApiDogUp, ApiDogDown, ApiHorseUp, ApiHorseDown };
            // keep looping through so we can keep calling the api
            while (true)
            {
             //   Console.WriteLine(ApiCatUp.Value);
             //   Console.WriteLine(ApiCatDown.Value+"CatDown");
             //   Console.WriteLine(ApiDogUp.Value);
             //   Console.WriteLine(ApiDogDown.Value);
             //   Console.WriteLine(ApiHorseUp.Value);
            //    Console.WriteLine(ApiHorseDown.Value);
                Ping(dictUrl[animalNumber],dictGauge[x],dictGauge[y], animalNumber);

                if (x==4)
                {
                    x = -2;
                    
                }
                if (y==5)
                {
                    y = -1;
                    animalNumber = -1;
                    Thread.Sleep(15000);
                }
                animalNumber++;
                x=x+2;
                y = y + 2;

            }
        }


        private static void Ping(string url, Gauge apiUpName, Gauge apiDownName, int animalCounter)
        {
            int x = 0;
            bool testGoing = true;

                //start timer
                Stopwatch clockStop = Stopwatch.StartNew();
                string clockString = clockStop.ElapsedMilliseconds.ToString();
                Console.WriteLine("starting clock");
                clockStop = Stopwatch.StartNew();
                testGoing = true;
                while (testGoing)
                {
                    string responseCode = "";
                    try
                    {
                        // create a http request for a ping
                        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                        request.Timeout = 3000;
                        request.AllowAutoRedirect = false; // find out if this site is up and don't follow a redirector
                        request.Method = "HEAD";
                        System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
                        responseCode = response.StatusCode.ToString();
                        if (responseCode == "OK")
                        {
                            x = 0;

                            Console.WriteLine("Returned "+responseCode);

                            var data = response.GetResponseStream();

                        // crate a struct for our json class
                            List<animal> items2 = new List<animal>(); ;

                            try
                            {
                            // put the json we find into our json class
                                string json2 = new WebClient().DownloadString(url);

                                items2 = JsonConvert.DeserializeObject<List<animal>>(json2);

                                foreach (var item in items2)
                                {
                                    Console.WriteLine(item.type.ToString());
                                    Console.WriteLine(item.text.ToString());
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                
                            }

                        // comparison to check to see if our results are working from the api.
                            if (items2[0].type ==dictAnimalType[animalCounter]  && items2[1].type== dictAnimalType[animalCounter])
                            {
                                apiUpName.Set(100);
                                apiDownName.Set(0);
                                testGoing = false;
                            Console.WriteLine("\nComparsion Passed " + dictAnimalType[animalCounter]+" We got a "+ dictAnimalType[animalCounter]+ " Facts \n");
                            Thread.Sleep(6000);
                            }
                            else
                            {
                            apiUpName.Set(0);
                            apiDownName.Set(100);
                                testGoing = false;
                            Console.WriteLine("comparsion failed");
                        }
                        }
                        else
                        {
                            Console.WriteLine("Returned " + responseCode);
                        }
                    testGoing = false;
                    }
                    catch(Exception e)
                    {
                    // get error code
                        var checker = e.ToString();
                        var checkerSplit = checker.Split(' ');
                        var errorCode = checkerSplit[7];

                        if (responseCode=="")
                        {
                            Console.WriteLine("Response Code "+ errorCode);
                        }
                        else
                        {
                            Console.WriteLine("Response Code "+responseCode);
                        }
                       
                        x++;

                        if (x == 5)
                        {
                        apiUpName.Set(0);
                        apiDownName.Set(100);
                        testGoing = false;
                        }
                    }
                clockStop.Stop();
                Console.WriteLine(clockStop.ElapsedMilliseconds);
            }
        }


    }
}
