using NAudio.Wave;
using Newtonsoft.Json;
using SpeechToText.ClassLibrary.AWS;
using SpeechToText.ClassLibrary.AWS.EventStream;
using SpeechToText.ClassLibrary.Models;
using SpeechToText.ClassLibrary.Models.Amazon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeechToText.ClassLibrary.Transcriber
{
    public class AmazonTranscriber : ITranscriber
    {
        #region Fields

        private readonly CancellationToken _cancellationToken;
        private readonly ClientWebSocket _client;
        private static string _awsRegion;
        private static string _languageCode;
        private static int _sampleRate;
        private readonly WaveFormat _waveFormat;
        private bool _isOpeningRequest = true;

        private readonly Semaphore _sendingSemaphoreObject = new Semaphore(initialCount: 1, maximumCount: 1);
        private readonly Semaphore _closingSemaphoreObject = new Semaphore(initialCount: 1, maximumCount: 1);

        #endregion

        #region Constructor

        public AmazonTranscriber(CancellationToken cancellationToken,
            string languageCode, int sampleRate, WaveFormat waveFormat, string awsRegion = "us-east-2")
        {
            _cancellationToken = cancellationToken;
            _languageCode = languageCode;
            _sampleRate = sampleRate;
            _waveFormat = waveFormat;
            _awsRegion = awsRegion;
            _client = new ClientWebSocket();
        }

        #endregion

        #region Interface members implementation

        public async Task ConnectAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    var presignedUri = new Uri(PresignedUrl.Get(_awsRegion, _languageCode, _sampleRate));
                    await _client.ConnectAsync(presignedUri, _cancellationToken).ConfigureAwait(false);
                    // no need to await HandleResults method
                    HandleResults().ConfigureAwait(false);
                }, _cancellationToken);
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

        }

        /// <summary>
        /// Send data on System.Net.WebSockets.ClientWebSocket as an asynchronous operation.
        /// </summary>
        /// <param name="buffer">The buffer containing the message to be sent.</param>
        /// <param name="messageType">Specifies whether the buffer is clear text or in a binary format.</param>
        /// <param name="endOfMessage">Specifies whether this is the final asynchronous send. Set to true if this is
        /// the final send; false otherwise.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that this operation should
        /// be canceled.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendAsync(byte[] audio)
        {
            await Task.Run(async () =>
            {
                /* To avoid
                * 'There is already one outstanding 'SendAsync' call for this WebSocket instance. 
                * ReceiveAsync and SendAsync can be called simultaneously, but at most one outstanding 
                * operation for each of them is allowed at the same time.' exception.
                */
                _sendingSemaphoreObject.WaitOne();
                {
                    try
                    {
                        if (_client?.State == WebSocketState.Open)
                        {
                            byte[] payload = null;
                            if (!_isOpeningRequest) { payload = StreamingRequest.EncodeAudio(audio); }
                            else
                            {
                                UpdateStatus(new ServiceState { State = "listening" });
                                payload = StreamingRequest.IncludeWaveHeader(audio, _waveFormat);
                                _isOpeningRequest = false;
                            }

                            await _client.SendAsync(new ArraySegment<byte>(payload), WebSocketMessageType.Binary, true, _cancellationToken).ConfigureAwait(false);
                        }

                    }
                    catch (Exception ex)
                    {
                        _isOpeningRequest = true;
                        HandleException(ex);
                    }
                    finally
                    {
                        _sendingSemaphoreObject.Release();
                    }
                };
            });
        }

        /// <summary>
        /// Close the <see cref="_client"/> instance as an asynchronous operation.
        /// </summary>
        /// <param name="closeStatus">The WebSocket close status.</param>
        /// <param name="statusDescription"> A description of the close status.</param>
        /// <param name="cancellationToken">A cancellation token used to propagate notification that this operation should
        /// be canceled.</param>
        /// <returns>Returns System.Threading.Tasks.Task.The task object representing the asynchronous
        /// operation.</returns>
        public async Task CloseAsync()
        {
            _closingSemaphoreObject.WaitOne();
            {
                try
                {
                    if (_client?.State == WebSocketState.Open)
                    {
                        //To end the audio data stream, send an empty audio chunk in an event-stream-encoded message.
                        var eventStreamEncodedMessage = StreamingRequest.EncodeAudio(new byte[0]);
                        await _client.SendAsync(new ArraySegment<byte>(eventStreamEncodedMessage), WebSocketMessageType.Binary, true, CancellationToken.None);
                        await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None);
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
                finally
                {
                    _isOpeningRequest = true;
                    _closingSemaphoreObject.Release();
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the exception &amp; if it's not due to the <see cref="_cancellationToken"/> being requested, 
        /// constructs the exception message &amp; calls the <see cref="UpdateStatus(object)"/>
        /// </summary>
        /// <param name="exception"></param>
        private void HandleException(Exception exception)
        {
            if (!_cancellationToken.IsCancellationRequested)
            {
                var innerException = exception.InnerException != null ? $"{exception.InnerException.Message}." : string.Empty;
                UpdateStatus(new ServiceState { Error = $"{exception.Message}. {innerException}" });
            }
        }

        /// <summary>
        /// Handle results received.
        /// </summary>
        /// <returns></returns>
        private async Task HandleResults()
        {
            try
            {
                // buffer size is 4 KB
                var buffer = new byte[4 * 1024];
                while (true)
                {
                    var segment = new ArraySegment<byte>(buffer);
                    var result = await _client.ReceiveAsync(segment, _cancellationToken);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        return;
                    }

                    int count = result.Count;

                    while (!result.EndOfMessage)
                    {
                        if (count >= buffer.Length)
                        {
                            await _client.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Received message length is too long", _cancellationToken);
                            return;
                        }

                        segment = new ArraySegment<byte>(buffer, count, buffer.Length - count);
                        result = await _client.ReceiveAsync(segment, _cancellationToken);
                        count += result.Count;
                    }

                    (List<Header> headers, byte[] message) = StreamingReponse.Decode(buffer.Take(count).ToArray());


                    var exception = headers.FirstOrDefault(header => header.HeaderName
                        .Equals(":exception-type", StringComparison.OrdinalIgnoreCase));
                    if (exception != null)
                    {
                        var errorMessage = (ServiceState)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(message), typeof(ServiceState));
                        UpdateStatus(errorMessage);
                    }
                    else
                    {
                        var transcriptionEvent = (TranscriptEvent)JsonConvert.DeserializeObject(Encoding.UTF8.GetString(message), typeof(TranscriptEvent));
                        if (transcriptionEvent.Transcript.Results != null && transcriptionEvent.Transcript.Results.Count > 0)
                        {
                            Console.WriteLine(Encoding.UTF8.GetString(message));
                            UpdateTranscription(transcriptionEvent.Transcript);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        #endregion

        #region Delegates & events

        public event TranscriptionResultHandler UpdateTranscribedText;
        public event TranscriptionResultHandler StatusUpdater;

        public void UpdateTranscription(object transcript)
        {
            UpdateTranscribedText?.Invoke(transcript);
        }

        public void UpdateStatus(object result)
        {
            StatusUpdater?.Invoke(result);
        }

        #endregion
    }
}
