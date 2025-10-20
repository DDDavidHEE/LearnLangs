using System;
using System.IO;
using Microsoft.CognitiveServices.Speech.Audio;

namespace LearnLangs.Services.Pronunciation
{
    // Simple adapter for stream input to Azure Speech SDK
    public class BinaryAudioStreamReader : PullAudioInputStreamCallback
    {
        private readonly Stream _source;

        public BinaryAudioStreamReader(Stream source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _source.Position = 0;
        }

        public override int Read(byte[] dataBuffer, uint size)
        {
            try
            {
                var read = _source.Read(dataBuffer, 0, (int)size);
                return read;
            }
            catch
            {
                return 0;
            }
        }

        public override void Close()
        {
            base.Close();
            try { _source.Dispose(); } catch { }
        }
    }
}
