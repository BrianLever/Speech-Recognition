//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using System.Diagnostics;
//using System.IO;

//using CommandLine;
//using Google.Cloud.Speech.V1; //Beta1;
//using System.Linq;

//namespace LstnMic // GoogleCloudSamples
//{
//    class ProgramNew
//    {
//        static async Task<object> StreamingMicRecognizeAsync(int seconds, string language)
//        {
//            //Debugger.Launch();

//            //try
//            //{
//            var speech = SpeechClient.Create();
//            var streamingCall = speech.GrpcClient.StreamingRecognize();

//            // Write the initial request with the config.
//            await streamingCall.RequestStream.WriteAsync(new StreamingRecognizeRequest()
//            {
//                StreamingConfig = new StreamingRecognitionConfig()
//                {
//                    Config = new RecognitionConfig()
//                    {
//                        Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
//                        SampleRateHertz = 16000,
//                        //SampleRate = 16000,
//                        //LanguageCode = "en-US",
//                        LanguageCode = language,
//                        EnableWordTimeOffsets = true,
//                    },
//                    InterimResults = true,
//                }
//            });

//            // Print responses.
//            Task printResponses = Task.Run(async () =>
//            {
//                ////var headers = await streamingCall.ResponseHeadersAsync;
//                //foreach (var entry in streamingCall.GetTrailers())
//                //{
//                //    //Console.WriteLine($";;Dragan;;header:{entry.Key}, {entry.Value}");
//                //}

//                while (await streamingCall.ResponseStream.MoveNext(default(CancellationToken)))
//                {
//                    foreach (var result in streamingCall.ResponseStream.Current.Results)
//                    {
//                        if (!result.IsFinal) continue;

//                        //int n = result.Alternatives.Count;
//                        //Console.WriteLine(result.Alternatives[0].Transcript.Trim());

//                        string text = "";
//                        //string bts = "";
//                        Console.WriteLine($";;Dragan;;alternatives: {result.Alternatives.Count}");
//                        foreach (var alternative in result.Alternatives)
//                        {
//                            string txt = alternative.Transcript;
//                            txt = txt.Replace("\n\n", ";;np;;") + "|";
//                            text += txt.Replace("\n", ";;nl;;") + "|";

//                            var words = $";;Dragan;;words:";
//                            foreach (var word in alternative.Words) //.Aggregate()
//                            {
//                                var strword = word.Word;
//                                strword = strword.Replace("\n\n", ";;np;;");
//                                strword = strword.Replace("\n", ";;nl;;");
//                                words += $"{{{word.StartTime}, {word.EndTime}, '{strword}'}}";
//                            }
//                            Console.WriteLine(words);

//                            //for (int i = 0; i < txt.Length; i++)
//                            //bts+=((int)txt[i]).ToString()+" ";
//                        }
//                        Console.WriteLine(text);
//                        //Console.WriteLine("Bytes: "+bts);
//                        //Console.WriteLine(text + ". (" + alternative.Confidence.ToString() + ")");
//                    }
//                }
//            });

//            // Read from the microphone and stream to API.
//            object writeLock = new object();
//            bool writeMore = true;
//            var waveIn = new NAudio.Wave.WaveInEvent();
//            waveIn.DeviceNumber = 0;
//            waveIn.WaveFormat = new NAudio.Wave.WaveFormat(16000, 1);
//            waveIn.DataAvailable += (object sender, NAudio.Wave.WaveInEventArgs args) =>
//            {
//                lock (writeLock)
//                {
//                    if (!writeMore) return;
//                    streamingCall.RequestStream.WriteAsync(new StreamingRecognizeRequest()
//                    {
//                        AudioContent = Google.Protobuf.ByteString
//                            .CopyFrom(args.Buffer, 0, args.BytesRecorded)
//                    }).Wait();
//                }
//            };
//            waveIn.StartRecording();
//            Console.WriteLine(";;OK;;");
//            //Console.WriteLine("starting program...");
//            await Task.Delay(TimeSpan.FromSeconds(seconds));

//            //Console.WriteLine("program stopping...");

//            // Stop recording and shut down.
//            waveIn.StopRecording();
//            lock (writeLock) writeMore = false;
//            await streamingCall.RequestStream.CompleteAsync();
//            await printResponses;

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
//        static void MainNew(string[] args)
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
