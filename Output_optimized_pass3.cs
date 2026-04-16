using Mythosia;
using Mythosia.Integrity.CRC;
using System.Buffers.Binary;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;

namespace Mover.UI
{
    public class OUTPUT : MODULE
    {
        private readonly string binaryTooltip =
                "In binary format bit values are sent with the number of bytes specified for the actuators\n" +
                "The other values/variables (like mm, º or turns) are sent as a 4 byte float or a 8 byte double\n" +
                "Using a letter or any other key sends it's ASCII value as a byte\n" +
                "Examples: \n" +
                "<RIG.P.YAW> is a 4 byte float\n" +
                "<<RIG.P.YAW> is a 8 byte double\n" +
                "A<RIG.P.YAW> is the byte 65 for letter A followed by 4 bytes for the float value\n" +
                "You can also send specific values:\n" +
                "<233> is the byte 233\n" +
                "233 is not the byte 233, but 3 bytes for the chars specified: 50, 51, 51\n" +
                "<23464> does not fit in one byte, the byte shown is only the least significant byte for that value: 168\n" +
                "<<23464> fits in two bytes and shows with two bytes: 91, 168\n" +
                "<<<23464> shows with 3 bytes: 0, 91, 168\n" +
                "<123.45> contains a number with decimals and due to the one < it will be converted to 4 bytes (float)\n" +
                "<<123.45> contains a number with decimals and due to the two <, it will be converted to 8 bytes (double)\n" +
                "Resuming, the number of < used in the variables and values format the output by defining the amount of bytes used\n" +
                "The values will be converted to byte arrays in the selected endianness";

        private readonly string asciiTooltip =
                "In ASCII format you can send invisible chars sending their value:\n" +
                "<27> is the invisible ASCII char ESCAPE\n" +
                "<64> is the visible char @\n" +
                "Visible chars can be set directly\n" +
                "You can also set the minimum amount of digits shown before the decimal dot and the number of decimals shown\n" +
                "If the variable value is 2.234456 then:\n" +
                "<VARIABLE> shows the variable value as an int: 2\n" +
                "<<<VARIABLE>>> shows the variable value with at least 3 digits before the dot and 2 decimal places: 002.23\n" +
                "<<<VARIABLE> shows the variable value with at least 3 digits and no decimal places: 002\n" +
                "Resuming, the number of < used in the variables set the minimum number of digits shown before the decimal dot and the number of > used (minus one) is the number of decimals shown after the dot";

        // Controls
        protected BTN BTNConnectDisconnect = new()
        {
            Name = "BTNConnectDisconnect",
            Text = "Connect",
        };
        protected CHK CHKAutoConnect = new()
        {
            Name = "CHKAutoConnect",
        };
        protected LBL LBLInterval = new()
        {
            Text = "Interval loops",
            TextAlign = ContentAlignment.MiddleRight,
        };
        protected NUD NUDInterval = new()
        {
            Name = "NUDInterval",
            Minimum = 1,
            Maximum = 1000,
            Value = 1,
        };
        protected LBL LBLIntervalMS = new()
        {
            TextAlign = ContentAlignment.MiddleLeft,
        };
        protected LBL LBLType = new()
        {
            Text = "Type",
            TextAlign = ContentAlignment.MiddleRight,
        };        
        protected DDL DDLType = new()
        {
            Name = "DDLType",
            ItemsList = [
                "Binary BE",
                "Binary LE",
                "ASCII",
                ],
            ItemsToolTipsList = [
                "Binary format using big-endian where the most significant byte comes first",
                "Binary format using little-endian where the least significant byte comes first",
                "ASCII format",
                ],
            SelectedIndex = 0,
            AllowInvalidText = false,
        };
        protected LBL LBLStart = new()
        {
            Text = "Start",
            TextAlign = ContentAlignment.MiddleRight,
        };
        protected CHK CHKStart = new()
        {
            Name = "CHKStart",
        };
        protected TXT TXTStart = new()
        {
            Name = "TXTStart",
            AllowDrop = true,
        };
        protected NUD NUDStart = new()
        {
            Name = "NUDStart",
            Minimum = 0,
            Value = 10,
        };
        protected LBL LBLStartMS = new()
        {
            Text = "ms",
            TextAlign = ContentAlignment.MiddleLeft,
        };
        protected LBL LBLSend = new()
        {
            Text = "Send",
            TextAlign = ContentAlignment.MiddleRight,
        };
        protected CHK CHKSend = new()
        {
            Name = "CHKSend",
            Checked = true,
        };
        protected TXT TXTSend = new()
        {
            Name = "TXTSend",
            AllowDrop = true,
        };
        protected LBL LBLStop = new()
        {
            Text = "Stop",
            TextAlign = ContentAlignment.MiddleRight,
        };
        protected CHK CHKStop = new()
        {
            Name = "CHKStop",
        };
        protected TXT TXTStop = new()
        {
            Name = "TXTStop",
            AllowDrop = true,
        };
        protected NUD NUDStop = new()
        {
            Name = "NUDStop",
            Minimum = 0,
            Value = 10,
        };
        protected LBL LBLStopMS = new()
        {
            Text = "ms",
            TextAlign = ContentAlignment.MiddleLeft,
        };
        protected LBL LBLCRC = new()
        {
            Text = "Add CRC",
            TextAlign = ContentAlignment.MiddleRight,
        };
        protected DDL DDLCRC = new()
        {
            Name = "DDLCRC",
            ItemsList = [
                "None",
                "CRC-8",
                "CRC - 16",
                "CRC - 16 Modbus",
                "CRC - 16 CCITT",
                "CRC - 32",
                ],
            ItemsToolTipsList = [
                "Do not add CRC at the end of the data",
                "The classic CRC-8 (adds 1 byte at the end of the data)",
                "The classic CRC-16 (adds 2 bytes at the end of the data)",
                "The CRC-16 used in Modbus (adds 2 bytes at the end of the data)",
                "The CRC-16 used in Bluetooth (adds 2 bytes at the end of the data)",
                "The classic CRC-32 (adds 4 bytes at the end of the data)",
                ],
            SelectedIndex = 0,
        };
        protected SH SHValues = new()
        {
            Name = "SHValues",
            Direction = SH.ShowHideDirection.Right,
            MinimumTabSize = 100,
            MaximumTabSize = 1000,
            CurrentTabSize = 200,
        };
        protected TV TVValues = new()
        {
            Name = "TVValues",
        };
        protected SH SHDebug = new()
        {
            Name = "SHDebug",
            Direction = SH.ShowHideDirection.Bottom,
            MinimumTabSize = 128,
            MaximumTabSize = 128,
            CurrentTabSize = 128,
        };
        protected PNL PNLDebug = new()
        {
            Name = "PNLDebug",
        };
        internal HLP HLPHelp = new()
        {
            HelpLink = "https://www.flyptmover.com/",
        };
        protected BTN BTNPauseContinueOutput = new()
        {
            Text = "❚❚",
        };
        protected DDL DDLDebugType = new()
        {
            Name = "DDLDebugType",
            ItemsList = [
                "Show the bytes in decimal format",
                "Show the bytes in hexadecimal format",
                "Show the bytes in ASCII format"
                ],
            ItemsToolTipsList = [
                "Show byte values in decimal format, sent through the output",
                "Show byte values in hex format, sent through the output",
                "Show the ASCII values sent through the output (bytes without char representation are shown as <byte value>)"
                ],
            SelectedIndex = 0,
        };
        protected LBL LBLStartDebugDebug = new()
        {
            Text = "Stop",
            TextAlign = ContentAlignment.MiddleRight,
        };
        protected LBL LBLStartDebug = new()
        {
            TextAlign = ContentAlignment.MiddleLeft,
        };
        protected LBL LBLStartTT = new()
        {
            Text = "...",
        };
        protected LBL LBLSendDebugDebug = new()
        {
            Text = "Send",
            TextAlign = ContentAlignment.MiddleRight,
        };
        protected LBL LBLSendDebug = new()
        {
            TextAlign = ContentAlignment.MiddleLeft,
        };
        protected LBL LBLSendTT = new()
        {
            Text = "...",
        };
        protected LBL LBLStopDebugDebug = new()
        {
            Text = "Start",
            TextAlign = ContentAlignment.MiddleRight,
        };
        protected LBL LBLStopDebug = new()
        {
            TextAlign = ContentAlignment.MiddleLeft,
        };
        protected LBL LBLStopTT = new()
        {
            Text = "...",
        };

        internal int CRCType = 0; // None
        private int interval = 1; // Calculation step interval
        private int intervalCounter = 0; // Calculation step interval counter
        protected bool ForcedDisconnect = false; // True when we disconnect due to an error (keeps the auto connect check)
        private PacketPlan startPlan = PacketPlan.Empty; // Precompiled plan used to generate the start byte array
        private PacketPlan sendPlan = PacketPlan.Empty; // Precompiled plan used to generate the send byte array
        private PacketPlan stopPlan = PacketPlan.Empty; // Precompiled plan used to generate the stop byte array
        private byte[] startArray = []; // The array of bytes generated by the start string
        private byte[] sendArray = []; // The array of bytes generated by the send string
        private byte[] stopArray = []; // The array of bytes generated by the stop string
        protected int typeOfOutput = 0;
        private bool bigEndian = true;
        private int startDuration = 0;
        private int stopDuration = 0;
        public bool useStart = false;
        public bool useSend = true;
        public bool useStop = false;
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public double TransitionPercent { get; protected set; } = 0.0;
        private double lastTransitionPercent = 0.0;
        public OutputState State = OutputState.DISCONNECTED;
        private OutputState lastState = OutputState.DISCONNECTED;
        private bool updateStartToolTip = false;
        private bool updateSendToolTip = false;
        private bool updateStopToolTip = false;

        /// <summary>
        /// Pause sending data out
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        internal bool Paused { get; set; } = false;

        /// <summary>
        /// Allow/Disallow adding CRC
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool AllowCRC
        {
            set
            {
                this.LBLCRC.Dispose();
                this.DDLCRC.Dispose();
            }
        }

        /// <summary>
        /// Connection state of an output
        /// </summary>
        public enum OutputState
        {
            DISCONNECTED,   // Not connected (transition at 0%)
            CONNECTING,     // Connected and transitioning to get full value (transition going from 0% to 100%), sending the send array
            SEND_START,     // Connected, sending the start array
            CONNECTED,      // Connected (transition at 100%), sending the send array
            DISCONNECTING,  // Connected and transitioning to no value (transition going from 100% to 0%), sending the send array
            SEND_STOP       // Connected, sending the stop array
        }

        /// <summary>
        /// Timer to perform auto connection when it reaches the AUTO_CONNECT_TIME constant
        /// </summary>
        protected double AutoConnectTime = 0.0;

        /// <summary>
        /// Current transition time
        /// </summary>
        protected double CurrentTransitionTime = 0.0;

        /// <summary>
        /// Clock used to perform the transition
        /// </summary>
        protected Stopwatch TransitionClock = new();

        /// <summary>
        /// Clock used for the start/stop timer
        /// </summary>
        protected Stopwatch StartStopClock = new();

        /// <summary>
        /// Is the output connected
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public bool Connected
        {
            get => this.State != OutputState.DISCONNECTED;
            set
            {
                if (value && this.State == OutputState.DISCONNECTED) this.Connect();
                else if (!value && this.State != OutputState.DISCONNECTED) this.Disconnect();
            }
        }

        /// <summary>
        /// To get the connection color state for main buttons
        /// </summary>
        internal override Color? ConnectionInfoColor => this.BTNConnectDisconnect.BackColor;

        /// <summary>
        /// Constructor
        /// </summary>
        public OUTPUT()
        {
            // Initialize sizes
            this.ModuleRectangle = new(752, 330);
            // Tabs
            this.SHDebug.AssociatedControl = this.PNLDebug;
            this.SHValues.AssociatedControl = this.TVValues;
            ResizableHorizontally = true;
            ResizableVertically = false;
            this.ModuleRectangleFollowsRightBorder = false;
            this.ModuleRectangleFollowsBottomBorder = false;


            // Add controls
            this.Controls.AddRange([
                CHKAutoConnect ,
                BTNConnectDisconnect,
                BTNPauseContinueOutput,
                LBLInterval ,
                NUDInterval ,
                LBLIntervalMS ,
                LBLType,
                DDLType ,
                LBLStart ,
                CHKStart ,
                TXTStart ,
                NUDStart ,
                LBLStartMS ,
                LBLSend ,
                CHKSend,
                TXTSend,
                LBLStop,
                CHKStop,
                TXTStop,
                NUDStop,
                LBLStopMS,
                LBLCRC,
                DDLCRC,
                SHValues,
                TVValues,
                SHDebug,
                HLPHelp,
                PNLDebug,
            ]);
            this.PNLDebug.Controls.AddRange([
                DDLDebugType,
                LBLStartDebug,
                LBLStartDebugDebug,
                LBLStartTT,
                LBLSendDebug,
                LBLSendDebugDebug,
                LBLSendTT,
                LBLStopDebug,
                LBLStopDebugDebug,
                LBLStopTT,
            ]);

            // Update interval label
            UpdateIntervalInfo();

            // Controls events
            BTNConnectDisconnect.Click += (s, e) => { this.TryToConnectDisconnect(); };
            NUDInterval.ValueChanged += (s, e) =>
            {
                lock (Program.Locker)
                {
                    this.interval = (int)this.NUDInterval.Value;
                    this.UpdateIntervalInfo();
                }
            };
            CHKStart.CheckedChanged += (s,e) => { lock (Program.Locker) this.useStart = this.CHKStart.Checked; };
            CHKSend.CheckedChanged += (s, e) => { lock (Program.Locker) this.useSend = this.CHKSend.Checked; };
            CHKStop.CheckedChanged += (s, e) => { lock (Program.Locker) this.useStop = this.CHKStop.Checked; };
            DDLType.SelectedIndexChanged += (s, e) => {
                this.typeOfOutput = this.DDLType.SelectedIndex;
                this.bigEndian = this.typeOfOutput == 0;
                Program.Tip.SetToolTip(TXTStart,
                    "String sent to the output at the start of communication\n" +
                    "Values are updated and sent, only for the defined time after starting the output\n\n" + (this.typeOfOutput != 2 ? "You selected the binary format:\n" + binaryTooltip : "You selected the ASCII format:\n" + asciiTooltip));
                Program.Tip.SetToolTip(TXTSend,
                    "String sent to the output while it's connected\n" +
                    "This is sent after the Start and before the Stop, while the output is connected\n\n" + (this.typeOfOutput != 2 ? "You selected the binary format:\n" + binaryTooltip : "You selected the ASCII format:\n" + asciiTooltip));
                Program.Tip.SetToolTip(TXTStop,
                    "String sent to the output at the end of communication\n" +
                    "Values are updated and sent, only for the defined time after stopping the output\n\n" + (this.typeOfOutput != 2 ? "You selected the binary format:\n" + binaryTooltip : "You selected the ASCII format:\n" + asciiTooltip));
                UpdateStartArray();
                UpdateSendArray();
                UpdateStopArray();
            };
            NUDStart.ValueChanged += (s, e) =>
            {
                lock (Program.Locker) this.startDuration = (int)this.NUDStart.Value;
            };
            NUDStop.ValueChanged += (s, e) => {
                lock (Program.Locker) this.stopDuration = (int)this.NUDStop.Value;
            };
            DDLCRC.SelectedIndexChanged += (s,e)=> {
                lock (Program.Locker) this.CRCType = this.DDLCRC.SelectedIndex;
            };
            BTNPauseContinueOutput.Click += (s,e)=>
            {
                if (this.BTNPauseContinueOutput.Text == "❚❚")
                {
                    lock (Program.Locker) this.Paused = true;
                    this.BTNPauseContinueOutput.Text = "▶";
                }
                else
                {
                    lock (Program.Locker) this.Paused = false;
                    this.BTNPauseContinueOutput.Text = "❚❚";
                }
            };

            


            // Treat Drag from tree view
            TXTStart.TextChanged += (s,e) => UpdateStartArray();
            TXTStart.DragDrop += TXT_DragDrop;
            TXTStart.DragEnter += TXT_DragEnter;
            TXTStart.DragOver += TXT_DragOver;

            TXTSend.TextChanged += (s,e) => UpdateSendArray();
            TXTSend.DragDrop += TXT_DragDrop;
            TXTSend.DragEnter += TXT_DragEnter;
            TXTSend.DragOver += TXT_DragOver;

            TXTStop.TextChanged += (s,e) => UpdateStopArray();
            TXTStop.DragDrop += TXT_DragDrop;
            TXTStop.DragEnter += TXT_DragEnter;
            TXTStop.DragOver += TXT_DragOver;

            TVValues.ItemDrag += (s,e)=> {
                if (s is string text)
                {
                    var keyText = text;
                    this.DoDragDrop(keyText, DragDropEffects.Copy);
                }
            };

            // Create tooltip to show full output
            LBLStartTT.MouseEnter += LBLTT_MouseEnter;
            LBLStartTT.MouseLeave += LBLTT_MouseLeave;
            LBLSendTT.MouseEnter += LBLTT_MouseEnter;
            LBLSendTT.MouseLeave += LBLTT_MouseLeave;
            LBLStopTT.MouseEnter += LBLTT_MouseEnter;
            LBLStopTT.MouseLeave += LBLTT_MouseLeave;

            // Controls tooltips
            Program.Tip.SetToolTip(BTNConnectDisconnect, "Connect/Disconnect this output");
            Program.Tip.SetToolTip(CHKAutoConnect, "Auto connect output when possible");

            Program.Tip.SetToolTip(DDLType, "Binary format:\n" + binaryTooltip + "\n\nASCII format:\n" + asciiTooltip);
            
            Program.Tip.SetToolTip(NUDInterval, "Calculation loops between each send\nIf you set 10, 10 calculation loops are performed and data will be sent at the end of the 10th loop\nIf each loop uses 2 ms it means the time between sends would be 2*10 = 20 ms");
            Program.Tip.SetToolTip(NUDStart, "Amount of milliseconds for which the start string is sent to the hardware\nIt will be sent at least one time\nThe amount of times it's sent, depends on the selected time and the selected interval loop\nFor 10 ms interval loop, you need to set more than 10 ms time to send the string at least two times");
            Program.Tip.SetToolTip(NUDStop, "Amount of milliseconds for which the stop string is sent to the hardware\nIt will be sent at least one time\nThe amount of times it's sent, depends on the selected time and the selected interval loop\nFor 10 ms interval loop, you need to set more than 10 ms time to send the string at least two times");
            Program.Tip.SetToolTip(LBLStartDebug, "Preview the data sent to the hardware when the connection starts\nThere's no transition here, this is intended for hardware that needs a specific initialization message\nThis data is sent at least one time and is repeated in interval loops for the time set for the start\nFor binary output it shows the array of bytes sent\nFor decimal output it shows the string sent to the hardware\nIn decimal some chars have no representation, in that case the value of the char is shown\nFor example char 23 is shown as <23> instead of a letter");
            Program.Tip.SetToolTip(LBLSendDebug, "Preview the data sent to the hardware\nFor binary output it shows the array of bytes sent\nFor decimal output it shows the string sent to the hardware\nIn decimal some chars have no representation, in that case the value of the char is shown\nFor example char 23 is shown as <23> instead of a letter");
            Program.Tip.SetToolTip(LBLStopDebug, "Preview the data sent to the hardware when the connection stops\nThere's no transition here, this is intended for hardware that needs a specific termination message\nThis data is sent at least one time and is repeated in interval loops for the time set for the stop\nFor binary output it shows the array of bytes sent\nFor decimal output it shows the string sent to the hardware\nIn decimal some chars have no representation, in that case the value of the char is shown\nFor example char 23 is shown as <23> instead of a letter");
            Program.Tip.SetToolTip(TXTStart, 
                "String sent to the output at the start of communication\n" +
                "Values are updated and sent, only for the defined time after starting the output\n\nYou selected the binary format:\n" + binaryTooltip);
            Program.Tip.SetToolTip(TXTSend, 
                "String sent to the output while it's connected\n" +
                "This is sent after the Start and before the Stop, while the output is connected\n\nYou selected the binary format:\n" + binaryTooltip);
            Program.Tip.SetToolTip(TXTStop, 
                "String sent to the output at the end of communication\n" +
                "Values are updated and sent, only for the defined time after stopping the output\n\nYou selected the binary format:\n" + binaryTooltip);
            Program.Tip.SetToolTip(CHKStart, "Check to send the start data");
            Program.Tip.SetToolTip(CHKSend, "Check to send data");
            Program.Tip.SetToolTip(CHKStop, "Check to send the stop data");
            Program.Tip.SetToolTip(DDLCRC, "Select a CRC (cyclic redundancy check) to add bytes at the end of the packets for data corruption checking");
            Program.Tip.SetToolTip(SHValues, "Show/Hide the list of variables you can use in the strings\nYou can drag and drop the values in the Start/Send/Stop strings");
            Program.Tip.SetToolTip(HLPHelp, "Help");
            Program.Tip.SetToolTip(SHDebug, "Show/Hide the debug");
            Program.Tip.SetToolTip(BTNPauseContinueOutput, "Pause/Continue the output\nUse it for debugging and see the last data sent\nWhile paused, nothing is sent through the output");
            Program.Tip.SetToolTip(LBLStartTT, "Start is sending 0 bytes:\n");
            Program.Tip.SetToolTip(LBLSendTT, "Sending 0:\n");
            Program.Tip.SetToolTip(LBLStopTT, "Stop is sending 0:\n");
            Program.Tip.SetToolTip(TVValues, "Values you can add to the output start, send and stop strings\nYou can drag and drop them on the strings");
        }

        internal void UpdateStartArray()
        {
            lock (Program.Locker)
            {
                this.startPlan = this.BuildPacketPlanFromString(this.TXTStart.Text);
                this.startArray = this.GetByteArray(this.startPlan);
            }
        }

        internal void UpdateSendArray()
        {
            lock (Program.Locker)
            {
                this.sendPlan = this.BuildPacketPlanFromString(this.TXTSend.Text, true);
                this.sendArray = this.GetByteArray(this.sendPlan);
            }
        }

        internal void UpdateStopArray()
        {
            lock (Program.Locker)
            {
                this.stopPlan = this.BuildPacketPlanFromString(this.TXTStop.Text);
                this.stopArray = this.GetByteArray(this.stopPlan);
            }
        }

        /// <summary>
        /// Generate list of variables we can put in the outputs
        /// When updating the list, we want to keep the list position, the expanded nodes and the checked nodes
        /// </summary>
        public override void UpdateLocalLists()
        {
            // Get current state of tree view (expanded nodes, checked nodes and top node / vertical position)
            this.TVValues.StoreState();
            // Clear the list in the UI
            this.TVValues.Items.Clear();
            // Add Mover info
            var n = this.TVValues.AddItem("Mover");
            n.AddItem("Pretended calculation time (ms) <MOVER.PCTIME>");
            n.AddItem("Obtained calculation time (ms) <MOVER.OCTIME>");
            n.AddItem("User interface update time (ms) <MOVER.UITIME>");
            n.AddItem("Absolute Mover time (ms) <MOVER.TIME>");
            n.AddItem("Calculation counter (8 bits) <MOVER.COUNTER8>");
            n.AddItem("Calculation counter (16 bits) <MOVER.COUNTER16>");
            n.AddItem("Calculation counter (32 bits) <MOVER.COUNTER32>");
            // Add sources
            if (Application.OpenForms.OfType<UI.SOURCE>().Any())
            {
                n = this.TVValues.AddItem("Sources");
                foreach (var source in Application.OpenForms.OfType<UI.SOURCE>())
                {
                    var nn = n.AddItem(source.Name + " <" + source.Key + ">");
                    if (source is Sources.LoopAndNoise)
                    {
                        nn.AddItem("Value of the wave <" + source.Key + ".WV>");
                        nn.AddItem("Value of the noise <" + source.Key + ".NV>");
                        nn.AddItem("Value before filtering <" + source.Key + ".V>");
                        nn.AddItem("Value after filtering <" + source.Key + ".VF>");
                        var nnn = nn.AddItem("Output value (after transition)");
                        foreach (var i in source.SelectedNumerics)
                        {
                            nnn.AddItem(UI.SOURCE.NumericsNames[i] + " <" + source.Key + "." + UI.SOURCE.NumericsKeys[i] + ">");
                        }
                    }
                    else
                    {
                        foreach (var i in source.SelectedNumerics)
                        {
                            nn.AddItem(UI.SOURCE.NumericsNames[i] + " <" + source.Key + "." + UI.SOURCE.NumericsKeys[i] + ">");
                        }
                    }
                }
            }
            // Add poses
            if (Application.OpenForms.OfType<UI.POSE>().Any())
            {
                n = this.TVValues.AddItem("Poses");
                foreach (var pose in Application.OpenForms.OfType<UI.POSE>())
                {
                    var nn = n.AddItem(pose.Name + " <" + pose.Key + ">");
                    nn.AddItem("Sway <" + pose.Key + ".SWAY>");
                    nn.AddItem("Surge <" + pose.Key + ".SURGE>");
                    nn.AddItem("Heave <" + pose.Key + ".HEAVE>");
                    nn.AddItem("Yaw <" + pose.Key + ".YAW>");
                    nn.AddItem("Pitch <" + pose.Key + ".PITCH>");
                    nn.AddItem("Roll <" + pose.Key + ".ROLL>");
                }
            }
            // Add directs
            if (Application.OpenForms.OfType<UI.DIRECT>().Any())
            {
                n = this.TVValues.AddItem("Directs");
                foreach (var direct in Application.OpenForms.OfType<UI.DIRECT>())
                {
                    var nn = n.AddItem(direct.Name + " <" + direct.Key + ">");
                    nn.AddItem("Actuator position before filtering and cropping (mm or º) <" + direct.Key + ".PBFC>");
                    nn.AddItem("Actuator position before cropping (mm or º) <" + direct.Key + ".PBC>");
                    nn.AddItem("Actuator position (mm or º) <" + direct.Key + ".P>");
                    nn.AddItem("Actuator bits <" + direct.Key + ".B>");
                    nn.AddItem("Actuator motor turns <" + direct.Key + ".T>");
                }
            }
            // Add rigs
            if (Application.OpenForms.OfType<UI.RIG>().Any())
            {
                n = this.TVValues.AddItem("Rigs");
                foreach (var rig in Application.OpenForms.OfType<UI.RIG>())
                {
                    var nn = n.AddItem(rig.Name + " <" + rig.Key + ">");
                    // Actuators
                    int actuatorNumber = 1;
                    foreach (var actuator in rig.RigActuators)
                    {
                        var nnn = nn.AddItem("Actuator " + actuatorNumber.ToString() + " <" + rig.Key + "." + actuator.Key + ">");
                        nnn.AddItem("Position before filtering and cropping (mm or º) <" + rig.Key + "." + actuator.Key + ".PBFC>");
                        nnn.AddItem("Position before cropping (mm or º) <" + rig.Key + "." + actuator.Key + ".PBC>");
                        nnn.AddItem("Position (mm or º) <" + rig.Key + "." + actuator.Key + ".P>");
                        nnn.AddItem("Bits <" + rig.Key + "." + actuator.Key + ".B>");
                        nnn.AddItem("Motor turns <" + rig.Key + "." + actuator.Key + ".T>");
                        actuatorNumber++;
                    }
                    // Poses
                    var nnnbfc = nn.AddItem("Pose before filtering and cropping <" + rig.Key + ".PBFC>");
                    nnnbfc.AddItem("Sway <" + rig.Key + ".PBFC.SWAY>");
                    nnnbfc.AddItem("Surge <" + rig.Key + ".PBFC.SURGE>");
                    nnnbfc.AddItem("Heave <" + rig.Key + ".PBFC.HEAVE>");
                    nnnbfc.AddItem("Yaw <" + rig.Key + ".PBFC.YAW>");
                    nnnbfc.AddItem("Pitch <" + rig.Key + ".PBFC.PITCH>");
                    nnnbfc.AddItem("Roll <" + rig.Key + ".PBFC.ROLL>");

                    var nnnbc = nn.AddItem("Pose before cropping <" + rig.Key + ".PBC>");
                    nnnbc.AddItem("Sway <" + rig.Key + ".PBC.SWAY>");
                    nnnbc.AddItem("Surge <" + rig.Key + ".PBC.SURGE>");
                    nnnbc.AddItem("Heave <" + rig.Key + ".PBC.HEAVE>");
                    nnnbc.AddItem("Yaw <" + rig.Key + ".PBC.YAW>");
                    nnnbc.AddItem("Pitch <" + rig.Key + ".PBC.PITCH>");
                    nnnbc.AddItem("Roll <" + rig.Key + ".PBC.ROLL>");

                    var nnnp = nn.AddItem("Pose <" + rig.Key + ".P>");
                    nnnp.AddItem("Sway <" + rig.Key + ".P.SWAY>");
                    nnnp.AddItem("Surge <" + rig.Key + ".P.SURGE>");
                    nnnp.AddItem("Heave <" + rig.Key + ".P.HEAVE>");
                    nnnp.AddItem("Yaw <" + rig.Key + ".P.YAW>");
                    nnnp.AddItem("Pitch <" + rig.Key + ".P.PITCH>");
                    nnnp.AddItem("Roll <" + rig.Key + ".P.ROLL>");
                }
            }
            // Add transducers
            if (Application.OpenForms.OfType<UI.TRANSDUCER>().Any())
            {
                n = this.TVValues.AddItem("Transducers");
                foreach (var transducer in Application.OpenForms.OfType<Transducers.SingleWave>())
                {
                    var nn = n.AddItem(transducer.Name + " <" + transducer.Key + ">");
                    nn.AddItem("Input value <" + transducer.Key + ".IN>");
                    nn.AddItem("Frequency (Hz) <" + transducer.Key + ".F>");
                    nn.AddItem("Amplitude (0.0 to 1.0) <" + transducer.Key + ".A>");
                    nn.AddItem("Noise (0.0 to 1.0) <" + transducer.Key + ".N>");
                    nn.AddItem("Frequency bits <" + transducer.Key + ".F.B>");
                    nn.AddItem("Amplitude bits <" + transducer.Key + ".A.B>");
                    nn.AddItem("Noise bits <" + transducer.Key + ".N.B>");
                }
            }
            // Add joysticks
            if (Devices.Joysticks.Devices.Count != 0)
            {
                n = this.TVValues.AddItem("Joysticks");
                foreach (var joystick in Devices.Joysticks.Devices)
                {
                    var nn = n.AddItem(joystick.Name + " <J." + joystick.Key + ">");
                    for (int i = 0; i < Devices.Joysticks.ControlsNames.Count; i++)
                    {
                        if (joystick.Exists[i]) nn.AddItem(Devices.Joysticks.ControlsNames[i] + " <J." + joystick.Key + "." + Devices.Joysticks.ControlsKeys[i] + ">");
                    }
                }
            }
            // Add midis
            if (Devices.Midi.Devices.Count != 0)
            {
                n = this.TVValues.AddItem("Midis");
                foreach (var midi in Devices.Midi.Devices)
                {
                    var nn = n.AddItem(midi.Name + " <M." + midi.Key + ">");
                    for (int i = 0; i < Devices.Joysticks.ControlsNames.Count; i++)
                    {
                        if (midi.Exists[i]) nn.AddItem(Devices.Midi.ControlsNames[i] + " <M." + midi.Key + "." + Devices.Midi.ControlsKeys[i] + ">");
                    }
                }
            }
            // Restore state of tree view (expanded nodes, checked nodes and top node / vertical position)
            this.TVValues.RestoreState();
        }

        /// <summary>
        /// Drag key inside start/send/stop TXT
        /// </summary>
        protected void TXT_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(typeof(string)) && sender is TXT txt)
            {
                e.Effect = DragDropEffects.Copy;
                txt.Focus();
            }
        }

        /// <summary>
        /// Drag key over start/send/stop TXT
        /// </summary>
        protected void TXT_DragOver(object? sender, DragEventArgs e)
        {
            if (sender is TXT txt)
            {
                var capturedPoint = txt.PointToClient(Control.MousePosition);
                int cp = txt.GetCharIndexFromPosition(capturedPoint);
                if (txt.GetPositionFromCharIndex(txt.Text.Length).X < capturedPoint.X) cp++;
                txt.SelectionStart = cp;
                txt.SelectionLength = 0;
                txt.Refresh();
            }
        }

        /// <summary>
        /// Drop a dragged key in the start/send/stop TXT
        /// </summary>
        protected void TXT_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data != null && e.Data.GetDataPresent(typeof(string)) && sender is TXT txt)
            {
                var Item = e.Data.GetData(typeof(string)) as string;
                if (Item != null && Item.IndexOf('<') >= 0)
                {
                    Item = Item[Item.IndexOf('<')..];
                    txt.SelectionLength = 0;
                    txt.SelectedText = Item;
                }
            }
        }

        /// <summary>
        /// Generate a byte array from a precompiled packet plan
        /// </summary>
        /// <param name="plan">The precompiled plan</param>
        /// <returns>The generated byte array</returns>
        protected virtual byte[] GetByteArray(PacketPlan plan)
        {
            var data = plan.Build(this.bigEndian);
            if (data.Length == 0) return EmptyBytes;

            return this.CRCType switch
            {
                1 => [.. data.WithCRC8(CRC8Type.Classic)],
                2 => [.. data.WithCRC16(CRC16Type.Classic)],
                3 => [.. data.WithCRC16(CRC16Type.Modbus)],
                4 => [.. data.WithCRC16(CRC16Type.CCITTxModem)],
                5 => [.. data.WithCRC32(CRC32Type.Classic)],
                _ => data,
            };
        }

        /// <summary>
        /// Convert a long to bytes in big-endian depending on number of bits used
        /// </summary>
        /// <param name="value">Value we want to convert to an array of bytes</param>
        /// <param name="bits">The number of bits to use</param>
        /// <returns>Array of bytes</returns>
        protected static byte[] ConvertToBytesBE(long value, byte bits)
        {
            int length = GetOutputByteCount(bits);
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                int shift = (length - 1 - i) * 8;
                bytes[i] = (byte)((value >> shift) & 0xff);
            }
            return bytes;
        }

        /// <summary>
        /// Convert a long to bytes in little-endia order depending on number of bits used
        /// </summary>
        /// <param name="value">Value we want to convert to an array of bytes</param>
        /// <param name="bits">The number of bits to use</param>
        /// <returns>Array of bytes</returns>
        protected static byte[] ConvertToBytesLE(long value, byte bits)
        {
            int length = GetOutputByteCount(bits);
            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
            {
                bytes[i] = (byte)((value >> (i * 8)) & 0xff);
            }
            return bytes;
        }

        /// <summary>
        /// Convert float to 4 bytes in little-endian order
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Array of bytes</returns>
        protected static byte[] ConvertToBytesBE(float value)
        {
            byte[] bytes = [0, 0, 0, 0];
            BinaryPrimitives.WriteSingleBigEndian(bytes, value);
            return bytes;
        }

        /// <summary>
        /// Convert float to 4 bytes in big-endian order
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Array of bytes</returns>
        protected static byte[] ConvertToBytesLE(float value)
        {
            byte[] bytes = [0, 0, 0, 0];
            BinaryPrimitives.WriteSingleLittleEndian(bytes, value);
            return bytes;
        }

        /// <summary>
        /// Convert double to 8 bytes in big-endian order
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Array of bytes</returns>
        protected static byte[] ConvertToBytesBE(double value)
        {
            byte[] bytes = [0, 0, 0, 0, 0, 0, 0, 0];
            BinaryPrimitives.WriteDoubleBigEndian(bytes, value);
            return bytes;
        }

        /// <summary>
        /// Convert double to 8 bytes in little-endian order
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Array of bytes</returns>
        protected static byte[] ConvertToBytesLE(double value)
        {
            byte[] bytes = [0, 0, 0, 0, 0, 0, 0, 0];
            BinaryPrimitives.WriteDoubleLittleEndian(bytes, value);
            return bytes;
        }

        /// <summary>
        /// Update the output calculations
        /// </summary>
        public override void UpdateCalculation()
        {
            lock (Program.Locker)
            {
                this.intervalCounter--;
                if (this.intervalCounter <= 0 && !this.Paused)
                {
                    this.intervalCounter = this.interval;
                    // Send data depending on current state
                    switch (this.State)
                    {
                        case OutputState.SEND_START: // Send start string while the selected time has not elapsed
                            if (this.useStart)
                            {
                                // Send start right away to ensure we do it at least one time
                                this.startArray = this.GetByteArray(this.startPlan);
                                if (!this.Send(this.startArray))
                                {
                                    this.ForceDisconnect();
                                }
                                // If start time elapsed, go to next stage
                                if (this.StartStopClock.ElapsedMilliseconds >= this.startDuration)
                                {
                                    this.StartStopClock.Stop();
                                    this.State = OutputState.CONNECTING;
                                    this.TransitionClock.Restart();
                                }
                            }
                            else this.State = OutputState.CONNECTING;
                            break;
                        case OutputState.CONNECTING:
                            this.CurrentTransitionTime += this.TransitionClock.Elapsed.TotalMilliseconds;
                            this.TransitionClock.Restart();
                            this.TransitionPercent = Utils.Value.Transition(this.CurrentTransitionTime, Program.OUTPUTS_TRANSITION_TIME);
                            if (this.TransitionPercent >= 1.0)
                            {
                                this.TransitionPercent = 1.0;
                                this.TransitionClock.Stop();
                                this.CurrentTransitionTime = Program.OUTPUTS_TRANSITION_TIME;
                                this.State = OutputState.CONNECTED;
                            }
                            if (this.useSend)
                            {
                                this.sendArray = this.GetByteArray(this.sendPlan);
                                if (!this.Send(this.sendArray))
                                {
                                    this.ForceDisconnect();
                                }
                            }
                            break;
                        case OutputState.CONNECTED: // If connected, just send the bytes
                            if (this.useSend)
                            {
                                this.sendArray = this.GetByteArray(this.sendPlan);
                                if (!this.Send(this.sendArray))
                                {
                                    this.ForceDisconnect();
                                }
                            }
                            break;
                        case OutputState.DISCONNECTING: // While disconnecting, decrease the transition time
                            this.CurrentTransitionTime -= this.TransitionClock.Elapsed.TotalMilliseconds;
                            this.TransitionClock.Restart();
                            this.TransitionPercent = Utils.Value.Transition(this.CurrentTransitionTime, Program.OUTPUTS_TRANSITION_TIME);
                            if (this.TransitionPercent <= 0.0)
                            {
                                this.TransitionPercent = 0.0;
                                this.TransitionClock.Stop();
                                this.CurrentTransitionTime = 0;
                                this.State = OutputState.SEND_STOP;
                                this.StartStopClock.Restart();
                            }
                            // Send bytes
                            if (this.useSend)
                            {
                                this.sendArray = this.GetByteArray(this.sendPlan);
                                if (!this.Send(this.sendArray))
                                {
                                    this.ForceDisconnect();
                                }
                            }
                            break;
                        case OutputState.SEND_STOP: // Send stop string while the selected time has not elapsed
                            if (this.useStop)
                            {
                                // Send stop right away to ensure we do it at least one time
                                this.stopArray = this.GetByteArray(this.stopPlan);
                                if (!this.Send(this.stopArray))
                                {
                                    this.ForceDisconnect();
                                }
                                // If stop time elapsed, go to next stage
                                if (this.StartStopClock.ElapsedMilliseconds >= this.stopDuration)
                                {
                                    this.State = OutputState.DISCONNECTED;
                                    this.StartStopClock.Stop();
                                    this.Disconnect();
                                }
                            }
                            else this.State = OutputState.DISCONNECTED;
                            break;
                        case OutputState.DISCONNECTED: // If disconnected, don't send data or do anything
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Update the pose UI
        /// </summary>
        public override void UpdateInterface()
        {
            // if (!this.Visible) // We have to handle auto connect if the module is hidden
            // {
                 // Get needed data from the other thread
            //     var state = OutputState.DISCONNECTED;
            //     lock (Program.Locker)
            //     {
            //         state = this.State;
            //     }
            //     if (this.State == OutputState.DISCONNECTED && this.CHKAutoConnect.Checked)
            //     {
            //         this.AutoConnectTime += Program.ElapsedInterfaceTime;
            //         if (this.AutoConnectTime > Program.AUTO_CONNECT_TIME)
            //         {
            //             this.TryToConnectDisconnect();
            //             this.AutoConnectTime = 0;
            //         }
            //     }
            // }
            // else
            if (this.Visible)
            {
                // Show debug info if panel is visible
                if (this.PNLDebug.Visible)
                {
                    // Get data from the calculation thread
                    var UIStartArray = new List<byte>();
                    var UISendArray = new List<byte>();
                    var UIStopArray = new List<byte>();
                    lock (Program.Locker)
                    {
                        UIStartArray.AddRange(this.startArray);
                        UISendArray.AddRange(this.sendArray);
                        UIStopArray.AddRange(this.stopArray);
                    }
                    var startText = "";
                    var sendText = "";
                    var stopText = "";
                    if (this.DDLDebugType.SelectedIndex == 0)
                    {
                        startText = UIStartArray.Select(b => b.ToString("000")).JoinItems(" ");
                        sendText = UISendArray.Select(b => b.ToString("000")).JoinItems(" ");
                        stopText = UIStopArray.Select(b => b.ToString("000")).JoinItems(" ");
                    }
                    else if (this.DDLDebugType.SelectedIndex == 1)
                    {
                        startText = UIStartArray.Select(b => b.ToString("X2")).JoinItems(" ");
                        sendText = UISendArray.Select(b => b.ToString("X2")).JoinItems(" ");
                        stopText = UIStopArray.Select(b => b.ToString("X2")).JoinItems(" ");
                    }
                    else
                    {
                        startText = UIStartArray.Select(b => ((b > 31 && b < 127) ? ((char)b).ToString() : "<" + b.ToString() + ">")).JoinItems("");
                        sendText = UISendArray.Select(b => ((b > 31 && b < 127) ? ((char)b).ToString() : "<" + b.ToString() + ">")).JoinItems("");
                        stopText = UIStopArray.Select(b => ((b > 31 && b < 127) ? ((char)b).ToString() : "<" + b.ToString() + ">")).JoinItems("");
                    }
                    this.LBLStartDebug.Text = startText;
                    this.LBLSendDebug.Text = sendText;
                    this.LBLStopDebug.Text = stopText;
                    if (this.updateStartToolTip) Program.Tip.ShowText("Start is sending " + this.startArray.Length.ToString() + " bytes:\n" + this.LBLStartDebug.Text, true);
                    if (this.updateSendToolTip) Program.Tip.ShowText("Sending " + this.sendArray.Length.ToString() + " bytes:\n" + this.LBLSendDebug.Text, true);
                    if (this.updateStopToolTip) Program.Tip.ShowText("Stop is sending " + this.stopArray.Length.ToString() + " bytes:\n" + this.LBLStopDebug.Text, true);
                }
            }
            // Get needed data from the other thread
            var state = OutputState.DISCONNECTED;
            var transitionPercent = 0.0;
            lock (Program.Locker)
            {
                state = this.State;
                transitionPercent = this.TransitionPercent;
            }
            // Disable auto connect if we where connected and are now disconnected
            // Also call the disconnect function to run the code needed to perform the disconnection
            if (this.lastState != OutputState.DISCONNECTED && this.State == OutputState.DISCONNECTED)
            {
                if (this.ForcedDisconnect == false) this.CHKAutoConnect.Checked = false;
                else this.ForcedDisconnect = false;
                this.Disconnect();
            }
            // Try to connect if auto connect checked
            else if (this.State == OutputState.DISCONNECTED && this.CHKAutoConnect.Checked)
            {
                this.AutoConnectTime += Program.ElapsedInterfaceTime;
                if (this.AutoConnectTime > Program.AUTO_CONNECT_TIME)
                {
                    this.TryToConnectDisconnect();
                    this.AutoConnectTime = 0;
                }
            }
            // If state or transition percent changed, update the button text and button color
            if (this.lastState != state || this.lastTransitionPercent != transitionPercent)
            {
                // Enable/Disable controls depending on current state
                this.EnableDisableControls();

                this.lastState = state;
                this.lastTransitionPercent = transitionPercent;
                switch (state)
                {
                    case OutputState.CONNECTED:
                        this.BTNConnectDisconnect.Text = "     Disconnect";
                        this.BTNConnectDisconnect.BackColor = Program.OUTOFRANGE_BackColor;
                        this.BTNConnectDisconnect.ForeColor = Program.OUTOFRANGE_ForeColor;
                        break;
                    case OutputState.DISCONNECTED:
                        this.BTNConnectDisconnect.Text = "     Connect";
                        this.BTNConnectDisconnect.BackColor = Program.BTNConnectDisconnect_BackColor;
                        this.BTNConnectDisconnect.ForeColor = Program.BTNConnectDisconnect_ForeColor;
                        break;
                    case OutputState.SEND_START:
                        this.BTNConnectDisconnect.Text = "     Starting";
                        this.BTNConnectDisconnect.BackColor = Program.TRANSITION_BackColor;
                        this.BTNConnectDisconnect.ForeColor = Program.TRANSITION_ForeColor;
                        break;
                    case OutputState.CONNECTING:
                        this.BTNConnectDisconnect.Text = "     Connecting";
                        this.BTNConnectDisconnect.BackColor = Utils.ColorEffect.Transition(Program.OUTOFRANGE_BackColor, Program.TRANSITION_BackColor, transitionPercent);
                        this.BTNConnectDisconnect.ForeColor = Utils.ColorEffect.Transition(Program.OUTOFRANGE_ForeColor, Program.TRANSITION_ForeColor, transitionPercent);

                        break;
                    case OutputState.DISCONNECTING:
                        this.BTNConnectDisconnect.Text = "     Disconnecting";
                        this.BTNConnectDisconnect.BackColor = Utils.ColorEffect.Transition(Program.OUTOFRANGE_BackColor, Program.TRANSITION_BackColor, transitionPercent);
                        this.BTNConnectDisconnect.ForeColor = Utils.ColorEffect.Transition(Program.OUTOFRANGE_ForeColor, Program.TRANSITION_ForeColor, transitionPercent);
                        break;
                    case OutputState.SEND_STOP:
                        this.BTNConnectDisconnect.Text = "     Stopping";
                        this.BTNConnectDisconnect.BackColor = Program.TRANSITION_BackColor;
                        this.BTNConnectDisconnect.ForeColor = Program.TRANSITION_ForeColor;
                        break;
                }
                // Since state changed, force diagram update
                Program.UpdateDiagramGraphics = true;
            }
        }

        /// <summary>
        /// Put here code to enable/disable or show/hide controls when connected or disconnected
        /// </summary>
        public virtual void EnableDisableControls()
        {
        }

        /// <summary>
        /// Call this to connect or disconnect the output
        /// Changes state of the connection depending on current state
        /// Connection is made here, but disconnection is made when the SEND_STOP terminates
        /// </summary>
        public void TryToConnectDisconnect()
        {
            lock (Program.Locker)
            {
                switch (this.State)
                {
                    case OutputState.SEND_START:
                        this.State = OutputState.SEND_STOP;
                        this.StartStopClock.Restart();
                        break;
                    case OutputState.CONNECTING:
                    case OutputState.CONNECTED:
                        this.State = OutputState.DISCONNECTING;
                        break;
                    case OutputState.DISCONNECTING:
                        this.State = OutputState.CONNECTING;
                        break;
                    case OutputState.SEND_STOP:
                        this.State = OutputState.SEND_START;
                        this.StartStopClock.Restart();
                        break;
                    case OutputState.DISCONNECTED:
                        if (!this.Connect()) break; // If we can't connect, keep the current state
                        this.State = OutputState.SEND_START;
                        this.StartStopClock.Restart();
                        break;
                }
            }
        }

        /// <summary>
        /// Call this when changing actuators settings to update the outputs
        /// </summary>
        internal void UpdateOutputs()
        {
            this.UIChanged(this.TXTStart, EventArgs.Empty);
            this.UIChanged(this.TXTSend, EventArgs.Empty);
            this.UIChanged(this.TXTStop, EventArgs.Empty);
        }

        /// <summary>
        /// Put here the necessary code to connect the output
        /// </summary>
        /// <returns>True if connect was successfull</returns>
        public virtual bool Connect()
        {
            return true;
        }

        /// <summary>
        /// Put here the necessary code to disconnect the output
        /// </summary>
        public virtual void Disconnect()
        {
        }

        /// <summary>
        /// Override this function when you want to send data over the connection.
        /// </summary>
        /// <param name="array">The array of bytes you want to send.</param>
        /// <returns>Returns false if an error occurred while sending the array.</returns>
        protected virtual bool Send(byte[] array)
        {
            return false;
        }



        /// <summary>
        /// Updates connections on the diagram
        /// </summary>
        protected virtual void UpdateDiagramConnections()
        {
            var modulesConnected = new HashSet<UI.MODULE>();

            AddDiagramConnectionsFromText(this.TXTStart.Text, modulesConnected);
            AddDiagramConnectionsFromText(this.TXTSend.Text, modulesConnected);
            AddDiagramConnectionsFromText(this.TXTStop.Text, modulesConnected);

            foreach (var connection in Program.MainWindow.ModuleConnections
                .Where(c => c.EndM == this && !modulesConnected.Contains(c.StartM))
                .Reverse())
            {
                Program.MainWindow.RemoveConnection(connection.StartM, this);
            }

            foreach (var module in modulesConnected)
            {
                Program.MainWindow.AddConnection(module, this);
            }
        }

        private void AddDiagramConnectionsFromText(string text, HashSet<UI.MODULE> modulesConnected)
        {
            if (string.IsNullOrEmpty(text)) return;

            var variableFound = false;
            var variableText = new StringBuilder();

            foreach (var c in text)
            {
                if (c == '<')
                {
                    variableFound = true;
                    variableText.Clear();
                    continue;
                }

                if (!variableFound) continue;

                if (c != '>')
                {
                    variableText.Append(c);
                    continue;
                }

                variableFound = false;
                var token = variableText.ToString();
                if (token.Length == 0 ||
                    long.TryParse(token, out _) ||
                    token == "MOVER.COUNTER8" ||
                    token == "MOVER.COUNTER16" ||
                    token == "MOVER.COUNTER32")
                {
                    continue;
                }

                token = token.ToUpper();
                if (Operations.VARIABLE.GetVariableValue(token, this) == null) continue;

                var separator = token.IndexOf('.');
                var moduleKey = separator >= 0 ? token[..separator] : token;
                var module = Application.OpenForms
                    .OfType<UI.MODULE>()
                    .FirstOrDefault(m => m.Key == moduleKey && m is not UI.SOURCE_HIDDEN);

                if (module is not null)
                {
                    modulesConnected.Add(module);
                }
            }
        }


        










        /// <summary>
        /// Generate list of functions described by a string to generate a byte array
        /// We also update the diagram connections
        /// </summary>
        /// <param name="text">The string from where we generate the list of functions</param>
        /// <returns>List of functions</returns>
        private PacketPlan BuildPacketPlanFromString(string text, bool withTransition = false)
        {
            var segments = new List<PacketSegment>();

            if (this.typeOfOutput < 2)
            {
                BuildBinaryPacketPlan(text, withTransition, segments);
            }
            else
            {
                BuildAsciiPacketPlan(text, withTransition, segments);
            }

            this.UpdateDiagramConnections();
            return segments.Count == 0 ? PacketPlan.Empty : new PacketPlan(segments);
        }

        private void BuildBinaryPacketPlan(string text, bool withTransition, List<PacketSegment> segments)
        {
            var variableFound = false;
            var variableText = new StringBuilder();
            byte variableBits = 0;
            var literalBytes = new List<byte>();

            foreach (var c in text)
            {
                if (c == '<')
                {
                    if (!variableFound && literalBytes.Count != 0)
                    {
                        segments.Add(new ConstantSegment([.. literalBytes]));
                        literalBytes.Clear();
                    }

                    variableFound = true;
                    variableText.Clear();
                    variableBits += 8;
                    continue;
                }

                if (!variableFound)
                {
                    if (c != '>') literalBytes.Add((byte)c);
                    continue;
                }

                if (c != '>')
                {
                    variableText.Append(c);
                    continue;
                }

                variableFound = false;
                AddBinarySegment(segments, variableText.ToString(), variableBits, withTransition);
                variableBits = 0;
            }

            if (literalBytes.Count != 0) segments.Add(new ConstantSegment([.. literalBytes]));
        }

        private void BuildAsciiPacketPlan(string text, bool withTransition, List<PacketSegment> segments)
        {
            var variableFound = false;
            var variableText = new StringBuilder();
            int digits = 0;
            var literalBytes = new List<byte>();

            for (int i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '<')
                {
                    if (!variableFound && literalBytes.Count != 0)
                    {
                        segments.Add(new ConstantSegment([.. literalBytes]));
                        literalBytes.Clear();
                    }

                    variableFound = true;
                    variableText.Clear();
                    digits++;
                    continue;
                }

                if (!variableFound)
                {
                    if (c != '>') literalBytes.Add((byte)c);
                    continue;
                }

                if (c != '>')
                {
                    variableText.Append(c);
                    continue;
                }

                int decimals = -1;
                while (i < text.Length && text[i] == '>')
                {
                    decimals++;
                    i++;
                }
                i--;

                variableFound = false;
                AddAsciiSegment(segments, variableText.ToString(), digits, decimals, withTransition);
                digits = 0;
            }

            if (literalBytes.Count != 0) segments.Add(new ConstantSegment([.. literalBytes]));
        }

        private void AddBinarySegment(List<PacketSegment> segments, string variableText, byte variableBits, bool withTransition)
        {
            if (long.TryParse(variableText, out long longValue))
            {
                segments.Add(new ConstantSegment(ConvertIntegerToBytes(longValue, variableBits, this.bigEndian)));
                return;
            }

            if (double.TryParse(variableText, out double doubleValue))
            {
                byte[] binaryValue = variableBits > 15
                    ? ConvertDoubleToBytes(doubleValue, this.bigEndian)
                    : ConvertFloatToBytes((float)doubleValue, this.bigEndian);
                segments.Add(new ConstantSegment(binaryValue));
                return;
            }

            switch (variableText)
            {
                case "MOVER.COUNTER8":
                    segments.Add(new IntBitsSegment(() => Program.Counter8, 8));
                    return;
                case "MOVER.COUNTER16":
                    segments.Add(new IntBitsSegment(() => Program.Counter16, 16));
                    return;
                case "MOVER.COUNTER32":
                    segments.Add(new IntBitsSegment(() => Program.Counter32, 32));
                    return;
            }

            variableText = variableText.ToUpper();
            var functionToGetValue = Operations.VARIABLE.GetVariableValue(variableText, this, withTransition);
            if (functionToGetValue == null) return;

            if (variableText.EndsWith(".B"))
            {
                var functionToGetBits = Operations.VARIABLE.GetVariableBits(variableText);
                if (functionToGetBits == null) return;

                segments.Add(new VariableBitsSegment(functionToGetValue, functionToGetBits));
                return;
            }

            if (variableBits == 8)
            {
                segments.Add(new Float32Segment(functionToGetValue));
            }
            else
            {
                segments.Add(new Float64Segment(functionToGetValue));
            }
        }

        private void AddAsciiSegment(List<PacketSegment> segments, string variableText, int digits, int decimals, bool withTransition)
        {
            if (long.TryParse(variableText, out long value))
            {
                if (value >= 0 && value <= 255)
                {
                    segments.Add(new ConstantSegment([(byte)value]));
                }
                return;
            }

            variableText = variableText.ToUpper();
            var functionToGetValue = Operations.VARIABLE.GetVariableValue(variableText, this, withTransition);
            if (functionToGetValue == null) return;

            if (variableText.EndsWith(".B"))
            {
                segments.Add(new AsciiVariableSegment(functionToGetValue, "N0"));
                return;
            }

            var format = new string('0', digits) + '.' + new string('0', decimals);
            segments.Add(new AsciiVariableSegment(functionToGetValue, format));
        }

        private static byte[] ConvertIntegerToBytes(long value, byte bits, bool bigEndian)
        {
            return bigEndian ? ConvertToBytesBE(value, bits) : ConvertToBytesLE(value, bits);
        }

        private static byte[] ConvertFloatToBytes(float value, bool bigEndian)
        {
            return bigEndian ? ConvertToBytesBE(value) : ConvertToBytesLE(value);
        }

        private static byte[] ConvertDoubleToBytes(double value, bool bigEndian)
        {
            return bigEndian ? ConvertToBytesBE(value) : ConvertToBytesLE(value);
        }

        private sealed class PacketPlan
        {
            private readonly PacketSegment[] segments;
            private readonly int constantLength;
            private readonly bool fixedSize;

            public static readonly PacketPlan Empty = new([]);

            public PacketPlan(List<PacketSegment> segments)
            {
                this.segments = [.. segments];
                int length = 0;
                bool isFixed = true;
                foreach (var segment in this.segments)
                {
                    if (segment.IsFixedSize) length += segment.FixedSize;
                    else isFixed = false;
                }
                this.constantLength = length;
                this.fixedSize = isFixed;
            }

            public byte[] Build(bool bigEndian)
            {
                if (this.segments.Length == 0) return EmptyBytes;

                int totalLength = this.constantLength;
                if (!this.fixedSize)
                {
                    for (int i = 0; i < this.segments.Length; i++)
                    {
                        var segment = this.segments[i];
                        if (!segment.IsFixedSize) totalLength += segment.GetSize(bigEndian);
                    }
                }

                if (totalLength == 0) return EmptyBytes;

                var data = new byte[totalLength];
                int offset = 0;
                for (int i = 0; i < this.segments.Length; i++)
                {
                    offset += this.segments[i].Write(data, offset, bigEndian);
                }
                return data;
            }
        }

        private abstract class PacketSegment
        {
            public abstract bool IsFixedSize { get; }
            public abstract int FixedSize { get; }
            public abstract int GetSize(bool bigEndian);
            public abstract int Write(byte[] destination, int offset, bool bigEndian);
        }

        private sealed class ConstantSegment(byte[] data) : PacketSegment
        {
            private readonly byte[] data = data;
            public override bool IsFixedSize => true;
            public override int FixedSize => this.data.Length;
            public override int GetSize(bool bigEndian) => this.data.Length;
            public override int Write(byte[] destination, int offset, bool bigEndian)
            {
                Buffer.BlockCopy(this.data, 0, destination, offset, this.data.Length);
                return this.data.Length;
            }
        }

        private sealed class IntBitsSegment(Func<double> getter, int bits) : PacketSegment
        {
            private readonly Func<double> getter = getter;
            private readonly int bits = bits;
            private readonly int byteCount = GetOutputByteCount(bits);

            public override bool IsFixedSize => true;
            public override int FixedSize => this.byteCount;
            public override int GetSize(bool bigEndian) => this.byteCount;
            public override int Write(byte[] destination, int offset, bool bigEndian)
            {
                WriteIntegerBytes((long)this.getter(), this.bits, bigEndian, destination, offset);
                return this.byteCount;
            }
        }

        private sealed class VariableBitsSegment(Func<double> getter, Func<byte> bitsGetter) : PacketSegment
        {
            private readonly Func<double> getter = getter;
            private readonly Func<byte> bitsGetter = bitsGetter;
            public override bool IsFixedSize => false;
            public override int FixedSize => 0;
            public override int GetSize(bool bigEndian) => GetOutputByteCount(this.bitsGetter());
            public override int Write(byte[] destination, int offset, bool bigEndian)
            {
                byte bits = this.bitsGetter();
                int count = GetOutputByteCount(bits);
                WriteIntegerBytes((long)this.getter(), bits, bigEndian, destination, offset);
                return count;
            }
        }

        private sealed class Float32Segment(Func<double> getter) : PacketSegment
        {
            private readonly Func<double> getter = getter;
            public override bool IsFixedSize => true;
            public override int FixedSize => 4;
            public override int GetSize(bool bigEndian) => 4;
            public override int Write(byte[] destination, int offset, bool bigEndian)
            {
                WriteFloatBytes((float)this.getter(), bigEndian, destination, offset);
                return 4;
            }
        }

        private sealed class Float64Segment(Func<double> getter) : PacketSegment
        {
            private readonly Func<double> getter = getter;
            public override bool IsFixedSize => true;
            public override int FixedSize => 8;
            public override int GetSize(bool bigEndian) => 8;
            public override int Write(byte[] destination, int offset, bool bigEndian)
            {
                WriteDoubleBytes(this.getter(), bigEndian, destination, offset);
                return 8;
            }
        }

        private sealed class AsciiVariableSegment(Func<double> getter, string format) : PacketSegment
        {
            private readonly Func<double> getter = getter;
            private readonly string format = format;
            public override bool IsFixedSize => false;
            public override int FixedSize => 0;
            public override int GetSize(bool bigEndian)
            {
                var text = this.getter().ToString(this.format);
                return AsciiEncoding.GetByteCount(text);
            }
            public override int Write(byte[] destination, int offset, bool bigEndian)
            {
                var text = this.getter().ToString(this.format);
                return AsciiEncoding.GetBytes(text, 0, text.Length, destination, offset);
            }
        }

        private static void WriteIntegerBytes(long value, int bits, bool bigEndian, byte[] destination, int offset)
        {
            int byteCount = GetOutputByteCount(bits);
            if (bigEndian)
            {
                for (int i = 0; i < byteCount; i++)
                {
                    int shift = (byteCount - 1 - i) * 8;
                    destination[offset + i] = (byte)((value >> shift) & 0xff);
                }
            }
            else
            {
                for (int i = 0; i < byteCount; i++)
                {
                    int shift = i * 8;
                    destination[offset + i] = (byte)((value >> shift) & 0xff);
                }
            }
        }

        private static void WriteFloatBytes(float value, bool bigEndian, byte[] destination, int offset)
        {
            if (bigEndian) BinaryPrimitives.WriteSingleBigEndian(destination.AsSpan(offset, 4), value);
            else BinaryPrimitives.WriteSingleLittleEndian(destination.AsSpan(offset, 4), value);
        }

        private static void WriteDoubleBytes(double value, bool bigEndian, byte[] destination, int offset)
        {
            if (bigEndian) BinaryPrimitives.WriteDoubleBigEndian(destination.AsSpan(offset, 8), value);
            else BinaryPrimitives.WriteDoubleLittleEndian(destination.AsSpan(offset, 8), value);
        }

        /// <summary>
        /// Show tooltip of the debug
        /// </summary>
        private void LBLTT_MouseEnter(object? sender, EventArgs e)
        {
            if (sender == this.LBLStartTT) this.updateStartToolTip = true;
            else if (sender == this.LBLSendTT) this.updateSendToolTip = true;
            else if (sender == this.LBLStopTT) this.updateStopToolTip = true;
        }

        /// <summary>
        /// Hide tooltip of the debug
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LBLTT_MouseLeave(object? sender, EventArgs e)
        {
            if (sender == this.LBLStartTT)
            {
                this.updateStartToolTip = false;
                Program.Tip.SetToolTip(this.LBLStartTT, "");
            }
            else if (sender == this.LBLSendTT)
            {
                this.updateSendToolTip = false;
                Program.Tip.SetToolTip(this.LBLSendTT, "");
            }
            else if (sender == this.LBLStopTT)
            {
                this.updateStopToolTip = false;
                Program.Tip.SetToolTip(this.LBLStopTT, "");
            }
        }

        /// <summary>
        /// Update info of the interval loops (from here and from the Mover options)
        /// </summary>
        internal void UpdateIntervalInfo()
        {
            this.LBLIntervalMS.Text = "(" + (Program.PretendedCalculationTime * this.interval).ToString("N1") + " ms)";
        }

        /// <summary>
        /// Adjust panels positions and window size when changing expanding panels visibility
        /// </summary>
        private void ExpandingPanels_VisibleChanged(object sender, EventArgs e)
        {
            this.Width = this.TVValues.Visible ? this.TVValues.Right + 1 : this.TVValues.Left + 1;
            this.Height = this.PNLDebug.Visible ? this.PNLDebug.Bottom + 1 : this.PNLDebug.Top + 1;
        }

        /// <summary>
        /// Force disconnect of the output
        /// Goes directly to disconnected without transition
        /// Use on connection errors/crashes
        /// Keeps auto connect enabled
        /// </summary>
        public void ForceDisconnect()
        {
            this.ForcedDisconnect = true;
            this.Disconnect();
            this.TransitionClock.Stop();
            this.CurrentTransitionTime = 0.0;
            this.TransitionPercent = 0.0;
            this.State = OutputState.DISCONNECTED;
        }

        /// <summary>
        /// Update the controls sizes and positions
        /// </summary>
        public override void UpdateControlsPositionsAndSizes()
        {
            base.UpdateControlsPositionsAndSizes();
            // Window
            HLPHelp.SetSize(Program.Size24, Program.Size24);
            HLPHelp.AtRightOf(SHDebug, 0);
            BTNConnectDisconnect.SetSize(Program.Size100, Program.Size24);
            BTNConnectDisconnect.SetLine24(0);
            BTNConnectDisconnect.SetLeft(Program.Size96);
            CHKAutoConnect.SetSize(Program.Size14, Program.Size14);
            CHKAutoConnect.RelativeTo(BTNConnectDisconnect, Program.Size5, Program.Size5);
            BTNPauseContinueOutput.SetSize(Program.Size24, Program.Size24);
            BTNPauseContinueOutput.AtRightOf(BTNConnectDisconnect);
            LBLInterval.SetSize(Program.Size70, Program.Size20);
            LBLInterval.AtRightOf(BTNPauseContinueOutput, Program.Size40, Program.Alignment.CENTER);
            NUDInterval.SetSize(Program.Size70, Program.Size20);
            NUDInterval.AtRightOf(LBLInterval);
            LBLIntervalMS.SetSize(Program.Size70, Program.Size20);
            LBLIntervalMS.AtRightOf(NUDInterval);
            LBLStart.SetSize(Program.Size54, Program.Size20);
            LBLStart.Left = Program.Size12;
            LBLStart.SetLine20(6);
            LBLSend.AtBottomAndSizeOf(LBLStart);
            LBLStop.AtBottomAndSizeOf(LBLSend);
            CHKStart.SetSize(Program.Size14, Program.Size20);
            CHKStart.AtRightOf(LBLStart);
            CHKSend.AtBottomAndSizeOf(CHKStart);
            CHKStop.AtBottomAndSizeOf(CHKSend);
            TXTStart.SetSize(8 * Program.Size70 + 6 * Program.Size8 - Program.Size50, Program.Size20);
            TXTStart.AtRightOf(CHKStart);
            NUDStart.SetSize(Program.Size50, Program.Size20);
            NUDStart.AtRightOf(TXTStart);
            LBLStartMS.SetSize(Program.Size20, Program.Size20);
            LBLStartMS.AtRightOf(NUDStart);
            TXTSend.SetSize(8 * Program.Size70 + 7 * Program.Size8, Program.Size20);
            TXTSend.AtRightOf(CHKSend);
            TXTStop.SetSizeOf(TXTStart);
            TXTStop.AtRightOf(CHKStop);
            NUDStop.SetSizeOf(NUDStart);
            NUDStop.AtRightOf(TXTStop);
            LBLStopMS.SetSizeOf(LBLStartMS);
            LBLStopMS.AtRightOf(NUDStop);
            DDLCRC.SetSize(Program.Size120, Program.Size20);
            DDLCRC.AtBottomOf(NUDStop, Program.Alignment.RIGHT);
            LBLCRC.AtLeftAndSizeOf(DDLCRC);
            DDLType.SetSize(Program.Size100, Program.Size20);
            DDLType.HorizontalAlignTo(TXTStart, Program.Alignment.RIGHT);
            DDLType.VerticalAlignTo(LBLInterval);
            LBLType.AtLeftAndSizeOf(DDLType);
            // Debug panel
            DDLDebugType.SetSize(Program.Size200, Program.Size20);
            DDLDebugType.SetRight(PNLDebug.Width - Program.Size12);
            DDLDebugType.SetTop(Program.Size12);
            LBLStartTT.SetSize(Program.Size20, Program.Size20);
            LBLStartTT.AtBottomOf(DDLDebugType, Program.Alignment.RIGHT);
            LBLSendTT.AtBottomAndSizeOf(LBLStartTT);
            LBLStopTT.AtBottomAndSizeOf(LBLSendTT);
            LBLStartDebugDebug.SetSize(Program.Size30, Program.Size20);
            LBLStartDebugDebug.SetLeft(Program.Size12);
            LBLStartDebugDebug.VerticalAlignTo(LBLStartTT);
            LBLSendDebugDebug.AtBottomAndSizeOf(LBLStartDebugDebug);
            LBLStopDebugDebug.AtBottomAndSizeOf(LBLSendDebugDebug);
            LBLStartDebug.SetSize(LBLStartTT.Left - Program.Size8 - LBLStartDebugDebug.Right, Program.Size20);
            LBLStartDebug.AtLeftOf(LBLStartTT, 0);
            LBLSendDebug.AtBottomAndSizeOf(LBLStartDebug);
            LBLStopDebug.AtBottomAndSizeOf(LBLSendDebug);
        }

        /// <summary>
        /// Update the controls colors
        /// </summary>
        public override void UpdateControlsColors()
        {
            base.UpdateControlsColors();

            this.SHValues.BackColor = Program.MODULE_Border;
            this.SHDebug.BackColor = Program.Gray[205];

            switch (this.State)
            {
                case OutputState.CONNECTED:
                    this.BTNConnectDisconnect.BackColor = Program.OUTOFRANGE_BackColor;
                    this.BTNConnectDisconnect.ForeColor = Program.OUTOFRANGE_ForeColor;
                    break;
                case OutputState.DISCONNECTED:
                    this.BTNConnectDisconnect.BackColor = Program.BTNConnectDisconnect_BackColor;
                    this.BTNConnectDisconnect.ForeColor = Program.BTNConnectDisconnect_ForeColor;
                    break;
                case OutputState.SEND_START:
                case OutputState.SEND_STOP:
                    this.BTNConnectDisconnect.BackColor = Program.TRANSITION_BackColor;
                    this.BTNConnectDisconnect.ForeColor = Program.TRANSITION_ForeColor;
                    break;
            }
            BTNPauseContinueOutput.BackColor = Program.BTNConnectDisconnect_BackColor;
            BTNPauseContinueOutput.ForeColor = Program.BTNConnectDisconnect_ForeColor;

            LBLInterval.ForeColor =
                LBLIntervalMS.ForeColor =
                LBLType.ForeColor =
                LBLStart.ForeColor =
                LBLStartMS.ForeColor =
                LBLSend.ForeColor =
                LBLStop.ForeColor =
                LBLStopMS.ForeColor =
                LBLCRC.ForeColor =
                LBLStartDebugDebug.ForeColor =
                LBLStartDebug.ForeColor =
                LBLSendDebugDebug.ForeColor =
                LBLSendDebug.ForeColor =
                LBLStopDebugDebug.ForeColor =
                LBLStopDebug.ForeColor = 
                Program.MODULE_ForeColor;
            LBLStartDebug.BackColor =
                LBLSendDebug.BackColor =
                LBLStopDebug.BackColor =
                Program.Gray[195];
            LBLStartTT.BackColor =
                LBLSendTT.BackColor =
                LBLStopTT.BackColor =
                Program.Gray[180];
            LBLStartTT.ForeColor =
                LBLSendTT.ForeColor =
                LBLStopTT.ForeColor =
                Program.BTNConnectDisconnect_ForeColor;

            TXTStart.BackColor =
                TXTSend.BackColor =
                TXTStop.BackColor =
                Program.TXT_BackColor;
            TXTStart.ForeColor =
                TXTSend.ForeColor =
                TXTStop.ForeColor =
                Program.TXT_ForeColor;

            DDLType.BackColor =
                DDLCRC.BackColor =
                DDLDebugType.BackColor =
                Program.DDL_BackColor;
            DDLType.ForeColor =
                DDLCRC.ForeColor =
                DDLDebugType.ForeColor =
                Program.DDL_ForeColor;

            NUDInterval.BackColor =
                NUDStart.BackColor =
                NUDStop.BackColor =
                Program.NUD_BackColor;
            NUDInterval.ForeColor =
                NUDStart.ForeColor =
                NUDStop.ForeColor =
                Program.NUD_ForeColor;
        }
    }
}
