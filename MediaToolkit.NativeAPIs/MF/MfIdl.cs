﻿using MediaToolkit.NativeAPIs.MF.Objects;
using MediaToolkit.NativeAPIs.Ole;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace MediaToolkit.NativeAPIs.MF
{

	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
		Guid("7FEE9E9A-4A89-47A6-899C-B6A53A70FB67"),
		InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMFActivate : IMFAttributes
	{
		#region IMFAttributes methods

		[PreserveSig]
		new HResult GetItem(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFActivate.GetItem", MarshalTypeRef = typeof(Utils.PVMarshaler))] PropVariant pValue
			);

		[PreserveSig]
		new HResult GetItemType(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out MFAttributeType pType
			);

		[PreserveSig]
		new HResult CompareItem(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new HResult Compare(
			[MarshalAs(UnmanagedType.Interface)] IMFAttributes pTheirs,
			MFAttributesMatchType MatchType,
			[MarshalAs(UnmanagedType.Bool)] out bool pbResult
			);

		[PreserveSig]
		new HResult GetUINT32(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int punValue
			);

		[PreserveSig]
		new HResult GetUINT64(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out long punValue
			);

		[PreserveSig]
		new HResult GetDouble(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out double pfValue
			);

		[PreserveSig]
		new HResult GetGUID(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out Guid pguidValue
			);

		[PreserveSig]
		new HResult GetStringLength(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcchLength
			);

		[PreserveSig]
		new HResult GetString(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszValue,
			int cchBufSize,
			out int pcchLength
			);

		[PreserveSig]
		new HResult GetAllocatedString(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[MarshalAs(UnmanagedType.LPWStr)] out string ppwszValue,
			out int pcchLength
			);

		[PreserveSig]
		new HResult GetBlobSize(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out int pcbBlobSize
			);

		[PreserveSig]
		new HResult GetBlob(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[Out, MarshalAs(UnmanagedType.LPArray)] byte[] pBuf,
			int cbBufSize,
			out int pcbBlobSize
			);

		// Use GetBlob instead of this
		[PreserveSig]
		new HResult GetAllocatedBlob(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			out IntPtr ip,  // Read w/Marshal.Copy, Free w/Marshal.FreeCoTaskMem
			out int pcbSize
			);

		[PreserveSig]
		new HResult GetUnknown(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
			[MarshalAs(UnmanagedType.IUnknown)] out object ppv
			);

		[PreserveSig]
		new HResult SetItem(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant Value
			);

		[PreserveSig]
		new HResult DeleteItem(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey
			);

		[PreserveSig]
		new HResult DeleteAllItems();

		[PreserveSig]
		new HResult SetUINT32(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			int unValue
			);

		[PreserveSig]
		new HResult SetUINT64(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			long unValue
			);

		[PreserveSig]
		new HResult SetDouble(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			double fValue
			);

		[PreserveSig]
		new HResult SetGUID(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidValue
			);

		[PreserveSig]
		new HResult SetString(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In, MarshalAs(UnmanagedType.LPWStr)] string wszValue
			);

		[PreserveSig]
		new HResult SetBlob(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)] byte[] pBuf,
			int cbBufSize
			);

		[PreserveSig]
		new HResult SetUnknown(
			[MarshalAs(UnmanagedType.LPStruct)] Guid guidKey,
			[In, MarshalAs(UnmanagedType.IUnknown)] object pUnknown
			);

		[PreserveSig]
		new HResult LockStore();

		[PreserveSig]
		new HResult UnlockStore();

		[PreserveSig]
		new HResult GetCount(
			out int pcItems
			);

		[PreserveSig]
		new HResult GetItemByIndex(
			int unIndex,
			out Guid pguidKey,
			[In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalCookie = "IMFActivate.GetItemByIndex", MarshalTypeRef = typeof(Utils.PVMarshaler))] PropVariant pValue
			);

		[PreserveSig]
		new HResult CopyAllItems(
			[In, MarshalAs(UnmanagedType.Interface)] IMFAttributes pDest
			);

		#endregion

		[PreserveSig]
		HResult ActivateObject(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
			[MarshalAs(UnmanagedType.Interface)] out object ppv
			);

		[PreserveSig]
		HResult ShutdownObject();

		[PreserveSig]
		HResult DetachObject();
	}

	[StructLayout(LayoutKind.Explicit, Pack = 1),]
    public struct MFPaletteEntry
    {
        [FieldOffset(0)]
        public MFARGB ARGB;
        [FieldOffset(0)]
        public MFAYUVSample AYCbCr;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MFAYUVSample
    {
        public byte bCrValue;
        public byte bCbValue;
        public byte bYValue;
        public byte bSampleAlpha8;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public class MFARGB
    {
        public byte rgbBlue;
        public byte rgbGreen;
        public byte rgbRed;
        public byte rgbAlpha;

        public int ToInt32()
        {
            return (rgbBlue) | (rgbGreen << 8) | (rgbRed << 16) | (rgbAlpha << 24);
        }
    }



    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("992388B4-3372-4F67-8B6F-C84C071F4751")]
    public interface IMFVideoSampleAllocatorCallback
    {
        [PreserveSig]
        HResult SetCallback(IMFVideoSampleAllocatorNotify pNotify);

        [PreserveSig]
        HResult GetFreeSampleCount(out int plSamples);
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("A792CDBE-C374-4E89-8335-278E7B9956A4")]
    public interface IMFVideoSampleAllocatorNotify
    {
        [PreserveSig]
        HResult NotifyRelease();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("0A97B3CF-8E7C-4A3D-8F8C-0C843DC247FB"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFStreamSink : IMFMediaEventGenerator
    {
        #region IMFMediaEventGenerator methods

        [PreserveSig]
        new HResult GetEvent(
            [In] MFEventFlag dwFlags,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        new HResult BeginGetEvent(
            [In, MarshalAs(UnmanagedType.Interface)] IMFAsyncCallback pCallback,
            [In, MarshalAs(UnmanagedType.IUnknown)] object o
            );

        [PreserveSig]
        new HResult EndGetEvent(
            IMFAsyncResult pResult,
            out IMFMediaEvent ppEvent
            );

        [PreserveSig]
        new HResult QueueEvent(
            [In] MediaEventType met,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidExtendedType,
            [In] HResult hrStatus,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvValue
            );

        #endregion

        [PreserveSig]
        HResult GetMediaSink(
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaSink ppMediaSink
            );

        [PreserveSig]
        HResult GetIdentifier(
            out int pdwIdentifier
            );

        [PreserveSig]
        HResult GetMediaTypeHandler(
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaTypeHandler ppHandler
            );

        [PreserveSig]
        // HResult ProcessSample([In, MarshalAs(UnmanagedType.Interface)] IMFSample pSample );
        HResult ProcessSample([In] IntPtr pSample);

        [PreserveSig]
        HResult PlaceMarker(
            [In] MFStreamSinkMarkerType eMarkerType,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarMarkerValue,
            [In, MarshalAs(UnmanagedType.LPStruct)] ConstPropVariant pvarContextValue
            );

        [PreserveSig]
        HResult Flush();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("E93DCF6C-4B07-4E1E-8123-AA16ED6EADF5"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaTypeHandler
    {
        [PreserveSig]
        HResult IsMediaTypeSupported(
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pMediaType,
            IntPtr ppMediaType  //[MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppMediaType
            );

        [PreserveSig]
        HResult GetMediaTypeCount(
            out int pdwTypeCount
            );

        [PreserveSig]
        HResult GetMediaTypeByIndex(
            [In] int dwIndex,
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppType
            );

        [PreserveSig]
        HResult SetCurrentMediaType(
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pMediaType
            );

        [PreserveSig]
        HResult GetCurrentMediaType(
            [MarshalAs(UnmanagedType.Interface)] out IMFMediaType ppMediaType
            );

        [PreserveSig]
        HResult GetMajorType(
            out Guid pguidMajorType
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        Guid("6EF2A660-47C0-4666-B13D-CBB717F2FA2C"),
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFMediaSink
    {
        [PreserveSig]
        HResult GetCharacteristics(
            out MFMediaSinkCharacteristics pdwCharacteristics
            );

        [PreserveSig]
        HResult AddStreamSink(
            [In] int dwStreamSinkIdentifier,
            [In, MarshalAs(UnmanagedType.Interface)] IMFMediaType pMediaType,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSink ppStreamSink
            );

        [PreserveSig]
        HResult RemoveStreamSink(
            [In] int dwStreamSinkIdentifier
            );

        [PreserveSig]
        HResult GetStreamSinkCount(
            out int pcStreamSinkCount
            );

        [PreserveSig]
        HResult GetStreamSinkByIndex(
            [In] int dwIndex,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSink ppStreamSink
            );

        [PreserveSig]
        HResult GetStreamSinkById(
            [In] int dwStreamSinkIdentifier,
            [MarshalAs(UnmanagedType.Interface)] out IMFStreamSink ppStreamSink
            );

        [PreserveSig]
        HResult SetPresentationClock(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationClock pPresentationClock
            );

        [PreserveSig]
        HResult GetPresentationClock(
            [MarshalAs(UnmanagedType.Interface)] out IMFPresentationClock ppPresentationClock
            );

        [PreserveSig]
        HResult Shutdown();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
Guid("2EB1E945-18B8-4139-9B1A-D5D584818530")]
    public interface IMFClock
    {
        [PreserveSig]
        HResult GetClockCharacteristics(
            out MFClockCharacteristicsFlags pdwCharacteristics
            );

        [PreserveSig]
        HResult GetCorrelatedTime(
            [In] int dwReserved,
            out long pllClockTime,
            out long phnsSystemTime
            );

        [PreserveSig]
        HResult GetContinuityKey(
            out int pdwContinuityKey
            );

        [PreserveSig]
        HResult GetState(
            [In] int dwReserved,
            out MFClockState peClockState
            );

        [PreserveSig]
        HResult GetProperties(
            out MFClockProperties pClockProperties
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("868CE85C-8EA9-4F55-AB82-B009A910A805")]
    public interface IMFPresentationClock : IMFClock
    {
        #region IMFClock methods

        [PreserveSig]
        new HResult GetClockCharacteristics(
            out MFClockCharacteristicsFlags pdwCharacteristics
            );

        [PreserveSig]
        new HResult GetCorrelatedTime(
            [In] int dwReserved,
            out long pllClockTime,
            out long phnsSystemTime
            );

        [PreserveSig]
        new HResult GetContinuityKey(
            out int pdwContinuityKey
            );

        [PreserveSig]
        new HResult GetState(
            [In] int dwReserved,
            out MFClockState peClockState
            );

        [PreserveSig]
        new HResult GetProperties(
            out MFClockProperties pClockProperties
            );

        #endregion

        [PreserveSig]
        HResult SetTimeSource(
            [In, MarshalAs(UnmanagedType.Interface)] IMFPresentationTimeSource pTimeSource
            );

        [PreserveSig]
        HResult GetTimeSource(
            [MarshalAs(UnmanagedType.Interface)] out IMFPresentationTimeSource ppTimeSource
            );

        [PreserveSig]
        HResult GetTime(
            out long phnsClockTime
            );

        [PreserveSig]
        HResult AddClockStateSink(
            [In, MarshalAs(UnmanagedType.Interface)] IMFClockStateSink pStateSink
            );

        [PreserveSig]
        HResult RemoveClockStateSink(
            [In, MarshalAs(UnmanagedType.Interface)] IMFClockStateSink pStateSink
            );

        [PreserveSig]
        HResult Start(
            [In] long llClockStartOffset
            );

        [PreserveSig]
        HResult Stop();

        [PreserveSig]
        HResult Pause();
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
        InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
        Guid("F6696E82-74F7-4F3D-A178-8A5E09C3659F")]
    public interface IMFClockStateSink
    {
        [PreserveSig]
        HResult OnClockStart(
            [In] long hnsSystemTime,
            [In] long llClockStartOffset
            );

        [PreserveSig]
        HResult OnClockStop(
            [In] long hnsSystemTime
            );

        [PreserveSig]
        HResult OnClockPause(
            [In] long hnsSystemTime
            );

        [PreserveSig]
        HResult OnClockRestart(
            [In] long hnsSystemTime
            );

        [PreserveSig]
        HResult OnClockSetRate(
            [In] long hnsSystemTime,
            [In] float flRate
            );
    }

    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("7FF12CCE-F76F-41C2-863B-1666C8E5E139"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMFPresentationTimeSource : IMFClock
    {
        #region IMFClock methods

        [PreserveSig]
        new HResult GetClockCharacteristics(
            out MFClockCharacteristicsFlags pdwCharacteristics
            );

        [PreserveSig]
        new HResult GetCorrelatedTime(
            [In] int dwReserved,
            out long pllClockTime,
            out long phnsSystemTime
            );

        [PreserveSig]
        new HResult GetContinuityKey(
            out int pdwContinuityKey
            );

        [PreserveSig]
        new HResult GetState(
            [In] int dwReserved,
            out MFClockState peClockState
            );

        [PreserveSig]
        new HResult GetProperties(
            out MFClockProperties pClockProperties
            );

        #endregion

        [PreserveSig]
        HResult GetUnderlyingClock(
            [MarshalAs(UnmanagedType.Interface)] out IMFClock ppClock
            );
    }


    public enum MFStreamSinkMarkerType
    {
        Default,
        EndOfSegment,
        Tick,
        Event
    }

    [Flags]
    public enum MFMediaSinkCharacteristics
    {
        None = 0,
        FixedStreams = 0x00000001,
        CannotMatchClock = 0x00000002,
        Rateless = 0x00000004,
        ClockRequired = 0x00000008,
        CanPreroll = 0x00000010,
        RequireReferenceMediaType = 0x00000020
    }


    [Flags]
    public enum MFClockCharacteristicsFlags
    {
        None = 0,
        Frequency10Mhz = 0x2,
        AlwaysRunning = 0x4,
        IsSystemClock = 0x8
    }


    public enum MFClockState
    {
        Invalid,
        Running,
        Stopped,
        Paused
    }


    [StructLayout(LayoutKind.Sequential, Pack = 8)]
    public struct MFClockProperties
    {
        public long qwCorrelationRate;
        public Guid guidClockId;
        public MFClockRelationalFlags dwClockFlags;
        public long qwClockFrequency;
        public int dwClockTolerance;
        public int dwClockJitter;
    }

    [Flags]
    public enum MFClockRelationalFlags
    {
        None = 0,
        JitterNeverAhead = 0x1
    }

}
