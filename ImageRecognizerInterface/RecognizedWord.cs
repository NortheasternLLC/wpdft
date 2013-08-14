using System;

namespace ImageRecognizerInterface
{
    [Serializable]
    public sealed class RecognizedWord
    {
        public string Text
        {
            get;
            set;
        }

        public double Confidence
        {
            get;
            set;
        }

        public int Left
        {
            get;
            set;
        }

        public int Right
        {
            get;
            set;
        }

        public int Bottom
        {
            get;
            set;
        }

        public int Top
        {
            get;
            set;
        }
    }
}