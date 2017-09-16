using Captura.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Captura.Models
{
    class DrawingWindowProvider : IDrawingWindow
    {

        readonly Window _mainWindow;
        Window _drawingWindow;

        public DrawingWindowProvider(Window Window)
        {
            _mainWindow = Window;
        }

        public void Show()
        {
            if (_drawingWindow == null)
            {
                _drawingWindow = new DrawingWindow();
                _drawingWindow.Owner = this._mainWindow;
                _drawingWindow.WindowStyle = WindowStyle.None;                
                var rect = Screna.WindowProvider.DesktopRectangle;
                _drawingWindow.Left = 0;
                _drawingWindow.Top = 0;
                _drawingWindow.Width = rect.Width;
                _drawingWindow.Height = rect.Height;
                _drawingWindow.Closed += _drawingWindow_Closed;
            }
            _drawingWindow.Show();
        }

        private void _drawingWindow_Closed(object sender, EventArgs e)
        {
            this._drawingWindow = null;
        }
    }
}
