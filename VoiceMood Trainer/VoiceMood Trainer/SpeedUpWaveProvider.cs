using NAudio.Wave;
using SoundTouch.Net.NAudioSupport;

namespace VoiceMood_Trainer {
public class SpeedUpWaveProvider : IWaveProvider, IDisposable {
    private readonly SoundTouchWaveProvider _soundTouchProvider;
    private bool _disposed = false;

    public SpeedUpWaveProvider(WaveStream sourceStream, float speedRatio) {
        _soundTouchProvider = new SoundTouchWaveProvider(sourceStream);
        _soundTouchProvider.Tempo = speedRatio;
    }

    public int Read(byte[] buffer, int offset, int count) {
        return _soundTouchProvider.Read(buffer, offset, count);
    }

    public WaveFormat WaveFormat => _soundTouchProvider.WaveFormat;

    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing) {
        if (!_disposed) {
            if (disposing) {
                // Освобождаем управляемые ресурсы
                if (_soundTouchProvider is IDisposable disposableProvider) {
                    disposableProvider.Dispose();
                }
            }

            // Освобождаем неуправляемые ресурсы (если есть)

            _disposed = true;
        }
    }

    ~SpeedUpWaveProvider() {
        Dispose(false);
    }
}


}
