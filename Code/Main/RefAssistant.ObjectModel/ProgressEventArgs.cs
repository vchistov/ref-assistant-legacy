//
// Copyright © 2011 Lardite.
//
// Author: Belikov Sergey (sbelikov@lardite.com)
//

using System;

namespace Lardite.RefAssistant
{
    /// <summary>
    /// Progress event args.
    /// </summary>
    public sealed class ProgressEventArgs : EventArgs
    {
        private int _progress;
        private string _text;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="progress">Progress.</param>
        /// <param name="text">Text.</param>
        public ProgressEventArgs(int progress, string text)
        {
            this._progress = progress;
            this._text = text;
        }

        /// <summary>
        /// Progress.
        /// </summary>
        public int Progress 
        {
            get
            {
                return _progress;
            }
        }

        /// <summary>
        /// Text.
        /// </summary>
        public string Text
        {
            get
            {
                return _text;
            }
        }
    }
}