using IBM.Cloud.SDK.Core.Authentication.Iam;
using IBM.Cloud.SDK.Core.Http.Exceptions;
using IBM.Watson.LanguageTranslator.v3;
using IBM.Watson.LanguageTranslator.v3.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace LanguageTranslator
{
    /// <summary>
    /// The Speech translator class which translates the text from one language to another using <see cref="IBM.Watson.LanguageTranslator.v3"/>.
    /// </summary>
    public class IBMTranslator
    {
        #region Private Fields

        private static readonly string _apikey = ConfigurationManager.AppSettings["ApiKey_IBMLanguageTranslator"];
        private static readonly string _url = ConfigurationManager.AppSettings["URL_IBMLanguageTranslator"];
        private static readonly string _version = ConfigurationManager.AppSettings["Version_IBMLanguageTranslator"];

        private readonly IamAuthenticator _authenticator;
        private readonly LanguageTranslatorService service;

        #endregion

        #region Constructor

        public IBMTranslator()
        {
            _authenticator = new IamAuthenticator(
                apikey: _apikey
            );
            service = new LanguageTranslatorService(_version, _authenticator);
            service.SetServiceUrl(_url);
        }


        #endregion

        #region Methods

        /// <summary>
        /// Retrieves the list of identifiable languages from the IBM Speech Translator.
        /// </summary>
        /// <returns>List of Identifiable Languages</returns>
        public IdentifiableLanguages GetIdentifiableLanguagesList()
        {
            try
            {
                return JsonConvert.DeserializeObject<IdentifiableLanguages>(service.ListIdentifiableLanguages().Response);
            }
            catch (ServiceResponseException e)
            {
                UpdateLanguageTranslatorException($"{e.Message} {(e.InnerException != null ? $"{e.InnerException.ToString()} {e.InnerException.Message}" : string.Empty)}");
            }
            catch (Exception e)
            {
                UpdateLanguageTranslatorException($"{e.Message} {(e.InnerException != null ? $"{e.InnerException.ToString()} {e.InnerException.Message}" : string.Empty)}");
            }
            return null;
        }

        /// <summary>
        /// Detect the language of supplied text. 
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Returns the language with highest confidence.</returns>
        public IdentifiedLanguage IdentifyLanguage(string text)
        {
            try
            {
                var result = service.Identify(
                    text: text
                 );

                var identifiedLanguages =
                    JsonConvert.DeserializeObject<IdentifiedLanguages>(result.Response);

                var languageWithHighestConfidence = identifiedLanguages.Languages
                    .Aggregate((i1, i2) => i1.Confidence > i2.Confidence ? i1 : i2);

                return languageWithHighestConfidence;
            }
            catch (ServiceResponseException e)
            {
                UpdateLanguageTranslatorException($"{e.Message} {(e.InnerException != null ? $"{e.InnerException.ToString()} {e.InnerException.Message}" : string.Empty)}");
            }
            catch (Exception e)
            {
                UpdateLanguageTranslatorException($"{e.Message} {(e.InnerException != null ? $"{e.InnerException.ToString()} {e.InnerException.Message}" : string.Empty)}");
            }
            return null;
        }

        /// <summary>
        /// Get the list of available translation models from <see cref="IBM.Watson.LanguageTranslator.v3"/>.
        /// </summary>
        /// <param name="source">The source language code.</param>
        /// <param name="target">The target language code.</param>
        /// <param name="_dafault">A boolean value indicating if it's a default model or not.</param>
        /// <returns></returns>
        public TranslationModels GetModelsList(string source = null, string target = null, bool? _dafault = null)
        {
            try
            {
                return JsonConvert.DeserializeObject<TranslationModels>(service.ListModels(source, target, _dafault).Response);
            }
            catch (ServiceResponseException e)
            {
                UpdateLanguageTranslatorException($"{e.Message} {(e.InnerException != null ? $"{e.InnerException.ToString()} {e.InnerException.Message}" : string.Empty)}");
            }
            catch (Exception e)
            {
                UpdateLanguageTranslatorException($"{e.Message} {(e.InnerException != null ? $"{e.InnerException.ToString()} {e.InnerException.Message}" : string.Empty)}");
            }
            return null;
        }

        /// <summary>
        /// Translate the text from one language to another using model.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="modelId"></param>
        /// <returns></returns>
        public List<Translation> Translate(List<string> text, string modelId)
        {
            try
            {
                var result = service.Translate(
                text: text,
                modelId: modelId
                );
                var translationResult = JsonConvert.DeserializeObject<TranslationResult>(result.Response);
                return translationResult.Translations;
            }
            catch (ServiceResponseException e)
            {
                UpdateLanguageTranslatorException($"{e.Message} {(e.InnerException != null ? $"{e.InnerException.ToString()} {e.InnerException.Message}" : string.Empty)}");
            }
            catch (Exception e)
            {
                UpdateLanguageTranslatorException($"{e.Message} {(e.InnerException != null ? $"{e.InnerException.ToString()} {e.InnerException.Message}" : string.Empty)}");
            }
            return null;
        }


        #endregion

        #region Delegates

        public delegate void LanguageTranslatorExceptionHandler(object exception);
        public event LanguageTranslatorExceptionHandler LanguageTranslatorException;

        public void UpdateLanguageTranslatorException(object exception)
        {
            LanguageTranslatorException?.Invoke(exception);
        }

        #endregion
    }
}
