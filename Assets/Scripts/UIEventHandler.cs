using Python.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityPython
{
    [RequireComponent(typeof(UIDocument))]
    public class UIEventHandler : MonoBehaviour
    {
        private SemaphoreSlim _semaphoreSlim;
        private (Texture2D tex, int width, int height) _tex;
        private Button _plotButton;

        private void Start()
        {
            if (TryGetComponent(out UIDocument uiDocument))
            {
                InitializeView(uiDocument.rootVisualElement);
            }
        }

        private void InitializeView(VisualElement rootElement)
        {
            if (rootElement.Q<VisualElement>("Image") is { } imageElement)
            {
                (_tex.width, _tex.height) = (2, 2);
                _tex.tex = new Texture2D(_tex.width, _tex.height, TextureFormat.RGBA32, false)
                {
                    filterMode = FilterMode.Point,
                    wrapMode = TextureWrapMode.Clamp
                };
                var rawData = _tex.tex.GetRawTextureData();
                Array.Fill(rawData, (byte)255);
                _tex.tex.LoadRawTextureData(rawData);
                _tex.tex.Apply();

                imageElement.style.backgroundImage = _tex.tex;
            }

            if ((_plotButton = rootElement.Q<Button>("PlotButton")) is { })
            {
                _semaphoreSlim = new SemaphoreSlim(1, 1);

                _plotButton.clicked += PlotFig;
            }
        }

        private void OnDestroy()
        {
            _plotButton.clicked -= PlotFig;

            _semaphoreSlim.Dispose();
            Destroy(_tex.tex);
        }

        private async void PlotFig()
        {
            IntPtr? state = null;
            try
            {
                await _semaphoreSlim.WaitAsync(destroyCancellationToken);
                state = PythonEngine.BeginAllowThreads();
                var (bytes, w, h) = await Task.Run(Plot, destroyCancellationToken);
                LoadImage(bytes, w, h);
            }
            catch (OperationCanceledException e) when (e.CancellationToken != destroyCancellationToken)
            {
                throw;
            }
            finally
            {
                if (state.HasValue)
                {
                    PythonEngine.EndAllowThreads(state.Value);
                }
                _semaphoreSlim.Release();
            }
        }

        private (byte[], int, int) Plot()
        {
            using (Py.GIL())
            {
                using dynamic plotRandom = Py.Import("plot_random");

                using dynamic ret = plotRandom.draw();
                using (ret[0]) // To prevent GC of the numpy array, keep the byte array read from 'addr' on the C# side until it is fully loaded.
                {
                    var (addr, w, h, s) = ((long)ret[1], (int)ret[2], (int)ret[3], (int)ret[4]);
                    var ptr = new IntPtr(addr);
                    ReadOnlySpan<byte> span;
                    unsafe
                    {
                        span = new ReadOnlySpan<byte>(ptr.ToPointer(), w*h*s);
                    }
                    return (span.ToArray(), w, h);
                }
            }
        }

        private void LoadImage(ReadOnlySpan<byte> rawData, int w, int h)
        {
            if (_tex.width != w || _tex.height != h)
            {
                (_tex.width, _tex.height) = (w, h);
                _tex.tex.Reinitialize(_tex.width, _tex.height);
            }
            _tex.tex.LoadRawTextureData(rawData.ToArray());
            _tex.tex.Apply();
        }
    }
}