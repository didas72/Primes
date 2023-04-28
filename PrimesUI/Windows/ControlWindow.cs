using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;

using Raylib_cs;

using DidasUtils.Numerics;

using Primes.Common.Net;
using Primes.UI.Render;

namespace Primes.UI.Windows
{
    internal class ControlWindow : BaseWindow
    {
        private readonly TextBox ConnectionStatusText, RunStatusText, StatusText, PingText;
        private readonly TextBox BatchNumberText;
        private readonly ProgressBar BatchProgressBar;



        public ControlWindow()
        {
            Button btn; TextBox txtBox; ProgressBar prgBar;

            Window = new(Vector2i.Zero, "Control") { Id_Name = "CONTROL" };

            Window.Add(btn = new("Connect local", new(2, 2), new(168, 28))); btn.OnPressed += OnLocalConnectPressed;
            Window.Add(btn = new("Connect remote", new(172, 2), new(168, 28))); btn.OnPressed += OnRemoteConnectPressed;
            Window.Add(txtBox = new("Not connected.", 20, new(342, 2), new(338, 28), Highlights)); txtBox.Id_Name = "CONNECTION_STATUS"; ConnectionStatusText = txtBox;
            Window.Add(btn = new("Start/Stop", new(2, 32), new(168, 28))); btn.OnPressed += OnStartStopPressed;
            Window.Add(btn = new("Launch local", new(172, 32), new(168, 28))); btn.OnPressed += OnLocalLaunchPressed;
            Window.Add(txtBox = new("Run status...", 20, new(342, 32), new(338, 28), Highlights)); txtBox.Id_Name = "RUN_STATUS"; RunStatusText = txtBox;
            Window.Add(prgBar = new(new(2, 62), new(338, 28))); prgBar.Id_Name = "BATCH_PROGRESS"; BatchProgressBar = prgBar;
            Window.Add(txtBox = new("Batch XXXX", 20, new(342, 62), new(98, 28), Highlights)); txtBox.Id_Name = "BATCH_NUMBER"; BatchNumberText = txtBox;
            Window.Add(btn = new("Open primes folder", new(2, 92), new(338, 28))); btn.OnPressed += OnOpenPrimesFolderPressed;
            Window.Add(btn = new("Check for resource update", new(2, 122), new(338, 28))); btn.OnPressed += OnCheckForResourceUpdatePressed;
            Window.Add(txtBox = new("Status...", 20, new(2, 152), new(448, 28), Highlights)); txtBox.Id_Name = "STATUS"; StatusText = txtBox;
            Window.Add(txtBox = new("Ping: XXXms", 20, new(2, 182), new(448, 28), Highlights)); txtBox.Id_Name = "PING"; PingText = txtBox;
            Window.Add(btn = new("KILL SERVICE", 20, new(2, 538), new(168, 28), new(255, 255, 0, 255), new(255, 0, 0, 255), new(255, 100, 100, 255))); btn.OnPressed += OnKillServicePressed;
            Window.Add(btn = new("Disconnect", new(2, 212), new(168, 28))); btn.OnPressed += OnDisconnectPressed;

            ConnectionData.OnConnectionCheck += OnConnectionCheck;
        }



        #region Button handles
        private void OnLocalConnectPressed(object sender, EventArgs e)
        {
            ConnectionData.RemoteEndpoint = new IPEndPoint(IPAddress.Loopback, 13031);

            ConnectionData.EnableCheckTimer();
            UpdateConnectionStatus();
        }
        private void OnRemoteConnectPressed(object sender, EventArgs e)
        {

            PopupConnectRemote();
        }
        private void OnStartStopPressed(object sender, EventArgs e)
        {
            TimeSpan timeout = new(0, 0, 0, 0, 300);

            if (SendAndAwaitMessage(MessageBuilder.Message("run", string.Empty, "trun"), timeout, out byte[] response))
            {
                MessageBuilder.DeserializeMessage(response, out string messageType, out string target, out object value);
                if (!MessageBuilder.ValidateReturnMessage(messageType, target, value) || !((string)value).Contains("ACTION_PASS:"))
                {
                    StatusText.Text = "Start/stop failed.";
                    return;
                }

                StatusText.Text = "Started/stopped.";
            }
            else
            {
                StatusText.Text = "Start/stop failed.";
                return;
            }
        }
        private void OnLocalLaunchPressed(object sender, EventArgs e)
        {
            bool SVCrunning = Process.GetProcesses().Any((Process p) => p.ProcessName == "PrimesSVC.exe");
            string newStatus;

            if (SVCrunning)
            {
                newStatus = "Service already running.";
            }
            else
            {
                try
                {
                    ProcessStartInfo inf = new()
                    {
                        FileName = "cmd.exe",
                        Arguments = "/C start PrimesSVC.exe",
                        CreateNoWindow = true,
                        ErrorDialog = false,
                    };
                    Process p = Process.Start(inf);

                    if (!p.WaitForExit(20)) throw new Exception();
                    if (p.ExitCode != 0) throw new Exception();

                    newStatus = "Service started.";
                }
                catch { newStatus = "Failed to start service."; }
            }

            StatusText.Text = newStatus;
        }
        private void OnOpenPrimesFolderPressed(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", $"{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "primes")}");
        }
        private void OnCheckForResourceUpdatePressed(object sender, EventArgs e)
        {
            TimeSpan timeout = new(0, 0, 0, 0, 300);

            if (SendAndAwaitMessage(MessageBuilder.Message("req", string.Empty, "reslen"), timeout, out byte[] response))
            {
                MessageBuilder.DeserializeMessage(response, out string messageType, out string target, out object value);
                if (!MessageBuilder.ValidateReturnMessage(messageType, target, value) || !((string)value).Contains("REQUEST_PASS:"))
                {
                    StatusText.Text = "Failed to get resource info.";
                    return;
                }

                StatusText.Text = $"Reslen={((string)value)[13..]}. (not implemented)"; //TODO: Implement checking of resource version
            }
            else
            {
                StatusText.Text = "Failed to get resource info.";
                return;
            }
        }
        private void OnKillServicePressed(object sender, EventArgs e)
        {
            TimeSpan timeout = new(0, 0, 0, 1);

            if (SendAndAwaitMessage(MessageBuilder.Message("run", string.Empty, "fstop"), timeout, out byte[] response))
            {
                MessageBuilder.DeserializeMessage(response, out string messageType, out string target, out object value);
                if (!MessageBuilder.ValidateReturnMessage(messageType, target, value) || !((string)value).Contains("ACTION_PASS:"))
                {
                    StatusText.Text = "Failed to stop service.";
                    return;
                }

                StatusText.Text = "Service stopped.";
            }
            else
            {
                StatusText.Text = "Failed to stop service.";
                return;
            }
        }
        private void OnDisconnectPressed(object sender, EventArgs e)
        {
            ConnectionData.DisableCheckTimer();
            ConnectionData.RemoteEndpoint = null;

            ConnectionStatusText.Text = "Not connected.";
            StatusText.Text = "Disconnected.";
        }
        #endregion



        #region Popups
        private void PopupConnectRemote()
        {
            InputField field;
            Button btn;
            Holder pop = new(new(200, 175));
            pop.Add(new Panel(new(0, 0), new(400, 250), new Color(100, 100, 100, 255)));
            pop.Add(new TextBox("IP Address:", new(20, 20), new(200, 20)));
            pop.Add(field = new InputField(new(20, 45), new(300, 30))); field.Id_Name = "IP"; field.Text = "127.0.0.1:13031";
            pop.Add(btn = new("Connect", new(100, 212), new(90, 26))); btn.OnPressed += OnConnectRemotePressed;
            pop.Add(btn = new("Close", new(210, 212), new(90, 26))); btn.OnPressed += Program.OnPopupClosePressed;

            Program.ShowPopup(pop);
        }
        #endregion



        #region Popup button handles
        private void OnConnectRemotePressed(object sender, EventArgs e)
        {
            if (!ValidateRemoteAddress()) return;

            UpdateConnectionStatus();
            ConnectionData.EnableCheckTimer();

            Program.ClosePopup();
        }
        #endregion



        #region Net Helper
        private static bool ValidateRemoteAddress()
        {
            if (!IPEndPoint.TryParse(((InputField)Program.OpenPopups.Peek().Children.First((IRenderable rend) => rend.Id_Name == "IP")).Text, out IPEndPoint endpoint))
            {
                Program.PopupErrorMessage("Invalid IP and/or port.");
                return false;
            }

            ConnectionData.RemoteEndpoint = endpoint;

            return true;
        }
        private void OnConnectionCheck(object sender, EventArgs e)
        {
            try
            {
                TimeSpan timeout = new(0, 0, 0, 0, 300);
                if (!UpdateConnectionStatus()) return;
                UpdateBatchNumber(timeout);
                UpdateBatchProgress(timeout);
                UpdateRunStatus(timeout);
            }
            catch
            {
                ConnectionData.DisableCheckTimer();
            }
        }
        private bool PingService(TimeSpan timeout)
        {
            Stopwatch sw = Stopwatch.StartNew();
            bool ret = SendAndAwaitMessage(MessageBuilder.BuildPingMessage(), timeout, out byte[] _);
            sw.Stop();

            PingText.Text = $"Ping: {sw.Elapsed.Milliseconds}ms";

            return ret;
        }
        private bool UpdateConnectionStatus()
        {
            try
            {
                if (!PingService(new TimeSpan(0, 0, 0, 0, 300)))
                {
                    FailedConnection();
                    ConnectionStatusText.Text = "Failed to connect.";
                    return false;
                }
                else
                {
                    ConnectionStatusText.Text = "Connected.";
                    return true;
                }
            }
            catch
            {
                FailedConnection();
                ConnectionStatusText.Text = "Failed to connect.";
                return false;
            }
        }
        private void UpdateBatchNumber(TimeSpan timeout)
        {
            if (SendAndAwaitMessage(MessageBuilder.Message("req", string.Empty, "cbnum"), timeout, out byte[] response))
            {
                MessageBuilder.DeserializeMessage(response, out string messageType, out string target, out object value);
                if (!MessageBuilder.ValidateReturnMessage(messageType, target, value) || !((string)value).Contains("REQUEST_PASS:"))
                {
                    BatchNumberText.Text = "Batch XX";
                    return;
                }

                BatchNumberText.Text = "Batch " + ((string)value)[13..];
            }
            else
            {
                BatchNumberText.Text = "Batch XX";
                return;
            }
        }
        private void UpdateBatchProgress(TimeSpan timeout)
        {
            if (SendAndAwaitMessage(MessageBuilder.Message("req", string.Empty, "cbprog"), timeout, out byte[] response))
            {
                MessageBuilder.DeserializeMessage(response, out string messageType, out string target, out object value);
                if (!MessageBuilder.ValidateReturnMessage(messageType, target, value) || !((string)value).Contains("REQUEST_PASS:"))
                {
                    BatchProgressBar.Value = 0f;
                    return;
                }

                BatchProgressBar.Value = float.Parse(((string)value)[13..]);
            }
            else
            {
                BatchProgressBar.Value = 0f;
                return;
            }
        }
        private void UpdateRunStatus(TimeSpan timeout)
        {
            if (SendAndAwaitMessage(MessageBuilder.Message("req", string.Empty, "rstatus"), timeout, out byte[] response))
            {
                MessageBuilder.DeserializeMessage(response, out string messageType, out string target, out object value);
                if (!MessageBuilder.ValidateReturnMessage(messageType, target, value) || !((string)value).Contains("REQUEST_PASS:"))
                {
                    RunStatusText.Text = "Failed to check status.";
                    return;
                }

                RunStatusText.Text = ((string)value)[13..];
            }
            else
            {
                RunStatusText.Text = "Failed to check status.";
                return;
            }
        }

        private bool SendAndAwaitMessage(byte[] message, TimeSpan timeout, out byte[] response)
        {
            bool ret;
            response = null;

            try
            {
                TcpClient cli = new();
                if (ConnectionData.RemoteEndpoint == null)
                {
                    FailedConnection();
                    return false;
                }

                var result = cli.BeginConnect(ConnectionData.RemoteEndpoint.Address, ConnectionData.RemoteEndpoint.Port, null, null);

                var success = result.AsyncWaitHandle.WaitOne(timeout);
                MessageBuilder.SendMessage(message, cli);

                ret = MessageBuilder.ReceiveMessage(cli.GetStream(), out response, timeout);
                cli.Close();

                return ret;
            }
            catch
            {
                FailedConnection();
                return false;
            }
        }
        private void FailedConnection()
        {
            ConnectionData.DisableCheckTimer();
            ConnectionStatusText.Text = "Connection lost.";
        }
        #endregion
    }
}
