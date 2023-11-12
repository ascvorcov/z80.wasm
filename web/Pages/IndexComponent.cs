using Blazor.Extensions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

using z80emu;

namespace z80wasm.Pages
{
    public class IndexComponent : ComponentBase
    {
        private Emulator _emulator;
        protected BECanvasComponent _canvasReference;
        protected ElementReference _mainDivReference;

        [Inject]
        public IJSRuntime? _runtime { get; set; }


        [JSInvokable]
        public async ValueTask GameLoop(float timeStamp)
        {
            var frame = _emulator.RunToNextFrame(CancellationToken.None);
            if (frame == null)
            {
                return;
            }

            var reference = _canvasReference.CanvasReference;
            await _runtime.InvokeAsync<object>("drawOnCanvas", new object[]
            {
                reference,
                frame.Frame,
                frame.Palette.SelectMany(c => new[] { c.R, c.G, c.B }).ToArray()
            });
        }
        
        [JSInvokable]
        public async ValueTask UploadData(string data)
        {
            await Task.Yield();
            var bytes = Convert.FromBase64String(data.Split(',').Last());
            _emulator.LoadZ80(bytes);
        }

        protected void OnKeyDown(KeyboardEventArgs e)
        {
            _emulator.KeyDown(Map(e));
        }

        protected void OnKeyUp(KeyboardEventArgs e)
        {
            _emulator.KeyUp(Map(e));
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            this._emulator = new Emulator();
            this._emulator.LoadSample();
            object obj = DotNetObjectReference.Create(this);
            await _runtime.InvokeAsync<object>("initGame", new[] { obj });
            await _mainDivReference.FocusAsync();
        }

        private Key Map(KeyboardEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Key))
                return Key.None;

            if (KeyCodes.TryGetValue(e.Key.ToUpperInvariant(), out Key ret))
                return ret;
            
            return Key.None;
        }

        private static readonly Dictionary<string, Key> KeyCodes = new ()
        {
            { "ENTER", Key.Enter },
            { "SHIFT", Key.Shift },
            { "CONTROL", Key.Sym },
            { " ", Key.Space },
            { "0", Key.D0 },
            { "1", Key.D1 },
            { "2", Key.D2 },
            { "3", Key.D3 },
            { "4", Key.D4 },
            { "5", Key.D5 },
            { "6", Key.D6 },
            { "7", Key.D7 },
            { "8", Key.D8 },
            { "9", Key.D9 },
            { "A", Key.A },
            { "B", Key.B },
            { "C", Key.C },
            { "D", Key.D },
            { "E", Key.E },
            { "F", Key.F },
            { "G", Key.G },
            { "H", Key.H },
            { "I", Key.I },
            { "J", Key.J },
            { "K", Key.K },
            { "L", Key.L },
            { "M", Key.M },
            { "N", Key.N },
            { "O", Key.O },
            { "P", Key.P },
            { "Q", Key.Q },
            { "R", Key.R },
            { "S", Key.S },
            { "T", Key.T },
            { "U", Key.U },
            { "V", Key.V },
            { "W", Key.W },
            { "X", Key.X },
            { "Y", Key.Y },
            { "Z", Key.Z }
        };        
    }
}