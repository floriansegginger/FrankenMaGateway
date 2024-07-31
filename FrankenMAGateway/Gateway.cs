using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FrankenMAGateway
{
    public struct FaderValue
    {
        public int value;
        public DateTime lastChange;
        public bool moving;
    }

    public class GatewaySettings
    {
        public string[,] Keys { get; set; } = new string[,]
        {
            { "Fix", "Select", "Off", "View", "Effect", "Goto", "Delete", "", "Blind" },
            { "Temp", "Top", "On", "Page", "Macro", "Preset", "Copy", "", "Freeze" },
            { "<<<", "Learn", ">>>", "Sequ", "Cue", "Exec", "", "", "Prvw" },
            { "Go -", "Pause", "Go +", "Channel", "Fixture", "Group", "Move", "Align", "Assign" },
            { "Time", "Esc", "7", "8", "9", "+", "Full", "Solo", "Highlt" },
            { "Edit", "Oops", "4", "5", "6", "Thru", "", "", "" },
            { "Update", "Clear", "1", "2", "3", "-", "", "", "Up" },
            { "", "", "0", ".", "If", "At", "Prev", "Next", "Set" },
            { "Store", "", "MA", "", "", "Please", "", "", "Down" }
        };

        public string[] SpecialKeys { get; set; } = new string[]
        {
            "Tools", "Setup", "Backup"
        };

        public string[] PortNames { get; set; } = new string[] { "COM8", "COM9", "COM11" };

        public int FaderBottomPage { get; set; } = 1;
        public int FaderBottomStartId { get; set; } = 1;
        public int FaderTopPage { get; set; } = 1;
        public int FaderTopStartId { get; set; } = 16;

        public int FaderButtonDmxUniverse { get; set; } = 10;
        public int FaderButtonDmxStartAddress { get; set; } = 1;

        public string User { get; set; } = "administrator";
        public string Password { get; set; } = "admin";
        public string Host { get; set; } = "localhost";
        public bool DebounceFaders { get; set; } = true;
    }

    public class Gateway : INotifyPropertyChanged
    {
        struct SerialPortParameters
        {
            public int portId;
            public string portName;
        }

        private List<SerialPort> _ports = new List<SerialPort>();
        private List<Thread> _threads = new List<Thread>();
        private bool _exiting = false;
        private object _lock = new object();

        private TcpClient _telnet;

        private Queue<string> _commandQueue = new Queue<string>();
        private AutoResetEvent _queueMutex = new AutoResetEvent(false);

        public int BaudRate { get; set; } = 115200;

        private bool[] portStatuses = { false, false, false, false };

        public string Port0Status => portStatuses[0] ? "OK" : "NOK";
        public string Port1Status => portStatuses[1] ? "OK" : "NOK";
        public string Port2Status => portStatuses[2] ? "OK" : "NOK";
        public string Port3Status => portStatuses[3] ? "OK" : "NOK";

        public bool[] PortStatuses
        {
            get => portStatuses;
        }

        public bool TelnetStatus
        {
            get { return _telnetStatus; }
            set
            {
                _telnetStatus = value;
                DoPropertyChanged(nameof(TelnetStatus));
            }
        }

        public GatewaySettings Settings { get; set; } = new GatewaySettings();

        public string[] PortNames { get => Settings.PortNames; }


        Dictionary<string, int> KeyCodes = new Dictionary<string, int>{
            {"Ch Pg +", 3},
            {"Ch Pg -", 4},
            {"Fd Pg +", 5},
            {"Fd Pg -", 6},
            {"Bt Pg +", 7},
            {"Bt Pg -", 8},
            {"X1", 12},
            {"X2", 13},
            {"X3", 14},
            {"X4", 15},
            {"X5", 16},
            {"X6", 17},
            {"X7", 18},
            {"X8", 19},
            {"X9", 20},
            {"X10", 21},
            {"X11", 22},
            {"X12", 23},
            {"X13", 24},
            {"X14", 25},
            {"X15", 26},
            {"X16", 27},
            {"X17", 28},
            {"X18", 29},
            {"X19", 30},
            {"X20", 31},
            {"List", 32},
            {"Fix", 41},
            {"Select", 42},
            {"Off", 43},
            {"Temp", 44},
            {"Top", 45},
            {"On", 46},
            {"<<<", 47},
            {"Learn", 48},
            {">>>", 49},
            {"Go -", 50},
            {"Pause", 51},
            {"Go +", 52},
            {"Oops", 53},
            {"Esc", 54},
            {"Edit", 55},
            {"Goto", 56},
            {"Update", 57},
            {"Time", 58},
            {"Store", 59},
            {"Blind", 60},
            {"Freeze", 61},
            {"Prvw", 62},
            {"Assign", 63},
            {"Align", 64},
            {"B.O.", 65},
            {"View", 66},
            {"Effect", 67},
            {"MA", 68},
            {"Delete", 69},
            {"Page", 70},
            {"Macro", 71},
            {"Preset", 72},
            {"Copy", 73},
            {"Sequ", 74},
            {"Cue", 75},
            {"Exec", 76},
            {"Channel", 82},
            {"Fixture", 83},
            {"Group", 84},
            {"Move", 85},
            {"0", 86},
            {"1", 87},
            {"2", 88},
            {"3", 89},
            {"4", 90},
            {"5", 91},
            {"6", 92},
            {"7", 93},
            {"8", 94},
            {"9", 95},
            {"+", 96},
            {"-", 97},
            {".", 98},
            {"Full", 99},
            {"Highlt", 100},
            {"Solo", 101},
            {"Thru", 102},
            {"If", 103},
            {"At", 104},
            {"Clear", 105},
            {"Please", 106},
            {"Up", 107},
            {"Set", 108},
            {"Prev", 109},
            {"Next", 110},
            {"Down", 111},
            {"Help", 116},
            {"Backup", 117},
            {"Setup", 118},
            {"Tools", 119},
            {"V1", 120},
            {"V2", 121},
            {"V3", 122},
            {"V4", 123},
            {"V5", 124},
            {"V6", 125},
            {"V7", 126},
            {"V8", 127},
            {"V9", 128},
            {"V10", 129}
        };

        public void Start()
        {

            JsonSerializer serializer = new JsonSerializer();

            using (StreamReader sr = new StreamReader(@"Settings.json"))
            using (JsonTextReader reader = new JsonTextReader(sr))
            {
                serializer.Formatting = Formatting.Indented;
                Settings = serializer.Deserialize<GatewaySettings>(reader);
            }

            int i = 0;
            foreach (var portName in PortNames)
            {
                var portThread = new Thread(new ParameterizedThreadStart(ComThread));
                portThread.Start(new SerialPortParameters() { portId = i++, portName = portName });
            }
            var telnetThread = new Thread(new ThreadStart(TelnetThread));
            telnetThread.Start();
        }

        private void ComThread(object parameters)
        {
            SerialPortParameters portParams = (SerialPortParameters)parameters;
            string portName = portParams.portName;
            int portId = portParams.portId;

            SerialPort port = null;

            while (!_exiting)
            {
                port = new SerialPort(portName);
                port.BaudRate = BaudRate;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.DataBits = 8;
                port.Handshake = Handshake.None;
                port.DtrEnable = true;
                port.NewLine = "\n";
                port.DataReceived += Port_DataReceived;
                try
                {
                    port.Open();
                    lock (PortStatuses)
                    {
                        PortStatuses[portId] = true;
                        lock (_receiveBuffer)
                        {
                            _receiveBuffer.Add(port, "");
                        }
                    }
                    break;
                }
                catch
                {
                    port.Dispose();
                    port = null;
                    lock (PortStatuses)
                    {
                        PortStatuses[portId] = false;
                    }
                    Thread.Sleep(2000);
                    continue;
                }
            }
            if (_exiting)
            {
                return;
            }
            PortStatuses[portId] = true;
            DoPropertyChanged(nameof(PortStatuses));
            DoPropertyChanged(nameof(Port0Status));
            DoPropertyChanged(nameof(Port1Status));
            DoPropertyChanged(nameof(Port2Status));

            Log($"Port {portName} ready!");
            while (!_exiting)
            {
                Thread.Sleep(5000);
            }
        }

        public string Logs
        {
            get { return _logs; }
            set
            {
                _logs = value;
                DoPropertyChanged(nameof(Logs));
            }
        }

        private void Log(string line)
        {
            Console.WriteLine(line);
            Logs = $"[{DateTime.Now.ToString("HH:mm:ss")}] {line}\r\n{Logs}";
            if (Logs.Length > 10000)
            {
                Logs = Logs.Substring(0, 10000);
            }
        }

        private void TelnetThread()
        {
            while (!_exiting)
            {
                try
                {
                    Log("Trying to connect...");
                    _telnet = new TcpClient(Settings.Host, 30000);
                }
                catch
                {
                    Thread.Sleep(2000);
                    _telnet = null;
                    continue;
                }
                var stream = _telnet.GetStream();
                var reader = new StreamReader(stream);
                var writer = new StreamWriter(stream);
                writer.NewLine = "\r\n";
                bool failed = false;
                int cnt = 0;
                while (!_exiting)
                {
                    cnt++;
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        failed = true;
                        break;
                    }
                    if (line.Contains("login !"))
                    {
                        break;
                    }
                    if (cnt >= 4)
                    {
                        break;
                    }
                }
                if (failed)
                {
                    Log("Failed, trying again...");
                    _telnet = null;
                    Thread.Sleep(2000);
                    continue;
                }
                while (!_exiting)
                {
                    Thread.Sleep(1000);
                    writer.WriteLine("login " + Settings.User + " " + Settings.Password);
                    writer.Flush();

                    string line = reader.ReadLine();
                    if (line.Contains("Logged in as"))
                    {
                        break;
                    }
                    if (cnt++ > 4)
                    {
                        break;
                    }
                }
                Log("Ready!");
                TelnetStatus = true;
                bool disconnected = false;
                while (!_exiting)
                {
                    _queueMutex.WaitOne(2000);
                    lock (_lock)
                    {
                        while (_commandQueue.Count > 0)
                        {
                            string command = _commandQueue.Dequeue();
                            try
                            {
                                writer.WriteLine(command);
                                writer.Flush();
                            }
                            catch
                            {
                                disconnected = true;
                                _telnet?.Close();
                                _telnet?.Dispose();
                                _telnet = null;
                                break;
                            }
                        }
                    }
                    if (disconnected)
                    {
                        TelnetStatus = false;
                        break;
                    }
                }
                if (disconnected)
                {
                    Log("Socket closed. Starting over.");
                    continue;
                }
            }
        }

        private FaderValue[] faderValues = new FaderValue[24];
        private bool _telnetStatus = false;
        private string _logs;

        private Dictionary<SerialPort, string> _receiveBuffer = new Dictionary<SerialPort, string>();

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string received = null;
            try
            {
                var port = sender as SerialPort;
                received = port.ReadExisting();

                string buffer = null;
                lock (_receiveBuffer)
                {
                    _receiveBuffer[port] += received;
                    buffer = _receiveBuffer[port];
                }
                if (buffer.Contains("\n"))
                {
                    var parts = buffer.Split('\n');
                    string leftInBuffer = "";
                    foreach (var part in parts)
                    {
                        try
                        {
                            OnPortData(part);
                            //Log(part.Replace("\n", ""));
                        } catch
                        {
                            //Log("PARTIAL: " + part.Replace("\n", ""));
                            leftInBuffer += part;
                        }
                    }
                    lock (_receiveBuffer)
                    {
                        _receiveBuffer[port] = leftInBuffer;
                    }
                }
            }
            catch (Exception exception)
            {
                Log("ERROR: " + received);
                Log(exception.ToString());
            }
        }

        private void OnPortData(string received)
        {

            var lines = received.Split(new string[] { "\r\n" }, StringSplitOptions.None);
            foreach (var line in lines)
            {
                if (line.StartsWith("KEY "))
                {
                    var parts = line.Split(' ');
                    string direction = parts[1];
                    int row = int.Parse(parts[2]);
                    int col = int.Parse(parts[3]);

                    string key = Settings.Keys[row, col];
                    int keyCode = KeyCodes[key];

                    string boolDirection = (direction == "UP") ? "false" : "true";

                    lock (_lock)
                    {
                        _commandQueue.Enqueue($"Lua \"gma.canbus.hardkey({keyCode},{boolDirection},false)\"");
                    }
                }

                if (line.StartsWith("SPECIAL_KEY "))
                {
                    var parts = line.Split(' ');
                    string direction = parts[1];
                    int row = int.Parse(parts[2]);

                    string key = Settings.SpecialKeys[row];
                    int keyCode = KeyCodes[key];

                    string boolDirection = (direction == "UP") ? "false" : "true";

                    lock (_lock)
                    {
                        _commandQueue.Enqueue($"Lua \"gma.canbus.hardkey({keyCode},{boolDirection},false)\"");
                    }
                }

                if (line.StartsWith("ENCODER "))
                {
                    var parts = line.Split(' ');
                    int id = int.Parse(parts[1]);
                    int amount = int.Parse(parts[2]);

                    lock (_lock)
                    {
                        _commandQueue.Enqueue($"Lua \"gma.canbus.encoder({id},{amount},pressed)\"");
                    }
                }

                if (line.StartsWith("FADER "))
                {
                    var parts = line.Split(' ');
                    int id = int.Parse(parts[1]);
                    int value = (int)Math.Round((double.Parse(parts[2]) / 1023.0) * 100.0);
                    byte byteValue = (byte)value;

                    int page = (id >= 12) ? Settings.FaderTopPage : Settings.FaderBottomPage;
                    int faderId = (id >= 12) ? (id - 12 + Settings.FaderTopStartId) : (id + Settings.FaderBottomStartId);


                    if (Math.Abs(faderValues[id].value - value) >= 2)
                    {
                        faderValues[id].moving = true;
                        faderValues[id].value = value;
                        faderValues[id].lastChange = DateTime.Now;
                    }
                    if (Math.Abs(faderValues[id].value - value) < 2)
                    {
                        var diff = DateTime.Now.Subtract(faderValues[id].lastChange);
                        if (diff > TimeSpan.FromMilliseconds(500))
                        {
                            faderValues[id].moving = false;
                        }
                    }

                    if (faderValues[id].moving || !Settings.DebounceFaders)
                    {
                        lock (_lock)
                        {
                            _commandQueue.Enqueue($"fader {page}.{faderId} at {value}");
                        }
                    }
                }

                if (line.StartsWith("FADER_BUTTON "))
                {
                    var parts = line.Split(' ');
                    string direction = parts[1];
                    int address = int.Parse(parts[2]) + Settings.FaderButtonDmxStartAddress;
                    int value = direction == "DOWN" ? 100 : 0;

                    lock (_lock)
                    {
                        _commandQueue.Enqueue($"dmx {Settings.FaderButtonDmxUniverse}.{address} at {value}");
                    }
                }
            }
            _queueMutex.Set();
        }

        public void Stop()
        {
            foreach (var port in _ports)
            {
                port.Close();
            }
            _exiting = true;
        }

        public List<string> GetPortNames()
        {
            return SerialPort.GetPortNames().ToList();
        }


        #region MVVM Boilerplate
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void DoPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
