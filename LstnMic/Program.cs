using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

using CommandLine;
using Google.Cloud.Speech.V1; //Beta1;

using System.Linq;
using Grpc.Core;
using Grpc.Auth;
using BlueMaria.Utilities;
using System.Text;

// DRAGAN: This is where the core changes are. It was based on Google streaming example (word for word almost), you can
// see the old code in the hidden ProgramJustNewAPIProduction.cs.
// The changes are minimal, the code is broken down into intuitive methods and mic recording is never stopping, just
// the Goolge streaming (in/out) is periodically restarted (to comply with Goolge 65 second limit).
// Also the init and close of Google streams are now done properly - which is essential, as we should close the streams
// properly, thus ensuring that any text recognition data is sent back (w/o losses) and not just cut off, killed like before.
// So init/teardown of streams is reworked and that had to be changed as that's what ensures that we don't have any lost
// text when restarting the streaming.
// On the other side, by keeping the mic recording ongoing (we're constantly streaming that - until this process is alive,
// i.e. until the user presses the mic button to turn it off) we ensure that there are no duplicates either (nor losses),
// as all recorded voice bytes are sent exactly once, there're no interruptions or synchronization to worry about.
// It's relatively simple logic but that's actually what's good about it.
// And one more thing, rollover practically moved out of DictateAny to here, except that we're not killing the recording.

namespace LstnMic // GoogleCloudSamples
{
    class Program
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // DRAGAN: we no longer have everything within one call (as we need to periodically restart the streams) so we've
        // moved out some of the variables of importance.

        // DRAGAN: this is the main streaming 'connection' of a sort, which needs to disposed on restart
        //static AsyncDuplexStreamingCall<StreamingRecognizeRequest, StreamingRecognizeResponse> _streamingCall;
        static SpeechClient.StreamingRecognizeStream _streamingCall;

        // DRAGAN: this is the taks that receives the output from the Google (recognized text)
        static Task _printResponses;

        // DRAGAN: some flags to keep track of statuses, time (to do the rollover)
        static int _isStreamingRecognize = 0;
        //static int _isInitSpeechStreams = 0;
        //static int _isCloseSpeechStreams = 0;
        static DateTime _streamingRecognizeStart = DateTime.MinValue;
        //static CancellationTokenSource _cancellationTokenSource;

        // DRAGAN: this inits all about Google streams (and is reusable, should be preceded by close) 
        static async Task<bool> InitSpeechStreams(string language)
        {
            // FIX: DRAGAN: issue with the custom language name being sent to Google in config (and failing)
            language = language.Split(new[] { '_' }, StringSplitOptions.None).First();

            // DRAGAN: this notes down the time of streaming start (to be checked for 45 sec rollover)
            //Interlocked.Increment(ref _isInitSpeechStreams);
            _streamingRecognizeStart = DateTime.Now;
            //_cancellationTokenSource = new CancellationTokenSource();

            try
            {
                //// TESTING:
                //const string accessToken = "ya29.c.Elo-BTPw0nGqsKoJDl39RaGiHFD2RGNyT-MMKilZO9X_0wbf4GUEpK_KfrEZX4aqGrnQX5Kln5GYhIzRRIX62uCME5c1w5fBgooqNHd6jE_I0SXfqtdCml8A7t4";
                //var accessToken = "ya29.c.Elo-BdzK6bMgLfwfUrDiL7hGFFgaK8kcwsuYC3spA3mBTFG1ZaLwf8lHDDJqPnOtJX2MiTfz_8BRLaYdgnOuqOfUWlku8tIZuE2_vwPmAYja-ReSTa9emR37tmA";
                //var accessToken = "ya29.c.Elo-BY5rZTUWHzThhBNPXbjPoOt_MbbRQHhAGeEF2Tj24B2CusIpeZmL6e-tXxcnItmCqXwJErBYAB_idAVuuqDTVAf8WcBiJn1OzcC87PudLSsBWEk3MBnCsuQ";
                //var accessToken = "ya29.c.Elo-BeT8IUqoaF1lda4RHtCx8T5xBHZkI4ytwm_duT7EiqdinmqT4Acfy0SpEDPxNKvNG2lDiYE40Ur_J1tnXm_JAwXswq9C_v4x17RoHppTXODabIboDWOSJjk";
                //var accessToken = "ya29.c.Ko8BxQfxz6ondXWykRKu_hQOiG63MeoT1g9s4oBE6psMW6atP-3x-xsoVpUVv6Nm2dCgEwhmFx2ou4i8m5XwrbweG5WlSdQmT5OkJSqxE1lJ5G4BVedziHdJBVPYNiCsgVnUeRKBK-UJvkL0HhmnrZd3lMVxM2m1EVza2v2t_WoaRSafyYYUeN5K";
                var accessToken = token;
                //Debugger.Launch();

                var credentials = Google.Apis.Auth.OAuth2.GoogleCredential.FromAccessToken(accessToken);
                var channel = new Channel(SpeechClient.DefaultEndpoint.Host, credentials.ToChannelCredentials());

                // DRAGAN: not sure if we should dispose of the client? It seems reused and not disposable (need to debug)
                // seems to be working ok like this
                // # API_SWITCHED_OFF: just comment/uncomment the following 2 lines (switch)
                var speech = SpeechClient.Create(channel);

                // DRAGAN: the 'call' is the real unit of streaming so to speak, that's disposable and what need to restart
                // DRAGAN: changed from GRPClient StreamingRecognize to direct
                _streamingCall = speech.StreamingRecognize();

                // DRAGAN: changed the call from grpclient to direct 
                // Write the initial request with the config.             

                await _streamingCall.WriteAsync(
                    new StreamingRecognizeRequest()
                    {
                        StreamingConfig = new StreamingRecognitionConfig()
                        {
                            Config = new RecognitionConfig()
                            {
                                Encoding =
                                RecognitionConfig.Types.AudioEncoding.Linear16,
                                SampleRateHertz = 16000,
                                LanguageCode = language,
                            },
                            InterimResults = true,
                        }
                    });

                // DRAGAN: this is the task that collects responses, recognized text and sends that to standard output
                // (that's then 'watched' by DictateAny)
                // Print responses.
                _printResponses = Task.Run(async () =>
                {
                    // DRAGAN: CancellationToken for some reason doesn't work and should be 'null', I tried, 
                    // doesn't matter, dispose instead
                    while (await _streamingCall.ResponseStream.MoveNext(default(CancellationToken))) // _cancellationTokenSource.Token)) // 
                    {
                        foreach (var result in _streamingCall.ResponseStream.Current.Results)
                        {
                            if (!result.IsFinal) continue;

                            // DRAGAN: TODO: we might have more alternatives, we're not doing anything about it

                            string text = "";
                            foreach (var alternative in result.Alternatives)
                            {
                                string txt = alternative.Transcript;
                                txt = txt.Replace("\n\n", ";;np;;") + "|";
                                text += txt.Replace("\n", ";;nl;;") + "|";

                                // DRAGAN: this is to test words and timestamps if ever needed, this works nicely, the only problem
                                // was how to integrate that into DictateAny, as we'd need to print then parse on other side and it's ugly
                                // for debugging we can turn this on by uncommenting and then everything is dumped to standard output

                                //var words = $";;Dragan;;words:";
                                //foreach (var word in alternative.Words) //.Aggregate()
                                //{
                                //    var strword = word.Word;
                                //    strword = strword.Replace("\n\n", ";;np;;");
                                //    strword = strword.Replace("\n", ";;nl;;");
                                //    words += $"{{{word.StartTime}, {word.EndTime}, '{strword}'}}";
                                //}
                                //Console.WriteLine(words);

                                //for (int i = 0; i < txt.Length; i++)
                                //bts+=((int)txt[i]).ToString()+" ";
                            }
                            Console.WriteLine(text);
                            Log.Debug($"response: {text}");

                            //Console.WriteLine("Bytes: "+bts);
                            //Console.WriteLine(text + ". (" + alternative.Confidence.ToString() + ")");
                        }
                    }
                });

                Log.Debug($"InitSpeechStreams starting... {DateTime.Now}");

                // DRAGAN: this is the flag telling if we're streaming or not, increment here meaning we're "on"
                Interlocked.Increment(ref _isStreamingRecognize);

                //_streamingRecognizeStart = DateTime.Now; // set on the very start of init, to be safe, not sure when it starts
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"exception: {e.Message}");

                // DRAGAN: reset the time as it failed, we haven't started yet
                _streamingRecognizeStart = DateTime.MinValue;

                return false;
            }
            finally
            {
                //Interlocked.Decrement(ref _isInitSpeechStreams);
            }
        }

        // DRAGAN: this is to close everything about Google streaming

        static async Task<bool> CloseSpeechStreams()
        {
            //Interlocked.Increment(ref _isCloseSpeechStreams);
            try
            {
                // DRAGAN: we're not streaming so get out
                if (_streamingCall == null) return true;

                // DRAGAN: shouldn't happen ?
                if (_printResponses == null) { throw new InvalidOperationException(); } // return true; }
                //_cancellationTokenSource.Cancel(); // true);

                // DRAGAN: properly complete the request, allowing for proper closing of the print task as well
                await _streamingCall.WriteCompleteAsync();

                // DRAGAN: now wait for all responses to stream out from Google (this ensure we have no losses
                // DRAGAN: TODO: this seems to work and at least better than before (or the same) but we need to test this
                // thoroughly if indeed all text is streamed out on restarting - it should but needs proper debugging

                // Print responses.
                await _printResponses;
                _printResponses = null;

                // DRAGAN: now dispose of the 'call' - which is what disposes of everything properly (instead of cancellation)

                _streamingCall = null; // DRAGAN:  to be able to check for it later on

                // DRAGAN: this is to cleanup the 'clients' as well, but there seems to be no need to do this, and might slow down
                // i.e. allow reuse of clients for now
                //await SpeechClient.ShutdownDefaultChannelsAsync();

                // DRAGAN: reset the time 
                _streamingRecognizeStart = DateTime.MinValue;

                // DRAGAN: reset the streaming flag, we're "off" now
                Interlocked.Decrement(ref _isStreamingRecognize);

                Log.Debug($"CloseSpeechStreams done. {DateTime.Now}");
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"exception: {e.Message}");
                return false;
            }
            finally
            {
                //Interlocked.Decrement(ref _isCloseSpeechStreams);
            }
        }

        // DRAGAN: this is the old main methiod, which is basically the main setup and 'loop' (task inside)

        static async Task<object> StreamingMicRecognizeAsync(int seconds, string language)
        {
            // DRAGAN: turn this on to test/debug, this gets debugger right in here, as it's hidden process

            // TESTING:
            Debugger.Launch();

            //try
            //{

            // DRAGAN: place this here so that streams initialize first and are ready when mic starts (and when mic
            // starts we get the ;;OK;; on the DIctateAny side and it goes 'green' before streams are ready. ANd first
            // time around that takes a bit of time
            InitSpeechStreams(language).Wait();

            // DRAGAN: I've removed the locks as those could be dangerous and doesn't work for async scenarios
            // I did try that but it required some more work (to fully remove the .Wait()-s which are possible lock suspects)
            // so I'm keeping it for now, half-way through changing this to proper async code 
            // (that's the leftover from old code, nothing to do with the lateest changes, new branch)

            // DRAGAN: the flags here are for proper close/tracking of mic recording, but isn't really used for now

            // Read from the microphone and stream to API.
            //object writeLock = new object();
            //bool writeMore = true;
            bool isWaveInProcessing = false;
            //int eventsCounter = 0;
            bool doWaveInProcessing = true;

            // DRAGAN: this is the voice recording setup...
            var waveIn = new NAudio.Wave.WaveInEvent()
            {
                DeviceNumber = 0,
                WaveFormat = new NAudio.Wave.WaveFormat(16000, 1)
            };

            Log.Debug($"{nameof(StreamingMicRecognizeAsync)}.thread: {Thread.CurrentThread.ManagedThreadId}");

            // DRAGAN: and this is the recording stream of data (events rather) coming in.
            // DRAGAN: TODO: I'm keeping this as 'sync' (vs async) for now, even though that part was mostly done as it wasn't working
            // properly, sync is easier as it 'stacks up' all input (isWaveProcessing flag is mostly for async scenario) but
            // it uses the .Wait()-s which is not good, but seems to be working ok (in old versions and copied here as is),
            // so I'm keeping it for now to minimize changes but this (.Wait() and sync) should be removed in the future

            var lastLogTime = DateTime.MinValue;
            waveIn.DataAvailable += (object sender, NAudio.Wave.WaveInEventArgs args) =>
            {
                try
                {
                    if (!doWaveInProcessing) return;
                    isWaveInProcessing = true;

                    // DRAGAN: this is where we check if the stream is ripe for restarting by comparing to start time
                    // also we're doing this in a loop to allow for close/init if needed (could be simpler though).
                    // the _isStreamingRecognize should be set inside the Init... and then we get out

                    // there is only one event at the time coming in here (in sync scenario)
                    var start = DateTime.Now;
                    while (!(_isStreamingRecognize > 0) || (_streamingRecognizeStart + TimeSpan.FromSeconds(45) < DateTime.Now))
                    {
                        // DRAGAN: bail out if this takes too long for some reason? in that case the whole recording will fail
                        // and this console exe - and it's probably to happen in case of internet connection being down or so

                        if (start + TimeSpan.FromSeconds(60) < DateTime.Now) return; // throw new ApplicationException();

                        // DRAGAN: restart the streams, first close to properly wait for any text and cleanup, dispose
                        // DRAGAN: TODO: this is dangerous (.Wait()-s), take care of it somehow
                        CloseSpeechStreams().Wait();
                        InitSpeechStreams(language).Wait();
                        //var success = await InitSpeechStreams(language);
                        //if (success) break; // if (!success) return; // throw ...
                    }

                    // DRAGAN: locks removed
                    //try
                    //{
                    //lock (writeLock)
                    //{
                    //    if (!writeMore) return;
                    //await 

                    // DRAGAN: this is where we write the buffers once stream is restarted (and / or ready).
                    // this is the ghist of the 'new branch' is we're never stopping the recording, we just 'pause' (the above)
                    // to restart from time to time, while properly closing (and thus not losing any info), restarting.
                    // So we record and buffer any voice, while only Google streams restart - that allows us to avoid any synchronization
                    // (as in overlapping buffers - to be distinguished from async/sync code above, that's strictly programming terms)

                    _streamingCall.WriteAsync(new StreamingRecognizeRequest()
                    {
                        AudioContent = Google.Protobuf.ByteString
                            .CopyFrom(args.Buffer, 0, args.BytesRecorded)
                    }).Wait();

                    //}
                    //}
                    //finally
                    //{
                    //    isWaveInProcessing = false;
                    //}
                }
                catch (AggregateException e)
                {
                    if (lastLogTime + TimeSpan.FromSeconds(1) < DateTime.Now)
                    {
                        lastLogTime = DateTime.Now;
                        var message = e.Flatten().InnerExceptions.Select(x => x.Message).SafeAggregate();
                        Log.Error($"data in stream error: {e.Message}, {message}");
                    }
                }
                catch (Exception e)
                {
                    if (lastLogTime + TimeSpan.FromSeconds(2) < DateTime.Now)
                    {
                        lastLogTime = DateTime.Now;
                        Log.Error($"data in stream error: {e.Message}");
                    }
                }
                finally
                {
                    isWaveInProcessing = false;
                }
            };

            waveIn.StartRecording();

            Console.WriteLine(";;OK;;");
            Log.Debug($"started: {";;OK;;"}");

            //Console.WriteLine("starting program...");
            if (seconds == -1)
                await Task.Delay(TimeSpan.FromMilliseconds(seconds));
            else
                await Task.Delay(TimeSpan.FromSeconds(seconds));

            // DRAGAN: we start recording, we pass '-1' and basically we never stop, until it's killed
            // DRAGAN: TODO: something to fix for the future, so that's what remains from the old code, and we might lose 
            // some text when the user is clicking the mic off/on - but that shouldn't happen as the user is doing that

            //Console.WriteLine("program stopping...");

            // DRAGAN: it never gets here

            // Stop recording and shut down.
            waveIn.StopRecording();

            doWaveInProcessing = false;
            var startClosing = DateTime.Now;
            while (isWaveInProcessing)
            {
                if (startClosing + TimeSpan.FromSeconds(60) < DateTime.Now) break;
                await Task.Delay(10);
            }

            //lock (writeLock) writeMore = false;
            await _streamingCall.WriteCompleteAsync();
            await _printResponses;

            busy = false;

            return 0;

            //}
            //catch (Exception e)
            //{
            //    Console.WriteLine(";;OK;;");
            //    Console.WriteLine("program force exit...");
            //    return 0;
            //}
        }

        // Some of the 80 supported language codes
        // English US: en-US
        // English UK: en-UK
        // Greek:      el-GR
        // Deutsch:    de-DE
        // French:     fr-FR
        // Spanish:    es-ES

        static bool busy = true;
        static string lang;
        static string token;
        static void Main(string[] args)
        {
            // TESTING:
            //Debugger.Launch();

            if (args.Length != 2) { Log.Error(""); return; }

            token = args[1].Trim().Trim('\r', '\n', ' ');

            // use this to start the log4net (or in AssemblyInfo.cs, this seems safer)
            log4net.Config.XmlConfigurator.Configure();

            Log.Debug($"{ nameof(LstnMic) }: starting...");
            Log.Debug($"token: {token}");

            //GetNewTokenFromGCloud();

            Console.OutputEncoding = System.Text.Encoding.UTF8;

            if (NAudio.Wave.WaveIn.DeviceCount < 1)
            {
                Console.WriteLine("No microphone!");
                Log.Error("No microphone!");

                Environment.Exit(-1);
            }

            Console.CancelKeyPress += delegate
            {
                Console.WriteLine("Clean exit");
                Log.Debug("Clean exit");

                Environment.Exit(0);
            };

            lang = args[0];

            // DRAGAN: this is ok for console
            StreamingMicRecognizeAsync(-1, lang);

            while (busy) System.Threading.Thread.Sleep(10);
        }

        private static void GetNewTokenFromGCloud()
        {
            Process.Start("cmd.exe /c gettoken.bat > token.log").WaitForExit();
            return;

            //Process.Start(@"C:\Windows\System32\cmd.exe", "/c");
            Process p = new Process();
            //ProcessStartInfo info = new ProcessStartInfo();
            var info = new ProcessStartInfo("cmd.exe", @"/C gettoken.bat");

            info.EnvironmentVariables["GOOGLE_APPLICATION_CREDENTIALS"] = "Any-App-Dictation v1-5cc9c575f099.json"; // "PP-Speed-f252b9a4cde9.json";
            //info.FileName = "gettoken.bat"; //.cmd"; // auth application-default print-access-token"; // "cmd.exe";
            //info.RedirectStandardInput = true;
            info.UseShellExecute = false;

            info.StandardOutputEncoding = System.Text.Encoding.UTF8;
            //info.CreateNoWindow = true;
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
            //Thread.Sleep(5000);
            p.WaitForExit();
            Console.WriteLine(sb.ToString());
            return;


            //var gcloudProc = new Process();
            //gcloudProc.StartInfo.FileName = "gcloud auth application-default print-access-token"; // STTmic.exe";
            //gcloudProc.StartInfo.EnvironmentVariables["GOOGLE_APPLICATION_CREDENTIALS"] = "Any-App-Dictation v1-5cc9c575f099.json"; // "PP-Speed-f252b9a4cde9.json";
            //gcloudProc.StartInfo.StandardOutputEncoding = System.Text.Encoding.UTF8;
            //gcloudProc.StartInfo.CreateNoWindow = true;
            //gcloudProc.StartInfo.UseShellExecute = false;
            //gcloudProc.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            //gcloudProc.StartInfo.RedirectStandardOutput = true;
            //gcloudProc.StartInfo.RedirectStandardInput = true;
            ////gcloudProc.OutputDataReceived += new DataReceivedEventHandler(DataRecieved);

            //gcloudProc.Start();
            //gcloudProc.BeginOutputReadLine();

        }
    }
}
