using System;

namespace WinAGI.Engine {
    /// <summary>
    /// A class to allow read/write access to AGI game resources. 
    /// </summary>
    public class RData {
        internal delegate void RDataChangedEventHandler(object sender, RDataChangedEventArgs e);
        internal event RDataChangedEventHandler PropertyChanged;
        byte[] mbytData = [];

        /// <summary>
        /// Construcor for new RData object
        /// </summary>
        /// <param name="Size"></param>
        public RData(int Size) {
            mbytData = new byte[Size];
            // raise change event
            OnPropertyChanged(Size);
        }

        // add a change event 
        public class RDataChangedEventArgs(int size) {
            public int Size { get; } = size;
        }
        protected void OnPropertyChanged(int size) {
            PropertyChanged?.Invoke(this, new RDataChangedEventArgs(size));
        }

        public byte this[int index] {
            get {
                return mbytData[index];
            }
            set {
                mbytData[index] = value;
                OnPropertyChanged(mbytData.Length);
            }
        }

        /// <summary>
        /// Gets or sets the entire byte array of data for this resource.
        /// </summary>
        public byte[] AllData {
            get {
                return mbytData;
            }
            set {
                mbytData = value;
                OnPropertyChanged(mbytData.Length);
            }
        }

        /// <summary>
        /// Returns the size of the resource data array.
        /// </summary>
        public int Length {
            get {
                return mbytData.Length;
            }
        }

        /// <summary>
        /// Adjusts the size of the resource data array.
        /// </summary>
        /// <param name="newSize"></param>
        /// <exception cref="IndexOutOfRangeException"></exception>
        public void ReSize(int newSize) {
            if (newSize < 0) {
                throw new IndexOutOfRangeException();
            }
            if (newSize != mbytData.Length) {
                Array.Resize(ref mbytData, newSize);
                OnPropertyChanged(mbytData.Length);
            }
        }

        /// <summary>
        /// Deletes all resource data, setting it to an empty array.
        /// </summary>
        public void Clear() {
            mbytData = [];
            OnPropertyChanged(mbytData.Length);
        }
    }
}
