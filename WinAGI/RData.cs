using System;

namespace WinAGI.Engine
{
    public class RData
    {
        internal delegate void RDataChangedEventHandler(object sender, RDataChangedEventArgs e);
        internal event RDataChangedEventHandler PropertyChanged;
        byte[] mbytData = [];
        public class RDataChangedEventArgs
        {
            public RDataChangedEventArgs(int size)
            {
                //
            }
            public int Size { get; }
        }
        protected void OnPropertyChanged(int size)
        {
            PropertyChanged?.Invoke(this, new RDataChangedEventArgs(size));
        }
        public byte this[int index]
        {
            get
            {
                return mbytData[index];
            }
            set
            {
                mbytData[index] = value;
                OnPropertyChanged(mbytData.Length);
            }
        }
        public byte[] AllData
        {
            get
            {
                return mbytData;
            }
            set
            {
                mbytData = value;
                OnPropertyChanged(mbytData.Length);
            }
        }
        public int Length { get { return mbytData.Length; } private set { } }
        public void ReSize(int newSize)
        {
            if (newSize < 0) {
                //error 
                throw new IndexOutOfRangeException();
            }
            if (newSize != mbytData.Length) {
                Array.Resize(ref mbytData, newSize);
                OnPropertyChanged(mbytData.Length);
            }
        }
        public void Clear()
        {
            //reset to an empty array
            mbytData = [];
            OnPropertyChanged(mbytData.Length);
        }
        public RData(int Size)
        {
            mbytData = new byte[Size];
            // raise change event
            OnPropertyChanged(Size);
        }
    }
}
