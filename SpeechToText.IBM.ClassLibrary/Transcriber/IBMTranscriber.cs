using Newtonsoft.Json;
using SpeechToText.ClassLibrary.Models;
using SpeechToText.ClassLibrary.Models.IBM;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SpeechToText.ClassLibrary.Transcriber
{
    class IBMTranscriber : ITranscriber
    {

        #region Fields

        private readonly Uri _connection = new Uri($"wss://gateway-wdc.watsonplatform.net/speech-to-text/api/v1/recognize");

        private readonly CancellationToken _cancellationToken;
        private readonly Semaphore _sendingSemaphoreObject = new Semaphore(initialCount: 1, maximumCount: 1);
        private readonly Semaphore _closingSemaphoreObject = new Semaphore(initialCount: 1, maximumCount: 1);

        private readonly ClientWebSocket _client;
        private readonly string _apKey;

        private readonly ArraySegment<byte> _openingMessage;
        private static readonly ArraySegment<byte> _closingMessage = new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert
                    .SerializeObject(new IBMRequestHeader
                    {
                        Action = "close"
                    })));

        #endregion


        #region Constructor

        public IBMTranscriber(string apiKey, CancellationToken cancellationToken,
            IBMRequestHeader openingRequestHeader)
        {
            _apKey = apiKey;
            _cancellationToken = cancellationToken;

            _openingMessage = new ArraySegment<byte>(Encoding.UTF8
                     .GetBytes(JsonConvert.SerializeObject(openingRequestHeader, Formatting.Indented)));
            _client = new ClientWebSocket();
        }

        #endregion


        #region Interface members implementation

        /// <summary>
        /// Connects to the IBM Speech to text api using the api key & sends the opening message.
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAsync()
        {
            try
            {
                await Task.Run(async () =>
                {
                    // get the token data using the api key
                    var tokenDataTask = GetIamDataWithCancellationTokenAsync(_apKey, _cancellationToken);
                    var tokenData = await tokenDataTask;

                    _client.Options.Proxy = null;
                    _client.Options.SetRequestHeader("Authorization", $"Bearer {tokenData.AccessToken}");

                    await _client.ConnectAsync(_connection, _cancellationToken).ConfigureAwait(false);
                    await SendAsync(_openingMessage, WebSocketMessageType.Text, true, _cancellationToken).ConfigureAwait(false);
                    // no need to await HandleResults method
                    HandleResults().ConfigureAwait(false);

                });
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        /// <summary>
        /// Sends the audio stream asynchronously to the API. API expects opening message first before any binary data.
        /// So ensure that opening header message is already sent before calling this method.
        /// </summary>
        /// <param name="audio">Audio bytes.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task SendAsync(byte[] audio)
        {
            await Task.Run(async () =>
            {
                await SendAsync(new ArraySegment<byte>(audio), WebSocketMessageType.Binary, true, _cancellationToken);
            });
        }

        /// <summary>
        /// Asynchronously closes the WebSocket Client connection.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task CloseAsync()
        {
            try
            {
                if (_client?.State == WebSocketState.Open)
                {
                    await _client.SendAsync(_closingMessage, WebSocketMessageType.Text, true, _cancellationToken);
                    await CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", _cancellationToken);
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        #endregion


        #region Methods

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
        private async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            /*
            * Allow only one thread to access the block of code to avoid 
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
                        await _client.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
                finally
                {
                    _sendingSemaphoreObject.Release();
                }
            }

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
        private async Task CloseAsync(WebSocketCloseStatus closeStatus, string statusDescription, CancellationToken cancellationToken)
        {
            _closingSemaphoreObject.WaitOne();
            {
                try
                {
                    if (_client?.State == WebSocketState.Open)
                    {
                        await _client.CloseAsync(closeStatus, statusDescription, cancellationToken);
                    }
                }
                catch (Exception ex)
                {
                    HandleException(ex);
                }
                finally
                {
                    _closingSemaphoreObject.Release();
                }
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
                var buffer = new byte[1024];
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
                            await CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Received message length is too long", _cancellationToken);
                            return;
                        }

                        segment = new ArraySegment<byte>(buffer, count, buffer.Length - count);
                        result = await _client.ReceiveAsync(segment, _cancellationToken);
                        count += result.Count;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, count);
                    Console.WriteLine(message);

                    // de-serialize the response message
                    var desObject = DeserializeResponseMessage(message);
                    if (desObject is SpeechRecognitionResults speechRecognitionResults)
                    {
                        UpdateTranscription(speechRecognitionResults);
                    }
                    else if (desObject is ServiceState serviceState)
                    {
                        UpdateStatus(serviceState);
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

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
        /// De-serialize the response message into relevant objects.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private object DeserializeResponseMessage(string message)
        {
            var result = (SpeechRecognitionResults)JsonConvert.DeserializeObject(message, typeof(SpeechRecognitionResults));

            if (result.Results == null)
            {
                return (ServiceState)JsonConvert.DeserializeObject(message, typeof(ServiceState));
            }

            return result;
        }

        /// <summary>
        /// If the <see cref="CancellationToken.IsCancellationRequested"/> this method will return immediately.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        private static async Task<IamTokenData> GetIamDataWithCancellationTokenAsync(string apiKey,
            CancellationToken cancellationToken)
        {
            // We create a TaskCompletionSource of IamTokenData
            var taskCompletionSource = new TaskCompletionSource<IamTokenData>();

            // Registering a lambda into the cancellationToken
            cancellationToken.Register(() =>
            {
                // We received a cancellation message, cancel the TaskCompletionSource.Task
                taskCompletionSource.TrySetCanceled();
            });

            var task = IamTokenData.GetIamToken(apiKey);

            // Wait for the first task to finish among the two
            var completedTask = await Task.WhenAny(task, taskCompletionSource.Task);

            return await completedTask;
        }

        #endregion


        #region Delegates & events


        public event TranscriptionResultHandler UpdateTranscribedText;
        public event TranscriptionResultHandler StatusUpdater;

        public void UpdateTranscription(object result)
        {
            if (result is SpeechRecognitionResults recognitionResults)
            {
                UpdateTranscribedText?.Invoke(recognitionResults);
            }
        }

        public void UpdateStatus(object result)
        {
            if (result is ServiceState serviceState)
            {
                StatusUpdater?.Invoke(serviceState);
            }
        }

        #endregion
    }
}
