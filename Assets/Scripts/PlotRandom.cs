using Python.Runtime;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPython
{
    [DefaultExecutionOrder(-1)]
    public class PlotRandom : MonoBehaviour
    {
        [SerializeField] private RawImage _rawImage;
        [SerializeField] private Button _button;

        private void OnEnable()
        {
            _button.onClick.AddListener(PlotFig);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(PlotFig);
        }

        private void PlotFig()
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
                    LoadImage(span, w, h);
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