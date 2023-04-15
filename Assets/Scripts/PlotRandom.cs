using Python.Runtime;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPython
{
    [DefaultExecutionOrder(-1)]
    public class PlotRandom : MonoBehaviour
    {
        [SerializeField] private RawImage _rawImage;
        [SerializeField] private Button _button;
        private CancellationTokenSource cts;
        private bool _executingPython = false;

        private void OnEnable()
        {
            cts = new CancellationTokenSource();
            _button.onClick.AddListener(PlotFig);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(PlotFig);
            cts.Cancel();
            cts.Dispose();
        }

        private async void PlotFig()
        {
            if (_executingPython)
            {
                return;
            }

            _executingPython = true;
            var state = PythonEngine.BeginAllowThreads();
            try
            {
                var (bytes, w, h) = await Task.Run(Plot, cts.Token);
                LoadImage(bytes, w, h);
            }
            catch (OperationCanceledException e) when (e.CancellationToken != cts.Token)
            {
                throw;
            }
            finally
            {
                PythonEngine.EndAllowThreads(state);
                _executingPython = false;
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
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point,
                wrapMode = TextureWrapMode.Clamp
            };
            tex.LoadRawTextureData(rawData.ToArray());
            tex.Apply();
            Destroy(_rawImage.texture);
            _rawImage.texture = tex;
        }

        private void OnDestroy()
        {
            Destroy(_rawImage.texture);
        }
    }
}