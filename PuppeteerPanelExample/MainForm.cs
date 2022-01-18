using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using PuppeteerSharp;

namespace PuppeteerPanelExample
{
    public partial class MainForm : Form
    {
        // ---------------- *IMPORTANT* ----------------
        // Set path to puppeteers chromium executable (chrome.exe). Example: C:\Temp\chrome.exe
        private string ChromiumExecutablePath = @"";

        private Process ChromiumProcess() => browser?.Process;
        private Browser browser;
        private Page page;

        public MainForm() => InitializeComponent();

        private void MainForm_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ChromiumExecutablePath))
            {
                MessageBox.Show(@"Error: Chromium executable path not set in MainForm.cs" + Environment.NewLine + @"Please try again.");
                Application.Exit();
            }
        }

        // Terminate chromium process on closing
        private void MainForm_FormClosing(object sender, FormClosingEventArgs e) => puppeteerPanel.CloseExistingChromiumProcs();

        private void btnStart_Click(object sender, EventArgs e) => StartChromium();
        private void btnStop_Click(object sender, EventArgs e) => StopChromium();

        private async Task InitializePuppeteerBrowser()
        {
            var options = new LaunchOptions
            {
                Headless = false,
                ExecutablePath = ChromiumExecutablePath
            };

            browser = await Puppeteer.LaunchAsync(options);
            page = await browser.NewPageAsync();
            await page.GoToAsync("http://www.example.org");
        }

        private async void StartChromium()
        {
            // Update buttons
            btnStart.Enabled = false;
            btnStop.Enabled = true;

            // Terminate chromium if already running
            puppeteerPanel.CloseExistingChromiumProcs();

            // Launch chromium using puppeteer
            await InitializePuppeteerBrowser();

            // Get process we just launched
            Process embeddedProcess = ChromiumProcess();

            if (embeddedProcess != null)
            {
                try
                {
                    // Show the process (window) in puppeteerPanel
                    puppeteerPanel.Show(ChromiumProcess());

                    // Update labels
                    SetStatusLabel("Running!");
                    SetProcessIdLabel();
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message);
                    SetStatusLabel("Error");
                }
            }
            else
            {
                SetStatusLabel("Error");
            }
        }

        private async void StopChromium()
        {
            await browser.CloseAsync();
            browser = null;

            // Make sure we terminate the chromium process
            if (ChromiumProcess() != null)
            {
                puppeteerPanel.Stop();
                puppeteerPanel.CloseExistingChromiumProcs();
            }

            // Update labels
            SetStatusLabel("Stopped");
            SetProcessIdLabel();

            // Update buttons
            btnStop.Enabled = false;
            btnStart.Enabled = true;
        }

        private void SetStatusLabel(string status) => lblStatus.Text = $@"Status: {status}";
        private void SetProcessIdLabel() => lblChromiumProcId.Text = ChromiumProcess() != null ? $@"Chromium Process ID: {ChromiumProcess().Id}" : @"Chromium Process ID:";
    }
}
