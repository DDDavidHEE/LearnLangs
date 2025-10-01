// LearnLangs/Services/Pronunciation/PronunciationAssessmentService.cs
using System.Diagnostics;
using System.Linq;
using LearnLangs.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.CognitiveServices.Speech;
using Microsoft.CognitiveServices.Speech.Audio;
using Microsoft.CognitiveServices.Speech.PronunciationAssessment;
using Microsoft.Extensions.Options;
using NAudio;
using NAudio.Wave;

namespace LearnLangs.Services.Pronunciation;

public class PronunciationAssessmentService : IPronunciationAssessmentService
{
    private readonly AzureSpeechOptions _opts;
    public PronunciationAssessmentService(IOptions<AzureSpeechOptions> opts) => _opts = opts.Value;

    public async Task<PronunciationResultDto> AssessAsync(
        string referenceText, IFormFile wavOrCompressedFile, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(referenceText))
            throw new ArgumentException("referenceText is required");
        if (wavOrCompressedFile is null || wavOrCompressedFile.Length == 0)
            throw new ArgumentException("Audio file is required");

        // Giới hạn kích thước (ví dụ 20MB)
        const long MAX_BYTES = 20L * 1024 * 1024;
        if (wavOrCompressedFile.Length > MAX_BYTES)
            throw new InvalidOperationException("Audio file is too large (limit 20 MB).");

        // Lưu file gốc
        var origExt = Path.GetExtension(wavOrCompressedFile.FileName);
        var origPath = Path.Combine(Path.GetTempPath(), $"learnlangs-orig-{Guid.NewGuid()}{origExt}");
        await using (var fs = new FileStream(origPath, FileMode.Create, FileAccess.Write))
        {
            await wavOrCompressedFile.CopyToAsync(fs, ct);
        }

        // Đường dẫn WAV PCM chuẩn hóa
        var wav16Path = Path.Combine(Path.GetTempPath(), $"learnlangs-16k-{Guid.NewGuid()}.wav");

        try
        {
            // ================== CHUẨN HÓA ÂM THANH ==================
            // Đọc mọi định dạng phổ biến (mp3/m4a/wav/flac...) -> PCM float
            // rồi resample -> WAV PCM 16kHz 16-bit mono
            using (var reader = new AudioFileReader(origPath))
            {
                var targetFormat = new WaveFormat(16000, 16, 1); // 16 kHz, 16-bit, mono
                using var resampler = new MediaFoundationResampler(reader, targetFormat)
                {
                    ResamplerQuality = 60
                };
                WaveFileWriter.CreateWaveFile(wav16Path, resampler);
            }

            // ================== GỌI SPEECH SDK ==================
            var speechConfig = SpeechConfig.FromSubscription(_opts.SubscriptionKey, _opts.Region);
            speechConfig.SpeechRecognitionLanguage = _opts.RecognitionLanguage; // "en-US"/"vi-VN"...

            using var audioConfig = AudioConfig.FromWavFileInput(wav16Path);
            using var recognizer = new SpeechRecognizer(speechConfig, audioConfig);

            var paConfig = new PronunciationAssessmentConfig(
                referenceText,
                GradingSystem.HundredMark,
                Granularity.Phoneme,
                enableMiscue: true);
            paConfig.EnableProsodyAssessment();
            paConfig.ApplyTo(recognizer);

            // (tùy chọn) ghi log SDK ra file để debug sâu
            speechConfig.SetProperty(PropertyId.Speech_LogFilename, Path.Combine(Path.GetTempPath(), "speechsdk.log"));

            // Nới thời gian im lặng đầu/cuối (ms)
            speechConfig.SetProperty(PropertyId.SpeechServiceConnection_InitialSilenceTimeoutMs, "6000");
            speechConfig.SetProperty(PropertyId.SpeechServiceConnection_EndSilenceTimeoutMs, "1500");

            var result = await recognizer.RecognizeOnceAsync().ConfigureAwait(false);

            switch (result.Reason)
            {
                case ResultReason.RecognizedSpeech:
                    break; // ok

                case ResultReason.NoMatch:
                    throw new InvalidOperationException("No speech could be recognized. Try a longer/clearer clip.");

                case ResultReason.Canceled:
                    var cancel = CancellationDetails.FromResult(result);
                    // Trả lỗi cực chi tiết để khoanh vùng nhanh
                    throw new InvalidOperationException(
                        $"Canceled: Reason={cancel.Reason}; ErrorCode={cancel.ErrorCode}; Details={cancel.ErrorDetails}");

                default:
                    throw new InvalidOperationException($"Unexpected result: {result.Reason}");
            }


            var paResult = PronunciationAssessmentResult.FromResult(result);
            var rawJson = result.Properties.GetProperty(PropertyId.SpeechServiceResponse_JsonResult);

            var words = (paResult.Words ?? Array.Empty<PronunciationAssessmentWordResult>())
                .Select(w => new WordScoreDto(w.Word, w.ErrorType.ToString(), w.AccuracyScore))
                .ToList();

            return new PronunciationResultDto(
                paResult.PronunciationScore,
                paResult.AccuracyScore,
                paResult.FluencyScore,
                paResult.CompletenessScore,
                paResult.ProsodyScore,
                words,
                rawJson);
        }
        catch (MmException mmEx)
        {
            // Lỗi codec/driver trên máy chủ
            throw new InvalidOperationException("Audio conversion failed on server (Media Foundation).", mmEx);
        }
        catch (Exception ex) when (ex is InvalidOperationException || ex is FormatException || ex is IOException)
        {
            // Gợi ý debug thân thiện
            throw new InvalidOperationException("Audio processing failed. Try another file (mp3/m4a/wav) or shorter clip.", ex);
        }
        finally
        {
            // Dọn file tạm
            TryDelete(origPath);
            TryDelete(wav16Path);
        }
    }

    private static void TryDelete(string path)
    {
        try { if (!string.IsNullOrEmpty(path) && File.Exists(path)) File.Delete(path); }
        catch { /* ignore */ }
    }
}
