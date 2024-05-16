using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Media.Media3D;
using System.Drawing.Imaging;
using System.IO;
using Tesseract;
using Path = System.IO.Path;
using Image = System.Drawing.Image;
using System.Reflection;
using Point = System.Windows.Point;
using System.Windows.Threading;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using System.Text.RegularExpressions;

namespace FacebookCFVRevealBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);
        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);
        [DllImport("user32.dll")]
        private static extern IntPtr GetMessageExtraInfo();

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        [Flags]
        public enum InputType
        {
            Mouse = 0,
            Keyboard = 1,
            Hardware = 2
        }

        [Flags]
        public enum KeyEventF
        {
            KeyDown = 0x0000,
            ExtendedKey = 0x0001,
            KeyUp = 0x0002,
            Unicode = 0x0004,
            Scancode = 0x0008,
        }

        public enum DirectXKeyStrokes
        {
            //keyboard DX inputs
            DIK_ESCAPE = 0x01,
            DIK_1 = 0x02,
            DIK_2 = 0x03,
            DIK_3 = 0x04,
            DIK_4 = 0x05,
            DIK_5 = 0x06,
            DIK_6 = 0x07,
            DIK_7 = 0x08,
            DIK_8 = 0x09,
            DIK_9 = 0x0A,
            DIK_0 = 0x0B,
            DIK_MINUS = 0x0C,
            DIK_EQUALS = 0x0D,
            DIK_BACK = 0x0E,
            DIK_TAB = 0x0F,
            DIK_Q = 0x10,
            DIK_W = 0x11,
            DIK_E = 0x12,
            DIK_R = 0x13,
            DIK_T = 0x14,
            DIK_Y = 0x15,
            DIK_U = 0x16,
            DIK_I = 0x17,
            DIK_O = 0x18,
            DIK_P = 0x19,
            DIK_LBRACKET = 0x1A,
            DIK_RBRACKET = 0x1B,
            DIK_RETURN = 0x1C,
            DIK_LCONTROL = 0x1D,
            DIK_A = 0x1E,
            DIK_S = 0x1F,
            DIK_D = 0x20,
            DIK_F = 0x21,
            DIK_G = 0x22,
            DIK_H = 0x23,
            DIK_J = 0x24,
            DIK_K = 0x25,
            DIK_L = 0x26,
            DIK_SEMICOLON = 0x27,
            DIK_APOSTROPHE = 0x28,
            DIK_GRAVE = 0x29,
            DIK_LSHIFT = 0x2A,
            DIK_BACKSLASH = 0x2B,
            DIK_Z = 0x2C,
            DIK_X = 0x2D,
            DIK_C = 0x2E,
            DIK_V = 0x2F,
            DIK_B = 0x30,
            DIK_N = 0x31,
            DIK_M = 0x32,
            DIK_COMMA = 0x33,
            DIK_PERIOD = 0x34,
            DIK_SLASH = 0x35,
            DIK_RSHIFT = 0x36,
            DIK_MULTIPLY = 0x37,
            DIK_LMENU = 0x38,
            DIK_SPACE = 0x39,
            DIK_CAPITAL = 0x3A,
            DIK_F1 = 0x3B,
            DIK_F2 = 0x3C,
            DIK_F3 = 0x3D,
            DIK_F4 = 0x3E,
            DIK_F5 = 0x3F,
            DIK_F6 = 0x40,
            DIK_F7 = 0x41,
            DIK_F8 = 0x42,
            DIK_F9 = 0x43,
            DIK_F10 = 0x44,
            DIK_NUMLOCK = 0x45,
            DIK_SCROLL = 0x46,
            DIK_NUMPAD7 = 0x47,
            DIK_NUMPAD8 = 0x48,
            DIK_NUMPAD9 = 0x49,
            DIK_SUBTRACT = 0x4A,
            DIK_NUMPAD4 = 0x4B,
            DIK_NUMPAD5 = 0x4C,
            DIK_NUMPAD6 = 0x4D,
            DIK_ADD = 0x4E,
            DIK_NUMPAD1 = 0x4F,
            DIK_NUMPAD2 = 0x50,
            DIK_NUMPAD3 = 0x51,
            DIK_NUMPAD0 = 0x52,
            DIK_DECIMAL = 0x53,
            DIK_F11 = 0x57,
            DIK_F12 = 0x58,
            DIK_F13 = 0x64,
            DIK_F14 = 0x65,
            DIK_F15 = 0x66,
            DIK_KANA = 0x70,
            DIK_CONVERT = 0x79,
            DIK_NOCONVERT = 0x7B,
            DIK_YEN = 0x7D,
            DIK_NUMPADEQUALS = 0x8D,
            DIK_CIRCUMFLEX = 0x90,
            //DIK_AT = 0x91 Does not work
            //DIK_COLON = 0x92 Does not work
            //DIK_UNDERLINE = 0x93 Does not work
            DIK_KANJI = 0x94,
            DIK_STOP = 0x95,
            DIK_AX = 0x96,
            DIK_UNLABELED = 0x97,
            DIK_NUMPADENTER = 0x9C,
            DIK_RCONTROL = 0x9D,
            DIK_NUMPADCOMMA = 0xB3,
            DIK_DIVIDE = 0xB5,
            DIK_SYSRQ = 0xB7,
            DIK_RMENU = 0xB8,
            DIK_HOME = 0xC7,
            DIK_UP = 0xC8,
            DIK_PRIOR = 0xC9,
            DIK_LEFT = 0xCB,
            DIK_RIGHT = 0xCD,
            DIK_END = 0xCF,
            DIK_DOWN = 0xD0,
            DIK_NEXT = 0xD1,
            DIK_INSERT = 0xD2,
            DIK_DELETE = 0xD3,
            DIK_LWIN = 0xDB,
            DIK_RWIN = 0xDC,
            DIK_APPS = 0xDD,
            DIK_BACKSPACE = DIK_BACK,
            DIK_NUMPADSTAR = DIK_MULTIPLY,
            DIK_LALT = DIK_LMENU,
            DIK_CAPSLOCK = DIK_CAPITAL,
            DIK_NUMPADMINUS = DIK_SUBTRACT,
            DIK_NUMPADPLUS = DIK_ADD,
            DIK_NUMPADPERIOD = DIK_DECIMAL,
            DIK_NUMPADSLASH = DIK_DIVIDE,
            DIK_RALT = DIK_RMENU,
            DIK_UPARROW = DIK_UP,
            DIK_PGUP = DIK_PRIOR,
            DIK_LEFTARROW = DIK_LEFT,
            DIK_RIGHTARROW = DIK_RIGHT,
            DIK_DOWNARROW = DIK_DOWN,
            DIK_PGDN = DIK_NEXT,
        }

        public struct Input
        {
            public int type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        public struct InputUnion
        {
            [FieldOffset(0)] public MouseInput mi;
            [FieldOffset(0)] public KeyboardInput ki;
            [FieldOffset(0)] public readonly HardwareInput hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MouseInput
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct KeyboardInput
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public readonly uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HardwareInput
        {
            public readonly uint uMsg;
            public readonly ushort wParamL;
            public readonly ushort wParamH;
        }

        [Flags]
        public enum MouseEventF
        {
            Absolute = 0x8000,
            HWheel = 0x01000,
            Move = 0x0001,
            MoveNoCoalesce = 0x2000,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            VirtualDesk = 0x4000,
            Wheel = 0x0800,
            XDown = 0x0080,
            XUp = 0x0100
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        public struct CoordinateLayout
        {
            public Point addressbar;
            public Point postTime;
            public Point browserBack;
            public Point browserRefresh;
            public Point messengerTab;
            public Point drevlinTab;
            public Point chatTextbox;
        }

        String lastPageRead { get; set; }
        Boolean checking = false;
        DispatcherTimer checkForReveals = new DispatcherTimer();
        String chromeBitmapPath = AppDomain.CurrentDomain.BaseDirectory + @"\\ChromeBitmap.jpg";
        CoordinateLayout currentLayout = new CoordinateLayout();
        System.Windows.Rect chromeRect = new System.Windows.Rect();
        Process chromeProcess = new Process();

        public MainWindow()
        {
            InitializeComponent();

            this.Topmost = true;

            DataContext = this;

            checkForReveals.Tick += (sender, e) => _ = CheckForReveals();
        }

        private void Start(object sender, RoutedEventArgs e)
        {
            switch (checkForReveals.IsEnabled)
            {
                case false:
                    start_button.Content = "Stop";

                    StartCheckingForReveals();
                    break;

                default:
                    start_button.Content = "Start";

                    StopCheckingForReveals();
                    break;
            }
        }

        private void StartCheckingForReveals()
        {
            checkForReveals.Interval = TimeSpan.FromMinutes(Convert.ToInt32(interval_textBox.Text.Trim()));
            checkForReveals.Start();
        }

        private void StopCheckingForReveals()
        {
            lastPageRead = null!;
            checkForReveals.Stop();
        }

        private async Task CheckForReveals()
        {
            if (!checking)
            {
                checking = true;

                String pageRead = "";
                Boolean failedToReach = false;
                Int32 waitCount = 0;

                await MoveToStep("BROWSERREFRESH", 100, Convert.ToInt32(loadWait_textBox.Text));

                await MoveToStep("POSTTIME", 100, Convert.ToInt32(loadWait_textBox.Text));

                while (String.IsNullOrEmpty(pageRead = GetPageRead(GetChromeHwnd())))
                {
                    if (waitCount > 100)
                    {
                        failedToReach = true;
                        break;
                    }

                    waitCount = waitCount + 1;

                    await Task.Delay(100);
                }

                if (failedToReach)
                {
                    //Could not get page text data

                    await MoveToStep("BROWSERBACK", 100, 100);

                    error_textBlock.Text = "Failed to retrieve bitmap text data from Tesseract engine.";

                    checking = false;

                    return;
                }

                if (String.IsNullOrEmpty(lastPageRead))
                {
                    //Set Initial Read
                    lastPageRead = pageRead;

                    await MoveToStep("BROWSERBACK", 100, 100);
                }
                else if (lastPageRead != pageRead)
                {
                    //New Post
                    await MoveToStep("ADDRESSBAR", 100, 100);

                    await MoveToStep("MESSENGERTAB", 100, 100);

                    await MoveToStep("CHATTEXTBOX", 100, 100);

                    await MoveToStep("DREVLINTAB", 100, 100);

                    await MoveToStep("BROWSERBACK", 100, 100);
                }
                else
                {
                    await MoveToStep("BROWSERBACK", 100, 100);
                }

                lastPageRead = pageRead;

                lastPageRead_textBlock.Text = lastPageRead;
            }

            checking = false;
        }

        private async Task MoveToStep(String step, Int32 clickDelay, Int32 waitDelay)
        {
            switch (step.ToUpper().Trim())
            {
                case "ADDRESSBAR":
                    SetCursorPos(Convert.ToInt32(currentLayout.addressbar.X),
                                 Convert.ToInt32(currentLayout.addressbar.Y));

                    await DoMouseClickAsync(clickDelay, waitDelay);

                    await HighlightAndCopy();
                    break;

                case "POSTTIME":
                    SetCursorPos(Convert.ToInt32(currentLayout.postTime.X),
                                 Convert.ToInt32(currentLayout.postTime.Y));

                    await DoMouseClickAsync(clickDelay, waitDelay);
                    break;

                case "BROWSERBACK":
                    SetCursorPos(Convert.ToInt32(currentLayout.browserBack.X),
                                 Convert.ToInt32(currentLayout.browserBack.Y));

                    await DoMouseClickAsync(clickDelay, waitDelay);
                    break;

                case "BROWSERREFRESH":
                    SetCursorPos(Convert.ToInt32(currentLayout.browserRefresh.X),
                                 Convert.ToInt32(currentLayout.browserRefresh.Y));

                    await DoMouseClickAsync(clickDelay, waitDelay);
                    break;

                case "MESSENGERTAB":
                    SetCursorPos(Convert.ToInt32(currentLayout.messengerTab.X),
                                 Convert.ToInt32(currentLayout.messengerTab.Y));

                    await DoMouseClickAsync(clickDelay, waitDelay);
                    break;

                case "DREVLINTAB":
                    SetCursorPos(Convert.ToInt32(currentLayout.drevlinTab.X),
                                 Convert.ToInt32(currentLayout.drevlinTab.Y));

                    await DoMouseClickAsync(clickDelay, waitDelay);
                    break;

                case "CHATTEXTBOX":
                    SetCursorPos(Convert.ToInt32(currentLayout.chatTextbox.X),
                                 Convert.ToInt32(currentLayout.chatTextbox.Y));

                    await DoMouseClickAsync(clickDelay, waitDelay);

                    await PasteNewReveal();
                    break;

                default:
                    error_textBlock.Text = "Step: " + coordsSelection_comboBox.Text + " is invalid.";
                    return;
            }
        }

        private async Task HighlightAndCopy()
        {
            SendKey(DirectXKeyStrokes.DIK_LCONTROL, false, InputType.Keyboard);
            SendKey(DirectXKeyStrokes.DIK_A, false, InputType.Keyboard);

            await Task.Delay(1000);

            SendKey(DirectXKeyStrokes.DIK_LCONTROL, true, InputType.Keyboard);
            SendKey(DirectXKeyStrokes.DIK_A, true, InputType.Keyboard);

            await Task.Delay(1000);

            SendKey(DirectXKeyStrokes.DIK_LCONTROL, false, InputType.Keyboard);
            SendKey(DirectXKeyStrokes.DIK_C, false, InputType.Keyboard);

            await Task.Delay(1000);

            SendKey(DirectXKeyStrokes.DIK_LCONTROL, true, InputType.Keyboard);
            SendKey(DirectXKeyStrokes.DIK_C, true, InputType.Keyboard);

            await Task.Delay(1000);
        }

        private async Task PasteNewReveal()
        {
            String fullMessage = message_textBox.Text + "\n" + Clipboard.GetText();

            Clipboard.SetText(fullMessage);

            await Task.Delay(1000);

            SendKey(DirectXKeyStrokes.DIK_LCONTROL, false, InputType.Keyboard);
            SendKey(DirectXKeyStrokes.DIK_V, false, InputType.Keyboard);

            await Task.Delay(1000);

            SendKey(DirectXKeyStrokes.DIK_LCONTROL, true, InputType.Keyboard);
            SendKey(DirectXKeyStrokes.DIK_V, true, InputType.Keyboard);

            await Task.Delay(1000);

            SendKey(DirectXKeyStrokes.DIK_RETURN, false, InputType.Keyboard);

            await Task.Delay(1000);

            SendKey(DirectXKeyStrokes.DIK_RETURN, true, InputType.Keyboard);

            await Task.Delay(1000);
        }

        private async Task DoMouseClickAsync(Int32 clickDelay, Int32 waitDelay)
        {
            await Task.Delay(clickDelay);

            DoMouseClick();

            await Task.Delay(waitDelay);
        }

        public static void MoveCursorToPoint(int x, int y)
        {
            SetCursorPos(x, y);
        }

        public static void DoMouseClick()
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        private void getCoords_button_Click(object sender, RoutedEventArgs e)
        {
            _ = SetCoord();
        }

        private async Task SetCoord()
        {
            Win32Point w32Mouse = new Win32Point();
            M1Overlay overlay = new M1Overlay();

            error_textBlock.Text = "";

            overlay.WindowState = WindowState.Maximized;
            overlay.Show();

            while (Mouse.LeftButton != MouseButtonState.Pressed)
            {
                await Task.Delay(1);
            }

            overlay.Close();

            GetCursorPos(ref w32Mouse);

            switch (coordsSelection_comboBox.Text.ToUpper().Trim())
            {
                case "ADDRESSBAR":
                    currentLayout.addressbar = new Point(w32Mouse.X, w32Mouse.Y);
                    break;

                case "POSTTIME":
                    currentLayout.postTime = new Point(w32Mouse.X, w32Mouse.Y);
                    break;

                case "BROWSERBACK":
                    currentLayout.browserBack = new Point(w32Mouse.X, w32Mouse.Y);
                    break;

                case "BROWSERREFRESH":
                    currentLayout.browserRefresh = new Point(w32Mouse.X, w32Mouse.Y);
                    break;

                case "MESSENGERTAB":
                    currentLayout.messengerTab = new Point(w32Mouse.X, w32Mouse.Y);
                    break;

                case "DREVLINTAB":
                    currentLayout.drevlinTab = new Point(w32Mouse.X, w32Mouse.Y);
                    break;

                case "CHATTEXTBOX":
                    currentLayout.chatTextbox = new Point(w32Mouse.X, w32Mouse.Y);
                    break;

                default:
                    error_textBlock.Text = "ComboBox selection: " + coordsSelection_comboBox.Text + " is invalid.";
                    return;
            }

            ModifyCoordinateTextblock(coordsSelection_comboBox.Text.ToUpper().Trim(), "X:" + w32Mouse.X.ToString() + "," + "Y:" + w32Mouse.Y.ToString());
        }

        private void ModifyCoordinateTextblock(String name, String coordinates)
        {
            TextBlock tb = MainGrid.Children.OfType<TextBlock>().Where(x => x.Tag != null && x.Tag.ToString()!.ToUpper().Trim() == name).FirstOrDefault()!;

            if (tb != null)
            {
                tb.Text = name + ": " + coordinates;
            }
        }

        private String GetPageRead(IntPtr hWnd)
        {
            ImageSource bitmapIs;
            Bitmap bitMap = new Bitmap(Convert.ToInt32(SystemParameters.PrimaryScreenWidth), 
                                       Convert.ToInt32(SystemParameters.PrimaryScreenHeight), 
                                       System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
            Graphics g = Graphics.FromImage(bitMap);
            RECT rct;

            GetWindowRect(new HandleRef(chromeProcess, hWnd), out rct);

            chromeRect.X = rct.Left;
            chromeRect.Y = rct.Top;
            chromeRect.Width = rct.Right - rct.Left;
            chromeRect.Height = rct.Bottom - rct.Top;


            g.CopyFromScreen(Convert.ToInt32(chromeRect.X) - Convert.ToInt32(leftDiff_textBox.Text), 
                             Convert.ToInt32(chromeRect.Y) - Convert.ToInt32(topDiff_textBox.Text), 
                             Convert.ToInt32(rightDiff_textBox.Text), 
                             Convert.ToInt32(bottomDiff_textBox.Text), 
                             new System.Drawing.Size(Convert.ToInt32(chromeRect.Width),
                             Convert.ToInt32(chromeRect.Height)));
            
            if (File.Exists(chromeBitmapPath))
            {
                File.Delete(chromeBitmapPath);
            }

            bitMap.Save(chromeBitmapPath);

            bitmapIs = ToBitmapImage(bitMap);

            bitMap_image.Source = bitmapIs;

            return GetAllTextFromBitmap(bitMap);
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private String GetAllTextFromBitmap(Bitmap bitMap)
        {
            String bitmapText = "";
            String path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
            path = Path.Combine(path, "tessdata");
            path = path.Replace("file:\\", "");

            using (TesseractEngine engine = new TesseractEngine(path, "eng", EngineMode.Default))
            {
                //engine.SetVariable("tessedit_char_whitelist", "1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ");
                //engine.SetVariable("tessedit_unrej_any_wd", true);

                using (var page = engine.Process(Pix.LoadFromFile(chromeBitmapPath)))
                {
                    bitmapText = page.GetText();
                }
            }

            return Regex.Replace(bitmapText, @"\s+", string.Empty);
        }
        private IntPtr GetChromeHwnd()
        {
            Process[] processes = Process.GetProcessesByName("chrome");

            chromeProcess = processes[0];

            return chromeProcess.MainWindowHandle;
        }

        public static void SendKey(DirectXKeyStrokes key, bool KeyUp, InputType inputType) //Send DX input key
        {
            try
            {
                uint flagtosend;

                if (KeyUp)
                {
                    flagtosend = (uint)(KeyEventF.KeyUp | KeyEventF.Scancode);
                }
                else
                {
                    flagtosend = (uint)(KeyEventF.KeyDown | KeyEventF.Scancode);
                }

                Input[] inputs =
                {
                    new Input
                    {
                        type = (int) inputType,
                        u = new InputUnion
                     {
                            ki = new KeyboardInput
                            {
                                wVk = 0,
                                wScan = (ushort) key,
                                dwFlags = flagtosend,
                                dwExtraInfo = GetMessageExtraInfo()
                            }
                        }
                    }
                };

                SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(Input)));
            }
            catch (Exception ex)
            {
                MessageBox.Show("You fucked something up, show to Will: " + "\n\n" + ex.ToString());
            }
        }

        private void test_button_Click(object sender, RoutedEventArgs e)
        {
            lastPageRead_textBlock.Text = GetPageRead(GetChromeHwnd());
        }
    }
}