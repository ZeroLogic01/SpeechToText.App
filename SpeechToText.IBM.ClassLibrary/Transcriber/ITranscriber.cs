using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace SpeechToText.ClassLibrary.Transcriber
{

    /// <summary>
    /// Delegate to update the transcription results.
    /// </summary>
    /// <param name="result"></param>
    public delegate void TranscriptionResultHandler(object result);

    public interface ITranscriber
    {
        event TranscriptionResultHandler UpdateTranscribedText;
        event TranscriptionResultHandler StatusUpdater;

        Task ConnectAsync();

        Task SendAsync(byte[] audio);

        Task CloseAsync();

    }
}