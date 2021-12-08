//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Diagnostics;
//using System.IO;

//using CommandLine;
//using Google.Cloud.Speech.V1; //Beta1;
//using System.Linq;
//using Grpc.Core;

//namespace LstnMic // GoogleCloudSamples
//{
//    class ProgramAsync
//    {
//        static AsyncDuplexStreamingCall<StreamingRecognizeRequest, StreamingRecognizeResponse> _streamingCall;
//        static Task _printResponses;
//        static int _isStreamingRecognize = 0;
//        static int _isInitSpeechStreams = 0;
//        static int _isCloseSpeechStreams = 0;
//        static DateTime _streamingRecognizeStart = DateTime.MinValue;
//        //static CancellationTokenSource _cancellationTokenSource;

//        static async Task<bool> InitSpeechStreams(string language)
//        {
//            Interlocked.Increment(ref _isInitSpeechStreams);
//            _streamingRecognizeStart = DateTime.Now;
//            //_cancellationTokenSource = new CancellationTokenSource();
//            try
//            {
//                var speech = SpeechClient.Create();
//                //var speech = await SpeechClient.CreateAsync();
//                _streamingCall = speech.GrpcClient.StreamingRecognize(); // null, null, _cancellationTokenSource.Token);

//                // Write the initial request with the config.
//                await _streamingCall.RequestStream.WriteAsync(new StreamingRecognizeRequest()
//                {
//                    StreamingConfig = new StreamingRecognitionConfig()
//                    {
//                        Config = new RecognitionConfig()
//                        {
//                            Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
//                            SampleRateHertz = 16000,
//                            //SampleRate = 16000,
//                            //LanguageCode = "en-US",
//                            LanguageCode = language,
//                            EnableWordTimeOffsets = true,
//                        },
//                        InterimResults = true,
//                    }
//                });

//                // Print responses.
//                _printResponses = Task.Run(async () =>
//                {
//                    ////var headers = await streamingCall.ResponseHeadersAsync;
//                    //foreach (var entry in streamingCall.GetTrailers())
//                    //{
//                    //    //Console.WriteLine($";;Dragan;;header:{entry.Key}, {entry.Value}");
//                    //}

//                    while (await _streamingCall.ResponseStream.MoveNext(default(CancellationToken))) // _cancellationTokenSource.Token)) // 
//                    {
//                        foreach (var result in _streamingCall.ResponseStream.Current.Results)
//                        {
//                            if (!result.IsFinal) continue;

//                            //int n = result.Alternatives.Count;
//                            //Console.WriteLine(result.Alternatives[0].Transcript.Trim());

//                            string text = "";
//                            //string bts = "";
//                            Console.WriteLine($";;Dragan;;alternatives: {result.Alternatives.Count}");
//                            foreach (var alternative in result.Alternatives)
//                            {
//                                string txt = alternative.Transcript;
//                                txt = txt.Replace("\n\n", ";;np;;") + "|";
//                                text += txt.Replace("\n", ";;nl;;") + "|";

//                                var words = $";;Dragan;;words:";
//                                foreach (var word in alternative.Words) //.Aggregate()
//                                {
//                                    var strword = word.Word;
//                                    strword = strword.Replace("\n\n", ";;np;;");
//                                    strword = strword.Replace("\n", ";;nl;;");
//                                    words += $"{{{word.StartTime}, {word.EndTime}, '{strword}'}}";
//                                }
//                                Console.WriteLine(words);

//                                //for (int i = 0; i < txt.Length; i++)
//                                //bts+=((int)txt[i]).ToString()+" ";
//                            }
//                            Console.WriteLine(text);
//                            //Console.WriteLine("Bytes: "+bts);
//                            //Console.WriteLine(text + ". (" + alternative.Confidence.ToString() + ")");
//                        }
//                    }
//                });

//                Interlocked.Increment(ref _isStreamingRecognize);
//                //_streamingRecognizeStart = DateTime.Now; // set on the very start of init, to be safe, not sure when it starts
//                return true;
//            }
//            catch (Exception e)
//            {
//                Debug.WriteLine($"exception: {e.Message}");
//                _streamingRecognizeStart = DateTime.MinValue;
//                return false;
//            }
//            finally
//            {
//                Interlocked.Decrement(ref _isInitSpeechStreams);
//            }
//        }

//        static async Task<bool> CloseSpeechStreams()
//        {
//            Interlocked.Increment(ref _isCloseSpeechStreams);
//            try
//            {
//                if (_streamingCall == null) return true;
//                if (_printResponses == null) { throw new InvalidOperationException(); } // return true; }
//                //_cancellationTokenSource.Cancel(); // true);

//                await _streamingCall.RequestStream.CompleteAsync();
//                //await _streamingCall.WriteCompleteAsync();
//                // Print responses.
//                await _printResponses;
//                _printResponses = null;

//                var streamingCall = _streamingCall;
//                _streamingCall = null;
//                streamingCall.Dispose(); // cancelling, instead of cancellationToken

//                //await SpeechClient.ShutdownDefaultChannelsAsync();

//                _streamingRecognizeStart = DateTime.MinValue;
//                Interlocked.Decrement(ref _isStreamingRecognize);

//                return true;
//            }
//            catch (Exception e)
//            {
//                Debug.WriteLine($"exception: {e.Message}");
//                return false;
//            }
//            finally
//            {
//                Interlocked.Decrement(ref _isCloseSpeechStreams);
//            }
//        }

//        static async Task<object> StreamingMicRecognizeAsync(int seconds, string language)
//        {
//            Debugger.Launch();

//            //try
//            //{

//            // Read from the microphone and stream to API.
//            //object writeLock = new object();
//            //bool writeMore = true;
//            //bool isWaveInProcessing = false;
//            int eventsCounter = 0;
//            bool doWaveInProcessing = true;
//            var waveIn = new NAudio.Wave.WaveInEvent()
//            {
//                DeviceNumber = 0,
//                WaveFormat = new NAudio.Wave.WaveFormat(16000, 1)
//            };
//            Debug.WriteLine($"{nameof(StreamingMicRecognizeAsync)}.thread: {Thread.CurrentThread.ManagedThreadId}");
//            waveIn.DataAvailable += async (object sender, NAudio.Wave.WaveInEventArgs args) =>
//            {
//                try
//                {
//                    var start = DateTime.Now;
//                    Debug.WriteLine($"DataAvailable.thread: {Thread.CurrentThread.ManagedThreadId}");
//                    // this is to sync all DataAvailable, all will be in the same (main?) thread, so flag is enough 
//                    // (asyncs could overlap only after await calls when the processing is yielded to another async/await)
//                    while (eventsCounter > 0) //1) //isWaveInProcessing)
//                    {
//                        if (start + TimeSpan.FromSeconds(60) < DateTime.Now) return; // throw new ApplicationException();
//                        await Task.Delay(10);
//                    }
//                    if (!doWaveInProcessing) return;
//                    Interlocked.Increment(ref eventsCounter);

//                    // there is only one event at the time coming in here 
//                    start = DateTime.Now;
//                    while (!(_isStreamingRecognize > 0) || (_streamingRecognizeStart + TimeSpan.FromSeconds(45) < DateTime.Now))
//                    {
//                        if (_isInitSpeechStreams > 0) { throw new InvalidOperationException(); } // shouldn't happen
//                        if (start + TimeSpan.FromSeconds(60) < DateTime.Now) return; // throw new ApplicationException();
//                        await CloseSpeechStreams();
//                        var success = await InitSpeechStreams(language);
//                        if (success) break; // if (!success) return; // throw ...
//                    }

//                    //isWaveInProcessing = true;
//                    //try
//                    //{
//                    //lock (writeLock)
//                    //{
//                    //    if (!writeMore) return;
//                    //await 
//                    _streamingCall.RequestStream.WriteAsync(new StreamingRecognizeRequest()
//                    {
//                        AudioContent = Google.Protobuf.ByteString
//                            .CopyFrom(args.Buffer, 0, args.BytesRecorded)
//                    }).Wait();
//                    //}
//                    //}
//                    //finally
//                    //{
//                    //    isWaveInProcessing = false;
//                    //}
//                }
//                finally
//                {
//                    Interlocked.Decrement(ref eventsCounter);
//                }
//            };
//            waveIn.StartRecording();
//            Console.WriteLine(";;OK;;");
//            //Console.WriteLine("starting program...");
//            await Task.Delay(TimeSpan.FromSeconds(seconds));

//            //Console.WriteLine("program stopping...");

//            // Stop recording and shut down.
//            waveIn.StopRecording();

//            doWaveInProcessing = false;
//            var startClosing = DateTime.Now;
//            while (eventsCounter > 0)
//            {
//                if (startClosing + TimeSpan.FromSeconds(60) < DateTime.Now) break;
//                await Task.Delay(10);
//            }

//            //lock (writeLock) writeMore = false;
//            await _streamingCall.RequestStream.CompleteAsync();
//            await _printResponses;

//            busy = false;

//            return 0;

//            //}
//            //catch (Exception e)
//            //{
//            //    Console.WriteLine(";;OK;;");
//            //    Console.WriteLine("program force exit...");
//            //    return 0;
//            //}
//        }

//        // Some of the 80 supported language codes
//        // English US: en-US
//        // English UK: en-UK
//        // Greek:      el-GR
//        // Deutsch:    de-DE
//        // French:     fr-FR
//        // Spanish:    es-ES

//        static bool busy = true;
//        static string lang;
//        static void MainAsync(string[] args)
//        {
//            if (args.Length != 1) return;

//            Console.OutputEncoding = System.Text.Encoding.UTF8;

//            if (NAudio.Wave.WaveIn.DeviceCount < 1)
//            {
//                Console.WriteLine("No microphone!");
//                Environment.Exit(-1);
//            }

//            Console.CancelKeyPress += delegate
//            {
//                Console.WriteLine("Clean exit");
//                Environment.Exit(0);
//            };

//            lang = args[0];
//            StreamingMicRecognizeAsync(-1, lang);

//            while (busy) System.Threading.Thread.Sleep(10);
//        }
//    }
//}
