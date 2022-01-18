using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PuppeteerPanelLib
{
    public partial class PuppeteerPanel : UserControl
    {
        private Action<object, EventArgs> appIdleAction = null;
        private EventHandler appIdleEvent = null;

        public PuppeteerPanel()
        {
            InitializeComponent();
            this.SuspendLayout();
            appIdleAction = new Action<object, EventArgs>(Application_Idle);
            appIdleEvent = new EventHandler(appIdleAction);
        }

        public PuppeteerPanel(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
            appIdleAction = new Action<object, EventArgs>(Application_Idle);
            appIdleEvent = new EventHandler(appIdleAction);
        }

        public void Show(Process app)
        {
            if (app == null || app.MainWindowHandle == IntPtr.Zero) return;

            AppProcess = app;

            try
            {
                SetParent(app.MainWindowHandle, this.Handle);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception - PuppeteerPanel => SetParent:" + ex.Message);
            }

            try
            {
                SetWindowLong(new HandleRef(this, app.MainWindowHandle), GWL_STYLE, WS_VISIBLE);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception - PuppeteerPanel => SetWindowLong:" + ex.Message);
            }

            try
            {
                MoveWindow(app.MainWindowHandle, 0, 0, this.Width, this.Height, true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception - PuppeteerPanel => MoveWindow:" + ex.Message);
            }
        }

        public void Stop()
        {
            if (_appProcess != null)
            {
                try
                {
                    if (!_appProcess.HasExited)
                    {
                        _appProcess.Kill();
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception - PuppeteerPanel => Stop:" + ex.Message);
                }
                _appProcess = null;
            }
        }

        private void ReEmbed()
        {
            Show(_appProcess);
        }

        public void CloseExistingChromiumProcs()
        {
            // Check if chromium is already running, end process if its running

            try
            {
                Process[] processesChrome = Process.GetProcessesByName("chrome");

                if (processesChrome.Length > 0)
                {
                    foreach (Process proc in processesChrome)
                    {
                        if (proc.MainWindowHandle == IntPtr.Zero)
                            continue;

                        // Ensure that we *only* close chromium and NOT google chrome
                        if (proc.MainModule != null && proc.MainModule.FileVersionInfo.FileDescription == @"Chromium")
                        {
                            proc.Kill();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }

        }

        #region Process

        private Process _appProcess = null;

        public Process AppProcess
        {
            get { return _appProcess; }
            set { _appProcess = value; }
        }

        public bool IsStarted { get { return (_appProcess != null); } }

        #endregion

        #region EVENTS

        private void Application_Idle(object sender, EventArgs e)
        {
            if (_appProcess == null || _appProcess.HasExited)
            {
                _appProcess = null;
                Application.Idle -= appIdleEvent;
                return;
            }
            if (_appProcess.MainWindowHandle == IntPtr.Zero) return;
            Application.Idle -= appIdleEvent;
            Show(_appProcess);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            Stop();
            base.OnHandleDestroyed(e);
        }

        protected override void OnResize(EventArgs eventargs)
        {
            Debug.WriteLine("Event - Resized");

            if (_appProcess != null && !_appProcess.HasExited)
            {
                MoveWindow(_appProcess.MainWindowHandle, 0, 0, this.Width, this.Height, true);
            }
            base.OnResize(eventargs);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            Debug.WriteLine("Event - Size Changed");

            this.Invalidate();
            base.OnSizeChanged(e);
        }

        #endregion

        #region Win32 API

        private const int GWL_STYLE = (-16);
        private const int WS_VISIBLE = 0x10000000;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern long SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        public static IntPtr SetWindowLong(HandleRef hWnd, int nIndex, int dwNewLong)
        {
            if (IntPtr.Size == 4)
            {
                return SetWindowLongPtr32(hWnd, nIndex, dwNewLong);
            }
            return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr32(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
        public static extern IntPtr SetWindowLongPtr64(HandleRef hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

        #endregion

    }
}
