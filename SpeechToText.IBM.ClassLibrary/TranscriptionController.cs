using NAudio.Wave;
using SpeechToText.ClassLibrary.Enums;
using SpeechToText.ClassLibrary.Models.IBM;
using SpeechToText.ClassLibrary.Transcriber;
using System;
using System.Threading;

namespace SpeechToText.ClassLibrary
{
    public class TranscriptionController
    {
        #region Fields

        /// <summary>
        /// Our recorder object.
        /// </summary>
        private WaveIn waveIn;

        /// <summary>
        /// Format of the audio in which <see cref="waveIn"/> records audio.
        /// </summary>
        private readonly WaveFormat _format;

        /// <summary>
        /// Describes which service to use.
        /// </summary>
        private readonly SpeechToTextServiceEnum _speechToTextServiceEnum;

        /// <summary>
        /// To encode the raw pcm into opus.
        /// </summary>
        private PCMToOpusEncoder _pcmToOpusEncoder;

        private readonly ITranscriber _transcriber;
        private readonly CancellationToken _cancellationToken;


        #endregion

        #region Constructor

        /// <summary>
        /// Class constructor which initializes the PCM format with the specified sample rate, bit depth and channels
        /// </summary>
        /// <param name="speechToTextServiceEnum">Indicating which API to use for transcription</param>
        /// <param name="rate">The sample rate. It's default value will be 16000 if non provided</param>
        /// <param name="bits">The bit depth. It's default value will be 16 if non provided</param>
        /// <param name="channels">Number of channels. It's default value will be 1 if non provided</param>
        public TranscriptionController(SpeechToTextServiceEnum speechToTextServiceEnum, CancellationToken cancellationToken,
            int rate = 16000, int bits = 16, int channels = 1)
        {
            _speechToTextServiceEnum = speechToTextServiceEnum;
            _cancellationToken = cancellationToken;

            if (_speechToTextServiceEnum == SpeechToTextServiceEnum.IBMWatson)
            {
                // Replace this api key with yours
                //    string apiKey = @"eZdz_UIeEb_eOwUQ37H3Uaqge4cLcew5-WuFqllqzXEv";
                //CLLxZNrGTrYAHJDAqzzilJmtwPewJm5W7eV5F0EDqbqn
                string apiKey = @"eZdz_UIeEb_eOwUQ37H3Uaqge4cLcew5-WuFqllqzXEv";


                IBMRequestHeader header = new IBMRequestHeader
                {
                    Action = "start",
                    //ContentType = $"audio/l16;rate={rate}",
                    ContentType = $"audio/ogg; codecs=opus",
                    InterimResults = true,
                    SmartFormatting = true,
                    InactivityTimeout = -1 /* -1: to disable service timeout*/
                };


                _transcriber = new IBMTranscriber(apiKey, cancellationToken: _cancellationToken, openingRequestHeader: header);
                _pcmToOpusEncoder = new PCMToOpusEncoder(rate, bitRate: bits, channels);
                _format = new WaveFormat(rate, bits, channels);
            }
            else
            {
                // Supported language codes are en-US, en-GB, fr-FR, fr-CA, es-US.
                string languageCode = "en-US";
                /*
                * Currently, US English (en-US) and US Spanish (es-US) support sample rates up to 16000 Hz. 
                * Other languages support rates up to 8000 Hz.
                */
                if (languageCode.Equals("en-US") || languageCode.Equals("es-US"))
                {
                    if (rate > 16000)
                    {
                        rate = 16000;
                    }
                }
                else
                {
                    rate = 8000;
                }
                _format = new WaveFormat(rate, bits, channels);
                _transcriber = new AmazonTranscriber(_cancellationToken, languageCode, rate, _format);
            }
            _transcriber.UpdateTranscribedText += UpdateTranscription;
            _transcriber.StatusUpdater += UpdateStatus;
        }

        #endregion

        #region WaveIn Methods

        /// <summary>
        /// Starts the recording. Further registers the callback events to handle the audio data & do the transcription.
        /// </summary>
        /// <param name="deviceNumber">The device number to use. Default is 0 for Microphone.</param>
        public async void StartRecording(int deviceNumber = 0)
        {
            waveIn = new WaveIn
            {
                BufferMilliseconds = 50,
                DeviceNumber = deviceNumber,
                WaveFormat = _format
            };


            waveIn.DataAvailable += new EventHandler<WaveInEventArgs>(WaveIn_DataAvailable);
            waveIn.RecordingStopped += new EventHandler<StoppedEventArgs>(WaveIn_RecordingStopped);

            waveIn.StartRecording();

            await _transcriber.ConnectAsync();

        }

        /// <summary>
        /// Stops the recorder.
        /// </summary>
        public async void StopRecording()
        {
            waveIn?.StopRecording();
            _pcmToOpusEncoder?.Fininsh();
            await _transcriber.CloseAsync();
        }

        #endregion

        #region WaveIn Event handlers

        /// <summary>
        /// Invoked when recording gets started.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (_speechToTextServiceEnum == SpeechToTextServiceEnum.IBMWatson)
            {
                var encodedAudio = _pcmToOpusEncoder.EncodePCM(e.Buffer);

                Console.WriteLine(encodedAudio.Length + "  " + e.BytesRecorded);
                if (encodedAudio != null)
                {
                    await _transcriber.SendAsync(encodedAudio);
                }
            }
            else
            {
                await _transcriber.SendAsync(e.Buffer).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Gets invoked when <see cref="waveIn"/> stops recording.
        /// </summary>
        /// <param name="sender">The invoker.</param>
        /// <param name="e"></param>
        private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            /*If the waveIn is already not null, dispose the object & set it to null*/
            waveIn?.Dispose();
            waveIn = null;
            _pcmToOpusEncoder = null;
        }

        #endregion

        #region Delegates & events

        /// <summary>
        /// Delegate to update the transcription results.
        /// </summary>
        /// <param name="result"></param>
        public delegate void TranscriptionResultHandler(object result);
        public event TranscriptionResultHandler TranscribedTextUpdater;
        public event TranscriptionResultHandler StatusUpdater;


        public void UpdateTranscription(object result)
        {
            TranscribedTextUpdater?.Invoke(result);
        }

        public void UpdateStatus(object result)
        {
            StatusUpdater?.Invoke(result);
        }

        #endregion

    }
}
