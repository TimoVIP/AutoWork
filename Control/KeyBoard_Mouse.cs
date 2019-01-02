using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace TimoControl
{
    public static class KeyBoard_Mouse
    {
        public const byte vKeyLButton = 0x1;    // 鼠标左键
        public const byte vKeyRButton = 0x2;    // 鼠标右键
        public const byte vKeyCancel = 0x3;     // CANCEL 键
        public const byte vKeyMButton = 0x4;    // 鼠标中键
        public const byte vKeyBack = 0x8;       // BACKSPACE 键
        public const byte vKeyTab = 0x9;        // TAB 键
        public const byte vKeyClear = 0xC;      // CLEAR 键
        public const byte vKeyReturn = 0xD;     // ENTER 键
        public const byte vKeyShift = 0x10;     // SHIFT 键
        public const byte vKeyControl = 0x11;   // CTRL 键
        public const byte vKeyAlt = 18;         // Alt 键  (键码18)
        public const byte vKeyMenu = 0x12;      // MENU 键
        public const byte vKeyPause = 0x13;     // PAUSE 键
        public const byte vKeyCapital = 0x14;   // CAPS LOCK 键
        public const byte vKeyEscape = 0x1B;    // ESC 键
        public const byte vKeySpace = 0x20;     // SPACEBAR 键
        public const byte vKeyPageUp = 0x21;    // PAGE UP 键
        public const byte vKeyEnd = 0x23;       // End 键
        public const byte vKeyHome = 0x24;      // HOME 键
        public const byte vKeyLeft = 0x25;      // LEFT ARROW 键
        public const byte vKeyUp = 0x26;        // UP ARROW 键
        public const byte vKeyRight = 0x27;     // RIGHT ARROW 键
        public const byte vKeyDown = 0x28;      // DOWN ARROW 键
        public const byte vKeySelect = 0x29;    // Select 键
        public const byte vKeyPrint = 0x2A;     // PRINT SCREEN 键
        public const byte vKeyExecute = 0x2B;   // EXECUTE 键
        public const byte vKeySnapshot = 0x2C;  // SNAPSHOT 键
        public const byte vKeyDelete = 0x2E;    // Delete 键
        public const byte vKeyHelp = 0x2F;      // HELP 键
        public const byte vKeyNumlock = 0x90;   // NUM LOCK 键

        //常用键 字母键A到Z
        public const byte vKeyA = 65;
        public const byte vKeyB = 66;
        public const byte vKeyC = 67;
        public const byte vKeyD = 68;
        public const byte vKeyE = 69;
        public const byte vKeyF = 70;
        public const byte vKeyG = 71;
        public const byte vKeyH = 72;
        public const byte vKeyI = 73;
        public const byte vKeyJ = 74;
        public const byte vKeyK = 75;
        public const byte vKeyL = 76;
        public const byte vKeyM = 77;
        public const byte vKeyN = 78;
        public const byte vKeyO = 79;
        public const byte vKeyP = 80;
        public const byte vKeyQ = 81;
        public const byte vKeyR = 82;
        public const byte vKeyS = 83;
        public const byte vKeyT = 84;
        public const byte vKeyU = 85;
        public const byte vKeyV = 86;
        public const byte vKeyW = 87;
        public const byte vKeyX = 88;
        public const byte vKeyY = 89;
        public const byte vKeyZ = 90;

        //数字键盘0到9
        public const byte vKey0 = 48;    // 0 键
        public const byte vKey1 = 49;    // 1 键
        public const byte vKey2 = 50;    // 2 键
        public const byte vKey3 = 51;    // 3 键
        public const byte vKey4 = 52;    // 4 键
        public const byte vKey5 = 53;    // 5 键
        public const byte vKey6 = 54;    // 6 键
        public const byte vKey7 = 55;    // 7 键
        public const byte vKey8 = 56;    // 8 键
        public const byte vKey9 = 57;    // 9 键


        public const byte vKeyNumpad0 = 0x60;    //0 键
        public const byte vKeyNumpad1 = 0x61;    //1 键
        public const byte vKeyNumpad2 = 0x62;    //2 键
        public const byte vKeyNumpad3 = 0x63;    //3 键
        public const byte vKeyNumpad4 = 0x64;    //4 键
        public const byte vKeyNumpad5 = 0x65;    //5 键
        public const byte vKeyNumpad6 = 0x66;    //6 键
        public const byte vKeyNumpad7 = 0x67;    //7 键
        public const byte vKeyNumpad8 = 0x68;    //8 键
        public const byte vKeyNumpad9 = 0x69;    //9 键
        public const byte vKeyMultiply = 0x6A;   // MULTIPLICATIONSIGN(*)键
        public const byte vKeyAdd = 0x6B;        // PLUS SIGN(+) 键
        public const byte vKeySeparator = 0x6C;  // ENTER 键
        public const byte vKeySubtract = 0x6D;   // MINUS SIGN(-) 键
        public const byte vKeyDecimal = 0x6E;    // DECIMAL POINT(.) 键
        public const byte vKeyDivide = 0x6F;     // DIVISION SIGN(/) 键


        //F1到F12按键
        public const byte vKeyF1 = 0x70;   //F1 键
        public const byte vKeyF2 = 0x71;   //F2 键
        public const byte vKeyF3 = 0x72;   //F3 键
        public const byte vKeyF4 = 0x73;   //F4 键
        public const byte vKeyF5 = 0x74;   //F5 键
        public const byte vKeyF6 = 0x75;   //F6 键
        public const byte vKeyF7 = 0x76;   //F7 键
        public const byte vKeyF8 = 0x77;   //F8 键
        public const byte vKeyF9 = 0x78;   //F9 键
        public const byte vKeyF10 = 0x79;  //F10 键
        public const byte vKeyF11 = 0x7A;  //F11 键
        public const byte vKeyF12 = 0x7B;  //F12 键

        /// <param name="bVk" >按键的虚拟键值</param>
        /// <param name= "bScan" >扫描码，一般不用设置，用0代替就行</param>
        /// <param name= "dwFlags" >选项标志：0：表示按下，2：表示松开</param>
        /// <param name= "dwExtraInfo">一般设置为0</param>
        [DllImport("user32.dll")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);

        public static void keyPress(byte keyName)
        {
            keybd_event(keyName, 0, 0, 0);
            keybd_event(keyName, 0, 2, 0);
        }

        enum MouseEventFlag : uint
        {
            Move = 0x0001,
            LeftDown = 0x0002,
            LeftUp = 0x0004,
            RightDown = 0x0008,
            RightUp = 0x0010,
            MiddleDown = 0x0020,
            MiddleUp = 0x0040,
            XDown = 0x0080,
            XUp = 0x0100,
            Wheel = 0x0800,
            VirtualDesk = 0x4000,
            Absolute = 0x8000
        }

        public enum MouseButton : uint
        {
            Left = 0,
            Right=1
        }

        [DllImport("user32.dll")]
        static extern void mouse_event(MouseEventFlag flags, int dx, int dy, uint data, UIntPtr extraInfo);

        [DllImport("user32.dll")]
        public static extern int SetCursorPos(int x, int y);

        
        /// <summary>
        /// 查找窗体
        /// </summary>
        /// <param name="lpClassName">窗体的类名 例如对话框类是"#32770"</param>
        /// <param name="lpWindowName">窗体的标题 例如打开记事本 标题是"无标题 - 记事本" 注意 - 号两侧的空格</param>
        /// <returns>窗体的句柄</returns>
        [DllImport("user32.dll", EntryPoint = "FindWindow")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpClassName, string lpWindowName);
        /// <summary>
        /// 向窗体发送消息
        /// </summary>
        /// <param name="hWnd"> 窗体句柄</param>
        /// <param name="Msg">消息类型</param>
        /// <param name="wParam">附加的消息信息</param>
        /// <param name="lParam">附加的消息信息</param>
        /// <returns></returns>
        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, string lParam);
        /// <summary>
        /// 鼠标左键单击
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="data"></param>
        public static void MouseLeftClickEvent(int dx, int dy, uint data)
        {
            SetCursorPos(dx, dy);
            System.Threading.Thread.Sleep(1 * 50);
            mouse_event(MouseEventFlag.LeftDown, dx, dy, data, UIntPtr.Zero);
            System.Threading.Thread.Sleep(1 * 50);
            mouse_event(MouseEventFlag.LeftUp, dx, dy, data, UIntPtr.Zero);
        }

        /// <summary>
        /// 鼠标右键单击
        /// </summary>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="data"></param>
        public static void MouseRightClickEvent(int dx, int dy, uint data)
        {
            SetCursorPos(dx, dy);
            System.Threading.Thread.Sleep(1 * 50);
            mouse_event(MouseEventFlag.RightDown, dx, dy, data, UIntPtr.Zero);
            System.Threading.Thread.Sleep(1 * 50);
            mouse_event(MouseEventFlag.RightUp, dx, dy, data, UIntPtr.Zero);
        }

        public static void MouseClick(Point p,MouseButton b)
        {
            if (b== MouseButton.Left)
            {
                MouseLeftClickEvent(p.X, p.Y,0);
            }
            else
            {
                MouseRightClickEvent(p.X, p.Y, 0);
            }
        }
        public static void MouseClick(int x,int y, MouseButton b)
        {
            if (b== MouseButton.Left)
            {
                MouseLeftClickEvent(x, y,0);
            }
            else
            {
                MouseRightClickEvent(x, y, 0);
            }
        }
    }
}
