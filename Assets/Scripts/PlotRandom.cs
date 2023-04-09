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

                // numpy配列のGCを防ぐためにC#側で保持して別途アドレス変換を呼び出す
                using var pixels = plotRandom.draw();
                using var ret = plotRandom.getarrayaddr(pixels);
                var (addr, w, h, s) = ((long)ret[0], (int)ret[1], (int)ret[2], (int)ret[3]);

                var ptr = new IntPtr(addr);
                ReadOnlySpan<byte> span;
                unsafe
                {
                    span = new ReadOnlySpan<byte>(ptr.ToPointer(), w*h*s);
                }
                LoadImage(span, w, h);
            }
        }

        private void LoadImage(ReadOnlySpan<byte> rawData, int w, int h)
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
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