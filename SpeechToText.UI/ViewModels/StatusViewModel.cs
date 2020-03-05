using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpeechToText.UI.ViewModels
{
    public sealed class StatusViewModel : BindableBase
    {
        #region Private Fields

        /// <summary>
        /// An object to make the singleton pattern implementation thread safe
        /// </summary>
        private static readonly object padlock = new object();

        #endregion

        #region Constructor
        private StatusViewModel()
        {

        }

        #endregion

        #region Full Properties

        private static StatusViewModel instance = null;

        public static StatusViewModel Instance
        {
            get
            {
                if (instance == null)
                {
                    /* lock this object so that one thread can access this code block at a time,
                     * this ensures that only one thread will create an instance */
                    lock (padlock)
                    {
                        if (instance == null)
                        {
                            instance = new StatusViewModel();
                        }
                    }
                }
                return instance;
            }
        }


        private string status;
        /// <summary>
        /// Status text.
        /// </summary>
        public string Status
        {
            get => status;
            private set => SetProperty(ref status, value);
        }

        #endregion

        #region Methods

        public void ChangeStatus(string status)
        {
            Status = status;
        }

        #endregion
    }
}
