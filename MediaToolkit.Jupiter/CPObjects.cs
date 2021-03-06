﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;


namespace MediaToolkit.Jupiter
{

    public enum ResultCodes : int
    { //ControlPoint Protocol Manual p.176
        S_OK = 0,
        S_FALSE = 0x00000001,

        // Server errors
        E_INVALID_WINID = unchecked((int)0x80040301),
        E_NOTFOUND = unchecked((int)0x80040302),
        E_WINTYPEMISMATCH = unchecked((int)0x80040303),
        E_INVALID_ARGS = unchecked((int)0x80040304),
        E_INVALID_ARCHIVE_VERSION = unchecked((int)0x80040305),
        E_ARCHIVE_NOTFOUND = unchecked((int)0x80040306),
        E_WINID_ALLREADY_USED = unchecked((int)0x80040307),
        E_INVALID_FORMAT = unchecked((int)0x80040308),

        E_FILE_NOTEXIST = unchecked((int)0x80070002),
        E_WIN_CANNOT_SHOW_OR_REMOVE = unchecked((int)0x800705A9),

        //Protocol errors enough 
        E_PARS_NOT_ENOUGH = unchecked((int)0x80040501),
        E_TOMANY_PARS_SUPPLIED = unchecked((int)0x80040502),
        E_INVALID_METHODNAME = unchecked((int)0x80040503),
        E_INVALID_OBJECTNAME = unchecked((int)0x80040504),
        E_BAD_FORMAT = unchecked((int)0x80040505),

        //RGB Related Error Codes
        E_UNSUPPORTED_DISPLAY_FORMAT = unchecked((int)0x80040600),
        E_NO_TIMING_SELECTED = unchecked((int)0x80040601),
        E_NO_INPUT_SELECTED = unchecked((int)0x80040602),
        E_NO_DISPLAY_INFO_AVAILABLE = unchecked((int)0x80040603),
        E_ENGINE_ALREADY_RUNNING = unchecked((int)0x80040604),
        E_NO_AVAILABLE_ENGINE = unchecked((int)0x80040605),
        E_NO_DEVICE_SELECTED = unchecked((int)0x80040606),

        //..
    }

    public enum StateFlag
    {// ControlPoint Protocol Manual p. 122
     //Get -Set Flags

        wsVisible = 0x0001,
        wsMinimized = 0x0002,
        wsMaximized = 0x0004,
        wsFramed = 0x0008,
        wsLockAspect = 0x0010,
        wsAlwaysOnTop = 0x0020,
        wsPosition = 0x0400,
        wsSize = 0x0800,
        wsZOrder = 0x1000,
        wsTitle = 0x2000,

        //Notify Flags (read only)
        wsKind = 0x4000,
        wsChannel = 0x00010000,
        wsBalance = 0x00020000,
        wsFormat = 0x00040000,
        wsCrop = 0x00080000,

    }

    public enum SubSystemKind
    { // ControlPoint Protocol Manual p. 48
        None = 0,
        Galileo = 1,
        LiveVideo = 2,
        RGBCapture = 3,
        SystemWindow = 4,
        CPShare = 5,
        VidStream = 6,
        CPWeb = 7,
        PictureViewer = 8,
        CatalystLink = 9,
        IPStream = 10,
    }

    public class WinId
    {// ControlPoint Protocol Manual p. 50

        public WinId(string data) : this(TreeNode.Parse(data))
        { }

        internal WinId(TreeNode argsTree)
        {
            var valueList = argsTree.ValueList;
            if (valueList.Count == 1)
            {
                var _id = valueList[0];
                Id = int.Parse(_id);
            }
            else
            {
                throw new FormatException("WinId invalid argsTree");
            }
        }

        public WinId(int id)
        {
            Id = id;
        }

        // >0 and <10,000 for User Defined ID numbers
        //>10,000 for ControlPoint generated ID numbers
        public int Id { get; set; } = -1;

        public override string ToString()
        {
            return "{ " + Id + " }";
        }

    }

    public class WinIdList : List<WinId>
    {
        //"{ 3 { 10001 } { 10002 } { 123 } }"

        public WinIdList(string data) : this(TreeNode.Parse(data))
        { }

        public WinIdList(TreeNode argsTree)
        {
            var valueList = argsTree.ValueList;
            if (valueList.Count == 1)
            {
                var _count = valueList[0];
                var count = int.Parse(_count);
                var nodes = argsTree.Nodes;

                if (nodes.Count == count)
                {
                    foreach (var n in nodes)
                    {
                        var winId = new WinId(n);
                        this.Add(winId);
                    }
                }
                else
                {
                    throw new ArgumentException("WinId invalid argsTree");
                }
            }
            else
            {
                throw new ArgumentException("WinId invalid argsTree");
            }
        }

        public override string ToString()
        {
            return "{ " + this.Count + " " + string.Join(" ", this) + " }";
        }
    }

    public class TWindowState
    {
        // { { 101 } 2 9 7183 154 292 320 240 { 10003 } }

        public WinId Id { get; set; } = new WinId(-1);
        public SubSystemKind Kind { get; set; } = SubSystemKind.None;
        public uint State { get; set; } = 0;
        public uint StateChange { get; set; } = 0;
        public int x { get; set; } = 0;
        public int y { get; set; } = 0;
        public int w { get; set; } = 0;
        public int h { get; set; } = 0;

        /// <summary>
        /// WinId of window for this window to follow (-1=has focus and on top -2 for in back)
        /// </summary>
		public WinId ZAfter { get; set; } = new WinId(-1);

        public Rectangle Rect => new Rectangle(x, y, w, h);
        public bool IsVisible => ((State & (uint)StateFlag.wsVisible) != 0);
        public bool IsMinimized => ((State & (uint)StateFlag.wsMinimized) != 0);
        public bool IsMaximized => ((State & (uint)StateFlag.wsMaximized) != 0);
        public bool IsFramed => ((State & (uint)StateFlag.wsFramed) != 0);

        public bool IsClosed => (State == 0);

        public int WindowId => Id.Id;

        public TWindowState() { }
        public TWindowState(string data) : this(TreeNode.Parse(data))
        { }

        public TWindowState(TreeNode argsTree)
        {
            var valueList = argsTree.ValueList;
            var nodes = argsTree.Nodes;
            if (nodes.Count == 2 && valueList.Count == 7)
            {
                this.Id = new WinId(nodes[0]);
                this.ZAfter = new WinId(nodes[1]);

                this.Kind = (SubSystemKind)int.Parse(valueList[0]);

                this.State = IntParser.Parse<uint>(valueList[1]);
                this.StateChange = IntParser.Parse<uint>(valueList[2]);

                this.x = int.Parse(valueList[3]);
                this.y = int.Parse(valueList[4]);
                this.w = int.Parse(valueList[5]);
                this.h = int.Parse(valueList[6]);

            }
            else
            {
                throw new ArgumentException("argsTree");
            }

        }

        public override string ToString()
        {
            return "{ " + string.Join(" ", Id, (int)Kind, State, StateChange, x, y, w, h, ZAfter) + " }";
        }

    }

    public class TWindowStateList : List<TWindowState>
    {
        //"{ 2 { { 101 } 2 9 7183 154 292 320 240 { 10003 } } { { 102 } 3 1 7183 152 21 320 240 { 102 } } }

        public TWindowStateList(string data) : this(TreeNode.Parse(data))
        { }

        public TWindowStateList(TreeNode argsTree)
        {
            var valueList = argsTree.ValueList;
            if (valueList.Count == 1)
            {
                var _count = valueList[0];
                var count = int.Parse(_count);
                var nodes = argsTree.Nodes;

                if (nodes.Count == count)
                {
                    foreach (var n in nodes)
                    {
                        var winId = new TWindowState(n);
                        this.Add(winId);
                    }
                }
                else
                {
                    throw new ArgumentException("TWindowStateList invalid argsTree");
                }
            }
            else
            {
                throw new ArgumentException("TWindowStateList invalid argsTree");
            }
        }

        public override string ToString()
        {
            return "{ " + this.Count + " " + string.Join(" ", this) + " }";
        }
    }

    public class CPScreenConfig
    {// ControlPoint Protocol Manual p. 55

        /*
		 * 
			TotalWidth total width of screen (horizontal resolution timesdisplays)
			TotalHeight total height of screen (vertical resolution times displays)
			SingleScreenWidth horizontal screen resolution
			SingleScreenHeight vertical screen resolution

		 */

        public CPScreenConfig(string data) : this(TreeNode.Parse(data))
        { }


        public CPScreenConfig(TreeNode argsTree)
        {
            var valueList = argsTree.ValueList;
            var nodes = argsTree.Nodes;
            if (nodes.Count == 0 && valueList.Count == 4)
            {
                this.TotalWidth = int.Parse(valueList[0]);
                this.TotalHeight = int.Parse(valueList[1]);
                this.SingleScreenWidth = int.Parse(valueList[2]);
                this.SingleScreenHeight = int.Parse(valueList[3]);
            }
            else
            {
                throw new ArgumentException("argsTree");
            }

        }

        public int TotalWidth { get; private set; }
        public int TotalHeight { get; private set; }
        public int SingleScreenWidth { get; private set; }
        public int SingleScreenHeight { get; private set; }

        public override string ToString()
        {
            return string.Join(" ", TotalWidth, TotalHeight, SingleScreenWidth, SingleScreenHeight);
        }

    }

    public class CPSize
    {// ControlPoint Protocol Manual p. 53

        public CPSize() { }

        public CPSize(string data) : this(TreeNode.Parse(data))
        { }

        public CPSize(TreeNode argsTree)
        {
            var valueList = argsTree.ValueList;
            var nodes = argsTree.Nodes;
            if (nodes.Count == 0 && valueList.Count == 2)
            {
                this.cx = int.Parse(valueList[0]);
                this.cy = int.Parse(valueList[1]);
            }
            else
            {
                throw new ArgumentException("CPSize invalid argsTree: " + nodes.Count + " " + valueList.Count);
            }

        }

        public int cx { get; set; }
        public int cy { get; set; }

        public Size Size => new Size(cx, cy);

        public override string ToString()
        {
            return string.Join(" ", cx, cy);
        }
    }

    public class CPRect
    {// ControlPoint Protocol Manual p. 53

        public CPRect() { }

        public CPRect(string data) : this(TreeNode.Parse(data))
        { }

        public CPRect(TreeNode argsTree)
        {
            var valueList = argsTree.ValueList;
            var nodes = argsTree.Nodes;
            if (nodes.Count == 0 && valueList.Count == 4)
            {
                this.nX = int.Parse(valueList[0]);
                this.nY = int.Parse(valueList[1]);
                this.nW = int.Parse(valueList[2]);
                this.nH = int.Parse(valueList[3]);
            }
            else
            {
                throw new ArgumentException("argsTree");
            }
        }

        public int nX { get; set; }
        public int nY { get; set; }
        public int nW { get; set; }
        public int nH { get; set; }

        public override string ToString()
        {
            return string.Join(" ", nX, nY, nW, nH);
        }
    }

    public class CPServerInfo
    {// ControlPoint Protocol Manual p. 56

        /*
		 *	unsigned long dwVersionMS
			unsigned long dwVersionLS
			unsigned long dwFileTimeMS
			unsigned long dwdwFileTimeLS

			dwVersionMS CPServer Version (high bytes)
			dwVersionLS CPServer Version (low bytes)
			dwFileTimeMS Write time for CPServer (high bytes)
			dwdwFileTimeLS Write time for CPServer (low bytes)
		 */

        public CPServerInfo(string data) : this(TreeNode.Parse(data))
        { }


        public CPServerInfo(TreeNode argsTree)
        {// TODO:
         // { 196613 514589198 30406492 -1941052928 } 

            var valueList = argsTree.ValueList;
            var nodes = argsTree.Nodes;
            if (nodes.Count == 0 && valueList.Count == 4)
            {
                this.VersionMS = uint.Parse(valueList[0]);
                this.VersionLS = uint.Parse(valueList[1]);
                this.FileTimeMS = uint.Parse(valueList[2]);
                this.FileTimeLS = uint.Parse(valueList[3]);
            }
            else
            {
                throw new ArgumentException("argsTree");
            }

        }

        public uint VersionMS { get; private set; }
        public uint VersionLS { get; private set; }
        public uint FileTimeMS { get; private set; }
        public uint FileTimeLS { get; private set; }

        public override string ToString()
        {
            return string.Join(" ", VersionMS, VersionLS, FileTimeMS, FileTimeLS);
        }
    }

    public class CPPlatformInfo
    {
        public readonly static Dictionary<long, string> PlatformCodes = new Dictionary<long, string>
        {
            {18, "FC 1000" },
            {19, "FC 4000" },
            {20, "FC 8000" },
            {21, "FC 1100" },
            {22, "VizionPlus II" },
            {23, "FC4500" },
        };

        public static string GetPlatformName(long code) 
        {
            string platformName = "PlatformCode " + code;
            if (PlatformCodes.ContainsKey(code)) 
            {
                platformName = PlatformCodes[code];
            }

            return platformName;
        }


        public CPPlatformInfo(string data) : this(TreeNode.Parse(data))
        { }

        public CPPlatformInfo(TreeNode argsTree)
        {
            //"{ 19 0 1 0 1199 } \"3.5.7852.526\""

            var valueList = argsTree.ValueList;
            var nodes = argsTree.Nodes;
            if (nodes.Count == 1 && valueList.Count == 1)
            {
                var _valueList = nodes[0].ValueList;
                if (_valueList.Count == 5)
                {
                    this.PlatformCode = long.Parse(_valueList[0]);
                    this.ModelVersion = long.Parse(_valueList[1]);
                    this.ModelRevision = long.Parse(_valueList[2]);
                    this.OEMCode = long.Parse(_valueList[3]);
                    this.SerialNumber = long.Parse(_valueList[4]);
                }
                else
                {
                    throw new ArgumentException("argsTree");
                }

                this.CPVersion = valueList[0];
            }
            else
            {
                throw new ArgumentException("argsTree");
            }

        }

        public long PlatformCode { get; private set; }
        public long ModelVersion { get; private set; }
        public long ModelRevision { get; private set; }
        public long OEMCode { get; private set; }
        public long SerialNumber { get; private set; }

        public string CPVersion { get; private set; } = "";

        public string GetDescription() 
        {
            return "Jupiter Info: " + string.Join("/", CPVersion.Trim('"'), GetPlatformName(PlatformCode), ModelVersion, ModelRevision, OEMCode, SerialNumber);

        }

        public override string ToString()
        {
            return "{ " + string.Join(" ", PlatformCode, ModelVersion, ModelRevision, OEMCode) + " } " + CPVersion;
        }

    }

    public class CPRGBTiming
    {// ControlPoint Protocol Manual p. 54
     /*
      * bValid is valid – Boolean 0 or 1
         nWidth width of RGB image
         nHTotal horizontal total of RGB Image
         nHOffset horizontal offset
         nHeight height of RGB image
         nVTotal total vertical pixels
         nVOffset vertical offset
         nPhase RGB Pixel Phase
         nVFreq scan frequency
         nSyncType sync type
         bHsynNeg horizontal negative sync – Boolean – 0 or 1
         bVsyncNeg vertical negative sync – Boolean – 0 or 1
      */

        // { 1 640 480 144 480 525 35 3 60 0 0 0 }

        // { 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 } 
        public CPRGBTiming(string data) : this(TreeNode.Parse(data))
        { }

        public CPRGBTiming(TreeNode argsTree)
        {
            var valueList = argsTree.ValueList;
            var nodes = argsTree.Nodes;
            if (nodes.Count == 0 && valueList.Count == 11)
            {
                this.bValid = int.Parse(valueList[0]);
                this.nWidth = int.Parse(valueList[1]);
                this.nHTotal = int.Parse(valueList[2]);
                this.nHOffset = int.Parse(valueList[3]);
                this.nVTotal = int.Parse(valueList[4]);
                this.nVOffset = int.Parse(valueList[5]);
                this.nPhase = int.Parse(valueList[6]);
                this.nVFreq = int.Parse(valueList[7]);
                this.nSyncType = int.Parse(valueList[8]);
                this.bHsynNeg = int.Parse(valueList[9]);
                this.bVsyncNeg = int.Parse(valueList[10]);
            }
            else
            {
                throw new ArgumentException("argsTree");
            }

        }

        public int bValid { get; private set; } //Boolean 0 or 1
        public int nWidth { get; private set; }
        public int nHTotal { get; private set; }
        public int nHOffset { get; private set; }
        public int nVTotal { get; private set; }
        public int nVOffset { get; private set; }
        public int nPhase { get; private set; }
        public int nVFreq { get; private set; }
        public int nSyncType { get; private set; }
        public int bHsynNeg { get; private set; }//Boolean 0 or 1
        public int bVsyncNeg { get; private set; }//Boolean 0 or 1

        public override string ToString()
        {
            return "{ " + string.Join(" ", bValid, nWidth, nHTotal, nHOffset, nVTotal, nVOffset, nPhase, nVFreq, nSyncType, bHsynNeg, bVsyncNeg) + " }";
        }

    }

    public class CPWndFrameInfo
    {//ControlPoint Protocol Manual p.63
		 //=00000000 { 15 4 8421504 1 0 }

		 /*
		  * 
		  *  unsigned Set
			 short nFrameWidth
			 DWORD rgbFrameColor
			 Boolean bShowTitle
			 Boolean bShowUserData


		  *  Set * bit field – low order 3 bits (0000 0111)
			 nFrameWidth frame with – 0 to 100 (default = 4)
			 rgbFrameColor frame color – RGB(R, G, B) R/G/B - 0 to 255
			 bShowTitle indicates show/not show title (default = true)
			 bShowUserData indicates show/not show user data (default = false)

			 *Set argument is used to determine which parameters are to be changed (are valid).
		  */
		public CPWndFrameInfo()
		{ }
		public CPWndFrameInfo(string data) : this(TreeNode.Parse(data))
		{ }

		public CPWndFrameInfo(TreeNode argsTree)
		{
			var valueList = argsTree.ValueList;
			var nodes = argsTree.Nodes;

			if (nodes.Count == 0 && valueList.Count == 5)
			{
				this.Set = uint.Parse(valueList[0]);
				this.nFrameWidth = short.Parse(valueList[1]);
				this.rgbFrameColor = uint.Parse(valueList[2]);
				this.bShowTitle = int.Parse(valueList[3]);
				this.bShowUserData = int.Parse(valueList[4]);

			}
			else
			{
				throw new ArgumentException("argsTree");
			}

		}

        public uint Set { get; set; } = 15;// 1111 -> 15 ALL

        public short nFrameWidth { get; set; } = 2;// 0 to 100 (default = 4)

        public uint rgbFrameColor { get; set; } = 255;//255 RED

        public int bShowTitle { get; set; } = 0; //Boolean 0 or 1

        public int bShowUserData { get; set; } = 0; //Boolean 0 or 1

		public override string ToString()
		{
			return "{ " + string.Join(" ", Set, nFrameWidth, rgbFrameColor, bShowTitle, bShowUserData) + " }";
		}
	}



    public abstract class CPObjBase
    {
        public CPObjBase(CPClient c)
        {
            this.client = c;
        }

        protected readonly CPClient client = null;
        public abstract string ObjectName { get; }

        protected void ThrowIfClientNotReady()
        {
            if (!client.IsConnected)
            {
                throw new Exception("Not connected");
            }

            if (!client.IsAuthenticated)
            {
                throw new Exception("Not authenticated");
            }

            //...
        }
    }

    public class ConfigSys : CPObjBase
    { // ControlPoint Protocol Manual p.69
      /*
	   *	GetServerInfo ( [out] CPPlatformInfo,[out,string] versionInfo )
			ListCfgGroup ( [in] group_code,[out,string] objNames )
	   */

        public ConfigSys(CPClient c) : base(c) { }

        public override string ObjectName => "ConfigSys";

        public async Task<CPPlatformInfo> GetServerInfo()
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "GetServerInfo");
            var response = await client.SendAsync(request) as CPResponse;

            response.ThrowIfError();

            var data = "{ " + response.ValueList + " }";

            return new CPPlatformInfo(data);
        }

    }

    public class PrevImage
    {
        public PrevImage(CPResponse resp)
        {
            this.ValueList = resp.ValueList;

            var pars = ValueList.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            this.Width = int.Parse(pars[0]);
            this.Height = int.Parse(pars[1]);
            //this.Bits = resp.RawData;

            this.binary = resp.BinData;

            const int headerSize = 4;
            var header = new byte[headerSize];
            Array.Copy(binary, header, header.Length);

            var bitsSize = BitConverter.ToInt32(header, 0);
            var realBitsSize = binary.Length - headerSize;

            if (realBitsSize != bitsSize)
            {
                throw new FormatException("Invalid bin data length. " + realBitsSize + "!=" + Bits.Length);
            }

            this.Bits = new byte[bitsSize];
            Array.Copy(binary, headerSize, Bits, 0, Bits.Length);

        }

        public readonly string ValueList = "";

        // максимальный размер по умолчанию 128x128, 
        // но можно увеличить в конфиге %ProgramData%\ControlPoint\ServerDataFiles\Config.xml
        public int Width { get; private set; }
        public int Height { get; private set; }

        // данные в формате 16 bit RGB555
        public byte[] Bits { get; private set; }

        private byte[] binary = null;
        public Bitmap GetBitmap()
        {
            if (!(Width > 0 && Height > 0))
            {
                throw new FormatException("Invalid size: " + Width + "x" + Height);
            }

            //преобразование вручную rgb555 -> rgb888 
            /*             
            //https://docs.microsoft.com/en-us/windows/win32/directshow/working-with-16-bit-rgb
            ushort red_mask = 0x7C00;
            ushort green_mask = 0x3E0;
            ushort blue_mask = 0x1F;

            var size = bits.Count() / sizeof(ushort);
            var pixels = new ushort[size];
            for (var index = 0; index < size; index++)
            {
                pixels[index] = BitConverter.ToUInt16(bits, index * sizeof(ushort));
            }

            foreach (var pixel in pixels)
            {   
                var red_value = (pixel & red_mask) >> 10;
                var green_value = (pixel & green_mask) >> 5;
                var blue_value = (pixel & blue_mask);

                byte red = (byte)(red_value << 3);
                byte green = (byte)(green_value << 3);
                byte blue = (byte)(blue_value << 3);
            }
            */

            var bmp = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);

            var data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, bmp.PixelFormat);
            try
            {
                var bmpStride = data.Stride;
                var bmpSize = data.Height * bmpStride;
                if (bmpSize == Bits.Length)
                {
                    Marshal.Copy(Bits, 0, data.Scan0, Bits.Length);
                }
                else
                {// invalid format...
                    throw new FormatException("Invalid bitmap length: " + bmpSize + "!=" + Bits.Length);             
                }
            }
            finally
            {
                bmp.UnlockBits(data);
            }

            return bmp;
        }
    }

    public class GalWinSys : CPObjBase
    {// ControlPoint Protocol Manual p.76

        /*
		 *	GetKind ( [out] SubSystemKind_t )
			IsOfKind ( [in] WinId_t )
			QueryAllWindows ( [out] TWindowState_array_t )
			NewWindow ( [out] WinId_t )
			NewWindowWithId ( [in] WinId_t )
			Start ( [in] WinId_t )
			Stop ( [in] WinId_t )
			Freeze ( [in] WinId_t )
			SetCrop ( [in] WinId_t wid, [in] struct CPRect * pRect )
			SetOrigin ( [in] WinId_t wid, [in] long x, [in] long y )
			GetCrop ( [in] WinId_t wid, [out] struct CPRect * pRect )
			GetImgBalance ( [in] WinId_t, [out] ImgBalance )
			SetImgBalance ( [in] WinId_t, [in, out] ImgBalance )
			GetInputSize ( [in] WinId_t, [out] CPSize )
			ApplyDefaults ( [in] WinId_t)
			QueryAllInputsCS ( [out] )
			SelectInput ( [in] WinId_t wid, inputName )
			GetInput ( [in] WinId_t wid, [out, inputName] )
		 */

        public GalWinSys(CPClient c) : base(c) { }

        public override string ObjectName => "GalWinSys";

        public async Task<SubSystemKind> GetKind()
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "GetKind");
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return (SubSystemKind)Enum.Parse(typeof(SubSystemKind), response.ValueList);

        }

        public async Task<TWindowStateList> QueryAllWindows()
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "QueryAllWindows");
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return new TWindowStateList(response.ValueList);
        }

        public async Task<WinId> NewWindow()
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "NewWindow");
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return new WinId(response.ValueList);
        }


        public async Task<bool> NewWindowWithId(WinId winId)
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "NewWindowWithId", winId);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();
            return response.Result;
        }

        public async Task<bool> Start(WinId winId)
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "Start", winId);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();
            return response.Result;
        }

        public async Task<bool> Freeze(WinId winId)
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "Freeze", winId);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();
            return response.Result;
        }

        public async Task<bool> Stop(WinId winId)
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "Stop", winId);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return response.Result;
        }


        public async Task<CPSize> GetInputSize(WinId winId)
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "GetInputSize", winId);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return new CPSize(response.ValueList);
        }

        public async Task<string> GetInput(WinId winId)
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "GetInput", winId);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return response.ValueList;
        }

        public async Task<bool> SelectInput(WinId winId, string inputName)
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "SelectInput", winId, inputName);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return response.Result;
        }
    }

    public class RGBSys : GalWinSys
    {//ControlPoint Protocol Manual p.90
     /*
		* 
		SetChannel ( [out] WinId_t, [in] short nChannel )
		GetChannel ( [in] WinId_t, [out] short * pnChannel )
		GetChannelRange ( [out] short * FirstCh, [out] short * LastCh )
		SetTiming ( [in] WinId_t, [in] CPRGBTiming * pTiming )
		GetTiming ( [in] WinId_t, [out] CPRGBTiming ) * pTiming )
		DetectTiming ( [in] WinId_t, [out] CPRGBTiming ) * pTiming )
		SetAutoDetectTiming ( [in] WinId_t wid, [in] Boolean bEnable )
		GetAutoDetectTiming ( [in] WinId_t wid, [out] Boolean * bEnabled )
		SetRCServer ( [in] WinId_t wid, [in, string] wchar_t * serverName )
		GetRCServer ( [in] WinId_t wid, [out, string] wchar_t ** serverName )
		SetDualLink ( [in] WinId_t wid, [in] Boolean bEnable )
		GetDualLink ( [in] WinId_t wid, [out] Boolean * bEnabled )
		SetComponent ( [in] WinId_t wid, [in] Boolean bEnable )
		GetComponent ( [in] WinId_t wid, [out] Boolean * bEnabled )
	  */

        public RGBSys(CPClient c) : base(c) { }

        public override string ObjectName => "RGBSys";


        public async Task<bool> SetAutoDetectTiming(WinId winId, bool enable)
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "SetAutoDetectTiming", winId, enable ? 1 : 0);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();
            return response.Result;
        }

        public async Task<int> GetChannel(WinId winId)
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "GetChannel", winId);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            int chNumber = int.Parse(response.ValueList);

            return chNumber;
        }

        public async Task<bool> SetChannel(WinId winId, int chNumber)
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "SetChannel", winId, chNumber);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return response.Result;
        }

        public async Task<Tuple<int, int>> GetChannelRange()
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "GetChannelRange");
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            var valueList = response.ValueList;
            var parts = valueList.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);

            var firstCh = int.Parse(parts[0]);
            var lastCh = int.Parse(parts[1]);

            return new Tuple<int, int>(firstCh, lastCh);
        }

        public async Task<CPRGBTiming> DetectTiming(WinId winId)
        {   // неработает
            // { 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 } <- сервер возвращает такую хрень...
            // вместо 12 аргументов - 15
            ThrowIfClientNotReady();

            var request = new CPRequest(ObjectName, "DetectTiming", winId);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            var data = response.ValueList;
            return new CPRGBTiming(data);
        }

    }

    public class Notify : CPObjBase
    {//ControlPoint Protocol Manual p.84

        /*
		  * WindowState ( [in] TWindowState_array_t )
			ScreenConfigChanged ( [in] CPScreenConfig )
		*/
        public Notify(CPClient c) : base(c)
        {
            base.client.NotificationReceived += Client_NotificationReceived;
        }

        public event Action<TWindowStateList> WindowStateEvent;

        public override string ObjectName => "Notify";

        private void Client_NotificationReceived(CPNotification notification)
        {
            var method = notification.Method;
            var obj = notification.ObjectName;
            if (obj == ObjectName)
            {
                if (method == "WindowsState")
                {
                    var data = notification.ValueList;
                    var windows = new TWindowStateList(data);

                    WindowStateEvent?.Invoke(windows);
                }
            }
        }
    }

    public class WinServer : CPObjBase
    { //ControlPoint Protocol Manual p.116

        /*
         *  DeleteWindow ( [in] WinId_t )
            QueryAllWindows ( [out] TWindowState_array_t )
            QueryWindows ( [in] WinId_t_array_t,
            [out] TwindowState_array_t )
            FindWindow ( [in,string] window_descriptor,
            [out] WinId_t )
            InvokeAppWindow ( [in,string] appWinName,
            [out] WinId_t )
            GetAppWinInfo ( [in] WinId_t winid,
            [out,string] window_descriptor,
            [out,string] cmdline,
            [out_string] workDir )
            RegisterNotifyTarget ( )
            UnregisterNotifyTarget ( )
            GetServerInfo ( [out] CPServerInfo )
            GetScreenConfig ( [out] CPScreenConfig )
            Quit ( )
            QueryAllLayoutsCS ( [out, string] )
            QueryLastSetLayout ( [out, string] )
            SetLayout ( [in, string] )
            SaveLayout ( [in, string],
            [in] WinId_t_array_t )
            DeleteLayout ( [in, string] )
         */

        public WinServer(CPClient c) : base(c) { }

        public override string ObjectName => "WinServer";


        public async Task<bool> DeleteWindow(WinId winId)
        {
            ThrowIfClientNotReady();

            var request = new CPRequest(ObjectName, "DeleteWindow", winId);
            var response = await client.SendAsync(request) as CPResponse;

            response.ThrowIfError();

            return response.Result;
        }

        public async Task<TWindowStateList> QueryAllWindows()
        {

            ThrowIfClientNotReady();

            var request = new CPRequest(ObjectName, "QueryAllWindows");
            var response = await client.SendAsync(request) as CPResponse;

            response.ThrowIfError();

            return new TWindowStateList(response.ValueList);
        }


        public async Task<CPServerInfo> GetServerInfo()
        { // не работает !! 
          // сервер возвращает такое:
          // { 196613 514589198 30406492 -1941052928 } 
          // вместо сктрктуры из четырех DWORD
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "GetServerInfo");
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return new CPServerInfo(response.ValueList);
        }


        public async Task<CPScreenConfig> GetScreenConfig()
        {
            ThrowIfClientNotReady();

            var request = new CPRequest(ObjectName, "GetScreenConfig");
            var response = await client.SendAsync(request) as CPResponse;

            response.ThrowIfError();

            return new CPScreenConfig(response.ValueList);
        }

        public async Task<bool> RegisterNotifyTarget()
        {
            ThrowIfClientNotReady();

            var request = new CPRequest(ObjectName, "RegisterNotifyTarget");
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return response.Result;
        }

        public async Task<bool> UnregisterNotifyTarget()
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "UnregisterNotifyTarget");
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return response.Result;
        }

        public async Task<bool> Quit()
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "Quit");
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return response.Result;

        }
    }

    public class Window : CPObjBase
    {//ControlPoint Protocol Manual p.108

        /*
         *  GetState ( [in] WinId_t, [out] TWindowState )
            SetState ( [in, out] TWindowState )
            GetTitle ( [in] WinId_t, [out, string] title )
            SetTitle ( [in] WinId_t, [in, string] title )
            GetFrameInfo ([in] WinId_t wid, [out] struct CPWndFrameInfo * fi)
            SetFrameInfo ([in] WinId_t wid, [in] struct CPWndFrameInfo * fi)
            GetTitleInfo ( [in] WinId_t, [out] struct CPWndTitleInfo * ti)
            SetTitleInfo ( [in] WinId_t, [in] struct CPWndTitleInfo * ti)
            GetTitleFontInfo ([in] WinId_t wid, [out] struct CPWndTitleFontInfo * tfi)
            SetTitleFontInfo ([in] WinId_t wid, [in] struct CPWndTitleFontInfo * tfi)
            GrabImage ([in] WinId_t, [out,string] wchar_t ** )
         */

        public Window(CPClient c) : base(c) { }
        public override string ObjectName => "Window";

        public async Task<TWindowState> GetState(WinId winId)
        {
            ThrowIfClientNotReady();

            var request = new CPRequest(ObjectName, "GetState", winId);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return new TWindowState(response.ValueList);
        }

        public async Task<TWindowState> SetState(TWindowState state)
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "SetState", state);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return new TWindowState(response.ValueList);
        }

        public async Task<CPWndFrameInfo> GetFrameInfo(WinId winId)
        {
            ThrowIfClientNotReady();

            var request = new CPRequest(ObjectName, "GetFrameInfo", winId);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return new CPWndFrameInfo(response.ValueList);
        }

        public async Task<bool> SetFrameInfo(WinId winId, CPWndFrameInfo fi)
        {
            ThrowIfClientNotReady();

            var request = new CPRequest(ObjectName, "SetFrameInfo", winId, fi);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return response.Result;
        }

		public async Task<string> GrabImage(WinId winId)
        { // ContorlPoint Server ver_3.5.7852.526 утекает память !!!

            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "GrabImage", winId);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();
            return response.ValueList.Replace("\"", "");

        }

        public async Task<PrevImage> GetPreview(WinId winId)
        {// недокументированный метод...
         // используется для получения эскизов форм в ContorlPoint-е
         //= 00000000 128 96DLE...rgb555...\r\n

            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "GetPreview", winId);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return response.Result ? new PrevImage(response) : null;
        }

    }

    public class CPDebug : CPObjBase
    { //ControlPoint Protocol Manual p.74 
        /*
         *  CloseFile ()
            FlushToFile ( [in, string] )
            GetFileName ( [out, string] )
            GetLevel ( [out] unsigned )
            GetOutputs ( [out] DWORD )
            OpenFile ( [in, string] )
            SetOutputs ( [in] DWORD new, [out] DWORD old )
            SetLevel ( [in] unsigned NewLevel, [out] unsigned OldLevel )
         */
        public CPDebug(CPClient c) : base(c) { }

        public override string ObjectName => "Debug";

        public async Task<bool> CloseFile()
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "CloseFile");
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return response.Result;
        }

        public async Task<bool> FlushToFile(string fileName)
        {
            ThrowIfClientNotReady();
            var arg0 = Utils.AutoDoubleQuoteString(fileName);
            var request = new CPRequest(ObjectName, "FlushToFile", arg0);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return response.Result;
        }

        public async Task<bool> OpenFile(string fileName)
        {// можно задать только имя файла, путь настроить нельзя 
            // %ProgramData%\ControlPoint\

            ThrowIfClientNotReady();

            var arg0 = Utils.AutoDoubleQuoteString(fileName);
            var request = new CPRequest(ObjectName, "OpenFile", arg0);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return response.Result;
        }


        public async Task<ushort> GetLevel()
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "GetLevel");
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return IntParser.Parse<ushort>(response.ValueList);
        }

        public async Task<ushort> SetLevel(ushort level)
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "SetLevel", level);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return IntParser.Parse<ushort>(response.ValueList);
        }

        public async Task<uint> GetOutputs()
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "GetOutputs");
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return IntParser.Parse<uint>(response.ValueList);
        }

        public async Task<uint> SetOutputs(uint output )
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "SetOutputs", output);
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return IntParser.Parse<uint>(response.ValueList);
        }

        public async Task<string> GetFileName()
        {
            ThrowIfClientNotReady();
            var request = new CPRequest(ObjectName, "GetFileName");
            var response = await client.SendAsync(request) as CPResponse;
            response.ThrowIfError();

            return response.ValueList;

        }
    }
}
