using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Google.Cloud.Speech.V1; //Beta1;
using System.Linq;
using Grpc.Core;
using Google.Apis.Auth.OAuth2;
using Grpc.Auth;
using System.IO;
using System.Text;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            //x-api-key
            var auth_token = @"@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@";
            var credentials = GoogleCredential.FromAccessToken("@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@");

            var channel = new Channel(SpeechClient.DefaultEndpoint.Host, credentials.ToChannelCredentials());

            //Grpc.Core.AsyncAuthInterceptor
            //var speech = await SpeechClient.CreateAsync();

            var metadata = new Metadata
            {
                { "authorization", "Bearer " + auth_token }
            };

            var speech = new Speech.SpeechClient(channel);
            speech.StreamingRecognize(metadata);

            // DRAGAN: the 'call' is the real unit of streaming so to speak, that's disposable and what need to restart
            //var _streamingCall = speech.GrpcClient.StreamingRecognize(metadata); // null, null, _cancellationTokenSource.Token);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var userDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var info = new ProcessStartInfo(
                Path.Combine(userDir, @"AppData\Local\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd"),
                "auth application-default print-access-token");

            //var info = new ProcessStartInfo(
            //    @"C:\Users\draga\AppData\Local\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd",
            //    "auth application-default print-access-token");

            //Process.Start(
            //    @"C:\Users\draga\AppData\Local\Google\Cloud SDK\google-cloud-sdk\bin\gcloud.cmd",
            //    "auth application-default print-access-token").WaitForExit();
            ////Process.Start("cmd.exe /c gettoken.bat > token.log").WaitForExit();
            //return;

            //Process.Start(@"C:\Windows\System32\cmd.exe", "/c");
            Process p = new Process();
            //ProcessStartInfo info = new ProcessStartInfo();
            info.EnvironmentVariables["GOOGLE_APPLICATION_CREDENTIALS"] = "Any-App-Dictation v1-5cc9c575f099.json"; // "PP-Speed-f252b9a4cde9.json";
            //info.FileName = "gettoken.bat"; //.cmd"; // auth application-default print-access-token"; // "cmd.exe";
            //info.RedirectStandardInput = true;
            info.UseShellExecute = false;

            info.StandardOutputEncoding = System.Text.Encoding.UTF8;
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.WindowStyle = ProcessWindowStyle.Hidden;
            info.RedirectStandardOutput = true;
            info.RedirectStandardError = true;
            //info.RedirectStandardInput = true;

            p.EnableRaisingEvents = true;

            p.StartInfo = info;
            //p.StartInfo.Arguments = "auth application-default print-access-token";
            //p.Start();

            StringBuilder sb = new StringBuilder();
            StringBuilder sbErr = new StringBuilder();
            p.OutputDataReceived += (sender, args) => sb.AppendLine(args.Data);
            p.ErrorDataReceived += (s, e) => sbErr.AppendLine(e.Data);
            p.Start();
            p.BeginOutputReadLine();
            //Thread.Sleep(5);
            p.WaitForExit();
            Console.WriteLine(sb.ToString());
            return;

            //using (StreamWriter sw = p.StandardInput)
            //{
            //    if (sw.BaseStream.CanWrite)
            //    {
            //        sw.WriteLine("cd \"c:\\sw\\dev\\samples\\guru\\Sven\\BlueMaria.WPFClient\\BlueMaria\\bin\\Debug\\\"");
            //        sw.WriteLine("gcloud auth activate-service-account 342018252753-compute@developer.gserviceaccount.com --key-file=\"Any - App - Dictation v1 - 5cc9c575f099.json\"");
            //        sw.WriteLine("gcloud auth print-access-token");
            //        //sw.WriteLine("mysql -u root -p");
            //        //sw.WriteLine("mypassword");
            //        //sw.WriteLine("use mydb;");
            //    }
            //}

            p.WaitForExit();

            //string output = p.StandardOutput.ReadToEnd();
            var output = "";
            using (var sr = p.StandardOutput)
            {
                output = sr.ReadToEnd();
            }

            p.WaitForExit();
        }
    }
}
