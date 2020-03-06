using IBM.Watson.LanguageTranslator.v3.Model;
using LanguageTranslator;
using MahApps.Metro.Controls.Dialogs;
using Prism.Commands;
using Prism.Mvvm;
using SpeechToText.ClassLibrary;
using SpeechToText.ClassLibrary.Enums;
using SpeechToText.ClassLibrary.Models;
using SpeechToText.ClassLibrary.Models.Amazon;
using SpeechToText.ClassLibrary.Models.IBM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SpeechToText.UI.ViewModels
{
    public class MainViewModel : BindableBase
    {
        #region Private Fields

        /// <summary>
        /// The transcription controller.
        /// </summary>
        private TranscriptionController _transcriptionController;

        /// <summary>
        /// Cancellation Token source.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource = null;

        /// <summary>
        /// Dialog coordinator to show dialog messages.
        /// </summary>
        private readonly IDialogCoordinator _dialogCoordinator = DialogCoordinator.Instance;

        #endregion

        #region UI Properties

        #region View settings

        /// <summary>
        /// The windows title.
        /// </summary>
        public string Title { get; set; } = "Speech To Text Transcriber";


        /// <summary>
        /// The default Height.
        /// </summary>
        public double Height { get; set; } = 450;

        /// <summary>
        /// Minimum height allowed.
        /// </summary>
        public double MinHeight { get; set; } = 350;

        /// <summary>
        /// The default width.
        /// </summary>
        public double Width { get; set; } = 500;

        /// <summary>
        /// View's width can decreased maximum to this value.
        /// </summary>
        public double MinWidth { get; set; } = 340;

        #endregion

        #region Toggle Control

        /// <summary>
        /// This label text will be visible when toggle is off.
        /// </summary>
        public string OffLabel { get; set; } = "IBM Watson";

        /// <summary>
        /// This label text will be visible when toggle is On.
        /// </summary>
        public string OnLabel { get; set; } = "Amazon Transcribe";

        #endregion

        #region Buttons

        /// <summary>
        /// Start button label text.
        /// </summary>
        public string StartTranscriptionBtnLabel { get; set; } = "Start Transcription";

        /// <summary>
        /// Stop button label text.
        /// </summary>
        public string StopTranscriptionBtnLabel { get; set; } = "Stop Transcription";


        /// <summary>
        /// Start button label text.
        /// </summary>
        public string StartTranslationBtnLabel { get; set; } = "Translate";

        /// <summary>
        /// Stop button label text.
        /// </summary>
        public string StopTranslationBtnLabel { get; set; } = "Stop";

        #endregion

        #endregion

        #region Properties

        #region Transcription Properties

        public IBMTranslator Translator { get; set; }

        private bool _isToggleEnabled = true;

        /// <summary>
        /// Boolean value indicating whether toggle is enabled or not. 
        /// Toggle will be disabled when transcription process starts & will be visible again when stopped.
        /// </summary>
        public bool IsToggleEnabled
        {
            get => _isToggleEnabled;
            set => SetProperty(ref _isToggleEnabled, value);
        }

        private string _transcribedText = string.Empty;

        /// <summary>
        /// The transcribed text received from the api. 
        /// </summary>
        public string TranscribedText
        {
            get => _transcribedText;
            set
            {
                SetProperty(ref _transcribedText, value);
                IsStartTranslationBtnEnabled = CanStartTranslationManually();
            }
        }

        private SpeechToTextServiceEnum _speechToTextService = SpeechToTextServiceEnum.Amazon;

        /// <summary>
        /// Describes which API to use for transcription.
        /// </summary>
        public SpeechToTextServiceEnum SpeechToTextService
        {
            get { return _speechToTextService; }
            set => SetProperty(ref _speechToTextService, value);
        }

        #endregion

        #region Transcription Button Properties

        private bool _isStartTranscriptionBtnEnabled = true;

        /// <summary>
        /// Boolean value indicating whether the start transcription button is enabled or not.
        /// </summary>
        public bool IsStartTranscriptionBtnEnabled
        {
            get => _isStartTranscriptionBtnEnabled;
            set => SetProperty(ref _isStartTranscriptionBtnEnabled, value);
        }

        private bool _isStopTranscriptionBtnEnabled = false;

        /// <summary>
        /// Boolean value to control the Stop transcription button.
        /// </summary>
        public bool IsStopTranscriptionBtnEnabled
        {
            get => _isStopTranscriptionBtnEnabled;
            set => SetProperty(ref _isStopTranscriptionBtnEnabled, value);
        }

        #endregion

        #region Translation Button Properties

        private bool _isStartTranslationBtnEnabled = false;

        /// <summary>
        /// Boolean value indicating whether the start translation button is enabled or not.
        /// </summary>
        public bool IsStartTranslationBtnEnabled
        {
            get => _isStartTranslationBtnEnabled;
            set => SetProperty(ref _isStartTranslationBtnEnabled, value);
        }

        #endregion

        #region Translation Properties


        private bool _isAutoTranslateCheckBoxEnabled = false;

        /// <summary>
        /// Boolean value indicating whether the Auto Translate CheckBox is checked or not.
        /// </summary>
        public bool IsAutoTranslateCheckBoxEnabled
        {
            get { return _isAutoTranslateCheckBoxEnabled; }
            set
            {
                SetProperty(ref _isAutoTranslateCheckBoxEnabled, value);
            }
        }


        private bool _isAutoTranslateCheckBoxChecked = true;

        public bool IsAutoTranslateCheckBoxChecked
        {
            get { return _isAutoTranslateCheckBoxChecked; }
            set
            {
                SetProperty(ref _isAutoTranslateCheckBoxChecked, value);
                IsStartTranslationBtnEnabled = CanStartTranslationManually();
            }
        }



        private string _translatedText = string.Empty;

        /// <summary>
        /// The translated text. 
        /// </summary>
        public string TranslatedText
        {
            get => _translatedText;
            set
            {
                SetProperty(ref _translatedText, value);
            }
        }

        private IdentifiableLanguages _sourceLanguages;

        /// <summary>
        /// Source Translation Languages supported by the IBM Speech Translation.
        /// </summary>
        public IdentifiableLanguages SourceLanguages
        {
            get => _sourceLanguages;
            private set => SetProperty(ref _sourceLanguages, value);
        }

        private IdentifiableLanguage _selectedSourceLanguage;

        /// <summary>
        /// The Currently selected source language.
        /// </summary>
        public IdentifiableLanguage SelectedSourceLangauge
        {
            set
            {
                SetProperty(ref _selectedSourceLanguage, value);


                // fetch out all target languages whose source language equals selected source language
                TargetLanguages = new IdentifiableLanguages
                {
                    Languages = SourceLanguages.Languages
                    .Where(p => _translationModels.Models.Any(p2 => p2.Source.Equals(_selectedSourceLanguage.Language) && p2.Target.Equals(p.Language)))
                    .ToList()
                };

                // if there only single target language, auto select 
                if (TargetLanguages.Languages.Count == 1)
                {
                    SelectedTargetLangauge = TargetLanguages.Languages.First();
                }
                IsAutoTranslateCheckBoxEnabled = SelectedSourceLangauge != null && SelectedTargetLangauge != null;
                IsStartTranslationBtnEnabled = CanStartTranslationManually();
            }
            get
            {
                return _selectedSourceLanguage;
            }
        }

        private IdentifiableLanguages _targetLanguages;

        /// <summary>
        /// The target languages.
        /// </summary>
        public IdentifiableLanguages TargetLanguages
        {
            get => _targetLanguages;
            private set => SetProperty(ref _targetLanguages, value);
        }

        private IdentifiableLanguage _selectedTargetLanguage;

        /// <summary>
        /// The Currently selected Target language.
        /// </summary>
        public IdentifiableLanguage SelectedTargetLangauge
        {
            set
            {
                SetProperty(ref _selectedTargetLanguage, value);
                IsAutoTranslateCheckBoxEnabled = SelectedSourceLangauge != null && SelectedTargetLangauge != null;
                IsStartTranslationBtnEnabled = CanStartTranslationManually();
            }
            get
            {
                return _selectedTargetLanguage;
            }
        }

        private TranslationModels _translationModels;

        /// <summary>
        /// Speech translation models provided by IBM Speech Translator.
        /// </summary>
        public TranslationModels TranslationModels
        {
            private set
            {
                SetProperty(ref _translationModels, value);
            }
            get
            {
                return _translationModels;
            }
        }


        #endregion

        #endregion

        #region Constructor

        public MainViewModel()
        {
            Translator = new IBMTranslator();
            Translator.LanguageTranslatorException += Translator_LanguageTranslatorException;

            LoadTranslationData().ConfigureAwait(false);

            StartTranscriptionCommand = new DelegateCommand(StartTranscription).ObservesCanExecute(() => IsStartTranscriptionBtnEnabled);
            StopTranscriptionCommand = new DelegateCommand(StopTranscription).ObservesCanExecute(() => IsStopTranscriptionBtnEnabled);


            StartTranslationCommand = new DelegateCommand(StartTranslation).ObservesCanExecute(() => IsStartTranslationBtnEnabled);

        }

        #endregion

        #region Commands
        public DelegateCommand StartTranscriptionCommand { get; private set; }
        public DelegateCommand StopTranscriptionCommand { get; private set; }
        public DelegateCommand StartTranslationCommand { get; private set; }
        #endregion

        #region Methods

        /// <summary>
        /// Enable/Disable buttons. Sets the <see cref="IsStopTranscriptionBtnEnabled"/> value 
        /// opposite to the value supplied.
        /// </summary>
        /// <param name="isEnable"></param>
        private void EnableButtons(bool isEnable)
        {
            IsToggleEnabled = isEnable;
            IsStartTranscriptionBtnEnabled = isEnable;
            IsStopTranscriptionBtnEnabled = !isEnable;
        }

        /// <summary>
        /// Starts the transcription.
        /// </summary>
        private void StartTranscription()
        {
            EnableButtons(false);
            _cancellationTokenSource = new CancellationTokenSource();

            _transcriptionController = new TranscriptionController(SpeechToTextService, _cancellationTokenSource.Token);
            _transcriptionController.StatusUpdater += TranscriptionController_UpdateStatusText;
            _transcriptionController.TranscribedTextUpdater += TranscriptionController_UpdateTranscribedText;

            StatusViewModel.Instance.ChangeStatus("Connecting...");

            _transcriptionController.StartRecording();

        }

        /// <summary>
        /// Stops the transcription.
        /// </summary>
        private void StopTranscription()
        {
            _transcriptionController.StopRecording();
            _cancellationTokenSource.Cancel();

            StatusViewModel.Instance.ChangeStatus("");

            // pass true to disable Stop button
            EnableButtons(true);
        }


        /// <summary>
        /// Starts the Translation.
        /// </summary>
        private async void StartTranslation()
        {
            IsStartTranslationBtnEnabled = false;

            await TranslateAsync(TranscribedText);

            IsStartTranslationBtnEnabled = CanStartTranslationManually();
        }

        /// <summary>
        /// Asynchronously translates the provided text using IBM Speech Translator.
        /// </summary>
        /// <param name="textToBeTranslated">The text to be translated.</param>
        /// <returns></returns>
        private async Task TranslateAsync(string textToBeTranslated)
        {
            await Task.Run(() =>
            {
                List<Translation> translations = Translator
                .Translate(new List<string>() { textToBeTranslated }, $"{SelectedSourceLangauge.Language}-{SelectedTargetLangauge.Language}");

                if (translations == null || translations.Count == 0) { return; }

                TranslatedText += string.IsNullOrWhiteSpace(TranslatedText) ? "" : " ";
                foreach (var translation in translations)
                {
                    TranslatedText += translation._Translation;
                }
            });
        }

        /// <summary>
        /// Determines whether a user can manually start the translation or not.
        /// </summary>
        /// <returns></returns>
        private bool CanStartTranslationManually()
        {
            return !string.IsNullOrWhiteSpace(_transcribedText) && !IsAutoTranslateCheckBoxChecked
                    && SelectedSourceLangauge != null && SelectedTargetLangauge != null;
        }

        private async Task LoadTranslationData()
        {
            await Task.Run(() =>
            {
                _translationModels = Translator.GetModelsList();
                if (_translationModels?.Models.Any() != true) { return; }

                SourceLanguages = Translator.GetIdentifiableLanguagesList();
                if (_sourceLanguages?.Languages.Any() != true) { return; }


                /* 
                 * * find out all those languages for which IBM provide speech translation.
                * SourceLanguages does contain some languages which can't be translated to any other language.
                */
                SourceLanguages.Languages = SourceLanguages.Languages
                .Where(p => _translationModels.Models.Any(p2 => p2.Source.Equals(p.Language)))
                .OrderBy(i => i.Name).ToList();
            });
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// The transcribed Text Backup.
        /// To be done: Backup TranscribedText value when text is edited from the text box. 
        /// Currently this is not working.
        /// </summary>
        private string TranscribedTextBackup { get; set; }


        private void TranscriptionController_UpdateTranscribedText(object recognitionResults)
        {
            switch (recognitionResults)
            {
                case SpeechRecognitionResults IBMSpeechRecognitionResults:
                    foreach (var result in IBMSpeechRecognitionResults.Results)
                    {
                        var tempText = "";
                        foreach (var transcript in result.Alternatives)
                        {
                            tempText += transcript.Transcript;
                        }

                        TranscribedText = TranscribedTextBackup + tempText;
                        if (result.FinalResults == true)
                        {
                            TranscribedTextBackup = TranscribedText;
                            if (IsAutoTranslateCheckBoxChecked && IsAutoTranslateCheckBoxEnabled)
                            {
                                TranslateAsync(tempText).ConfigureAwait(false);
                            }
                        }
                    }
                    break;
                case Transcript amazonTranscript:
                    foreach (var result in amazonTranscript.Results)
                    {
                        string tempText = "";
                        foreach (var alternative in result.Alternatives)
                        {
                            TranscribedText = TranscribedTextBackup + alternative.Transcript;
                            tempText = alternative.Transcript;
                        }
                        // take backup of the final result
                        if (!result.IsPartial)
                        {
                            TranscribedTextBackup = $"{TranscribedText} ";
                            if (IsAutoTranslateCheckBoxChecked && IsAutoTranslateCheckBoxEnabled)
                            {
                                TranslateAsync($"{tempText} ").ConfigureAwait(false);
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Update status text.
        /// </summary>
        /// <param name="status"></param>
        private async void TranscriptionController_UpdateStatusText(object status)
        {
            if (status is ServiceState response)
            {
                // Take the initial backup of the TranscribedText property when service starts in listening
                if (!string.IsNullOrWhiteSpace(response.State) &&
                    response.State.Equals("listening", StringComparison.OrdinalIgnoreCase))
                {
                    TranscribedTextBackup = TranscribedText;
                }
                else if (!string.IsNullOrWhiteSpace(response.Error))
                {
                    StopTranscription();
                    await _dialogCoordinator.ShowMessageAsync(this, "Error", response.Error);
                    //    MessageBox.Show(response.Error);
                }

                StatusViewModel.Instance.ChangeStatus(response.State);
            }
        }

        /// <summary>
        /// Gets invoked whenever any exception occurred in language translator.
        /// </summary>
        /// <param name="exception"></param>
        private async void Translator_LanguageTranslatorException(object exception)
        {
            if (exception is string error)
                await _dialogCoordinator.ShowMessageAsync(this, "Error", error);

            IsStartTranslationBtnEnabled = CanStartTranslationManually();
        }

        #endregion
    }
}
