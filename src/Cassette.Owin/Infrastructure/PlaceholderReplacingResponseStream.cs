using System.IO;

namespace Cassette.Owin.Infrastructure
{
    public class PlaceholderReplacingResponseStream : MemoryStream
    {
        private readonly MemoryStream _bufferStream = new MemoryStream();

        private readonly Stream _outputStream;
        private readonly IPlaceholderTracker _placeholderTracker;

        private bool _hasWrittenToOutputStream;

        public PlaceholderReplacingResponseStream(Stream outputStream, IPlaceholderTracker placeholderTracker)
        {
            _outputStream = outputStream;
            _placeholderTracker = placeholderTracker;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _bufferStream.Write(buffer, offset, count);
        }

        public override void Close()
        {
            if (!_hasWrittenToOutputStream)
            {
                WriteBufferedOutput();
                _hasWrittenToOutputStream = true;
            }

            base.Close();
        }

        void WriteBufferedOutput()
        {
            var output = GetOutputWithPlaceholdersReplaced(_bufferStream);

            using (var writer = new StreamWriter(_outputStream))
            {
                writer.Write(output);
            }
        }

        string GetOutputWithPlaceholdersReplaced(MemoryStream originalStream)
        {
            var reader = new StreamReader(originalStream);
            reader.BaseStream.Seek(0, SeekOrigin.Begin);

            var stringdata = reader.ReadToEnd();
            return _placeholderTracker.ReplacePlaceholders(stringdata);
        }
    }
}