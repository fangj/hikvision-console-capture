using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleHikvision
{
    class Program
    {
        private static uint iLastErr = 0;
        private static Int32 m_lUserID = -1;
        private static bool m_bInitSDK = false;
        private static bool m_bRecord = false;
        private static Int32 m_lRealHandle = -1;
        private static string str;


        [Verb("capture", HelpText = "capture photo to a jpeg file.")]
        class CaptureOptions
        {

            // Omitting long name, defaults to name of property, ie "--verbose"
            [Option('i',Default = "192.168.3.64",Required =true, HelpText = "The IP of camera.")]
            public string IP { get; set; }

            [Option("port", Default = 8000,  HelpText = "The port of camera.")]
            public int Port { get; set; }

            [Option('o',Default = "capture.jpg",  HelpText = "The output jpeg file path.")]
            public string OutFile { get; set; }

            [Option('u',Default = "admin", HelpText = "The user name for login camera.")]
            public string UserName { get; set; }

            [Option('p',Default = "haikang123", HelpText = "The password for login camera..")]
            public string Password { get; set; }

            [Usage(ApplicationAlias = "hikvision")]
            public static IEnumerable<Example> Examples
            {
                get
                {
                    yield return new Example("Simple", UnParserSettings.WithGroupSwitchesOnly(), new CaptureOptions { IP = "192.168.3.64"});
                    yield return new Example("Full", UnParserSettings.WithGroupSwitchesOnly(), new CaptureOptions { IP = "192.168.3.64", Port=8000, OutFile = "c:\\capture\\capture.jpg", UserName="admin",Password= "haikang123" });
                }
            }

        }

        static int Main(string[] args)
        {
           return CommandLine.Parser.Default.ParseArguments<CaptureOptions>(args)
                .MapResult(
                    options => RunOptionsAndReturnExitCode(options),
                    _ => 1);
        }

        static int RunOptionsAndReturnExitCode(CaptureOptions options)
        {
            InitCamera();
            Login(options.IP,options.Port,options.UserName,options.Password);
            Capture(options.OutFile);
            Logout(options.IP, options.Port);
            CleanUpCamera();
            StopCloseConsoleOnDebug();
            return (int)iLastErr;
        }

        static void StopCloseConsoleOnDebug()
        {
            if (Debugger.IsAttached)
            {
                Console.WriteLine("Press any key to continue . . .");
                Console.ReadLine();
            }
        }
        static void InitCamera()
        {
            m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            if (m_bInitSDK == false)
            {
                Debug.WriteLine("NET_DVR_Init error!");
                return;
            }
            else
            {
                //保存SDK日志 To save the SDK log
                CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog\\", true);
            }
        }

        static void CleanUpCamera()
        {
            CHCNetSDK.NET_DVR_Cleanup();
        }

 

        static private void Login(string ip="192.168.3.64",int port=8000,string username="admin",string password= "haikang123")
        {
            CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();
            //登录设备 Login the device
            m_lUserID = CHCNetSDK.NET_DVR_Login_V30(ip, port, "admin", "haikang123", ref DeviceInfo);
            if (m_lUserID < 0)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string str = "NET_DVR_Login_V30 failed, error code= " + iLastErr; //登录失败，输出错误号
                Console.WriteLine(str);
                Console.WriteLine("Login HK device fail:" + " ip:" + ip);
                return;
            }
            else
            {
                //登录成功
                Debug.WriteLine("Login Success!");
            }
        }

        static private void Logout(string ip = "192.168.3.64", int port = 8000)
        {
            //注销登录 Logout the device
            if (m_lRealHandle >= 0)
            {
                Console.WriteLine("Please stop live view firstly");
                return;
            }

            if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_Logout failed, error code= " + iLastErr;
                Console.WriteLine(str);
                return;
            }
            m_lUserID = -1;
        }



        static public int Capture(string outfile= "capture.jpg")
        {
            Debug.WriteLine("Capture");
            if (m_lUserID < 0)
            {
                Console.WriteLine("not login, cannot capture");
                return -1;
            }
            string sJpegPicFileName;
            //图片保存路径和文件名 the path and file name to save
            sJpegPicFileName = outfile;

            int lChannel = 1; //通道号 Channel number

            CHCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new CHCNetSDK.NET_DVR_JPEGPARA();
            lpJpegPara.wPicQuality = 0; //图像质量 Image quality
            lpJpegPara.wPicSize = 0xff; //抓图分辨率 Picture size: 2- 4CIF，0xff- Auto(使用当前码流分辨率)，抓图分辨率需要设备支持，更多取值请参考SDK文档

            //JPEG抓图 Capture a JPEG picture
            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture(m_lUserID, lChannel, ref lpJpegPara, sJpegPicFileName))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                String str = "NET_DVR_CaptureJPEGPicture failed, error code= " + iLastErr;
                Console.WriteLine(str);
                return -2;
            }
            else
            {
                String str = "Successful to capture the JPEG file and the saved file is " + sJpegPicFileName;
                Console.WriteLine(str);
            }
            return 0;
        }
    }
}
