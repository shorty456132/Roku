using System;
using System.Text;
using Crestron.SimplSharp;                          				// For Basic SIMPL# Classes
using Crestron.SimplSharp.Net;
using Crestron.SimplSharp.Net.Http;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharp.CrestronSockets;
using Crestron.SimplSharp.CrestronWebSocketClient;
using System.Collections.Specialized;

namespace RokuTV
{
    public delegate void AppsHandler (SimplSharpString RxString);

    public class RokuControl
    {
        public AppsHandler OnAppsHandler { get; private set; }

        private static int port = 8060;
        private static string _IPAddress;
        public string status;
        public string myCommand;
        public string fromRoku;
        /// <summary>
        /// SIMPL+ can only execute the default constructor. If you have variables that require initialization, please
        /// use an Initialize method
        /// </summary>
        public RokuControl()
        {
        }

        public void initialize(string IPAddress)
        {
            _IPAddress = IPAddress;
            CrestronConsole.PrintLine(String.Format("IP Address Set To: {0}", _IPAddress));
        }

        public void sendCommand(string command)
        {
            HttpClient myClient = new HttpClient();
            myClient.AllowAutoRedirect = true;
            myClient.Verbose = true;

            HttpClientRequest myRequest = new HttpClientRequest();
            HttpClientResponse myResponse;

            try
            {
                myRequest.RequestType = RequestType.Post;
                myRequest.Header.SetHeaderValue("content-type", "text/html");

                myClient.TimeoutEnabled = true;
                myClient.Timeout = 30;
                myRequest.KeepAlive = true;

                myRequest.Url = new UrlParser(String.Format("http://{0}:{1}{2}",_IPAddress,port,command)); //http://10.10.1.15:8060/keypad/power
                myResponse = myClient.Dispatch(myRequest);

                CrestronConsole.PrintLine(myResponse.ContentString);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Exception in Roku Connect: {0}", e.Message);
            }
            finally
            {
                myClient.Dispose();
            }
        }

        public void QueryCommand(string command)
        {
            HttpClient myClient = new HttpClient();
            myClient.AllowAutoRedirect = true;
            myClient.Verbose = true;

            HttpClientRequest myRequest = new HttpClientRequest();
            HttpClientResponse myResponse;

            try
            {
                myRequest.RequestType = RequestType.Get;
                myRequest.Header.SetHeaderValue("content-type", "test/html");

                myClient.TimeoutEnabled = true;
                myClient.Timeout = 30;
                myRequest.KeepAlive = true;

                myRequest.Url = new UrlParser(String.Format("http://{0}:{1}{2}", _IPAddress, port, command));
                myResponse = myClient.Dispatch(myRequest);

                CrestronConsole.PrintLine(String.Format("{0}", myResponse.Encoding));
                //TODO parse out responses from query. for now just dumping all into fromRoku.
                ParseResponse(myResponse.ContentString);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Exception in Roku Connect: {0}", e.Message);
            }
            finally
            {
                myClient.Dispose();
            }
        }

        public void ParseResponse(string receiveRoku)
        {
            foreach (var item in receiveRoku)
            {
                CrestronConsole.PrintLine(String.Format("items in rx string: {0}", item.ToString()));

                if (OnAppsHandler != null)
                    OnAppsHandler(item.ToString());
            }
        }
    }
}
