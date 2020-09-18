﻿
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
//using NLog;

namespace MediaToolkit.Jupiter
{

    public enum ClientState
    {
        Connecting,
        Connected,
        Disconnecting,
        Disconnected,
        Closed,
    }


    public class CPClient
    {
        private static TraceSource logger = TraceManager.GetTrace("MediaToolkit.Jupiter");

        //private static Logger logger = LogManager.GetCurrentClassLogger();

        public CPClient()
        {
            Window = new Window(this);
            WinServer = new WinServer(this);
            Notify = new Notify(this);
            RGBSys = new RGBSys(this);
            ConfigSys = new ConfigSys(this);
        }

        public string Host { get; private set; } = "127.0.0.1";
        public int Port { get; private set; } = 25456;

        public NetworkCredential Credential { get; private set; } = new NetworkCredential("admin", "");

        public ConfigSys ConfigSys { get; } = null;
        public Window Window { get; } = null;
        public WinServer WinServer { get; } = null;
        public Notify Notify { get; } = null;
        public RGBSys RGBSys { get; } = null;

        public event Action<CPNotification> NotificationReceived;
        public event Action StateChanged;

        private volatile ClientState state = ClientState.Disconnected;
        public ClientState State => state;

        private Socket socket = null;

        public bool IsConnected => socket?.Connected ?? false;

        private volatile bool isAuthenticated = false;
        public bool IsAuthenticated => isAuthenticated;


        private Queue<InvokeCommand> commandQueue = new Queue<InvokeCommand>();

        private AutoResetEvent syncEvent = new AutoResetEvent(false);

        private object syncRoot = new object();

        private ushort cSeq = 0;

        private volatile InvokeCommand sendCommand = null;


        private bool StartCommand(InvokeCommand command)
        {

            if (state != ClientState.Connected)
            {
                logger.Warn("StartCommand(...) invalid state: " + state);
                return false;
            }

            lock (syncRoot)
            {
                commandQueue.Enqueue(command);
            }

            syncEvent.Set();

            return true;
        }

        private bool TryGetCommand(out InvokeCommand command)
        {
            bool result = false;
            command = null;

            lock (syncRoot)
            {
                while(commandQueue.Count > 0)
                {
                    command = commandQueue.Dequeue();
                    if (command != null && !command.Aborted)
                    {
                        result = true;
                        break;
                    }
                    logger.Trace("Command aborted, try next...");
                }
            }

            return result;
        }

        private void DrainCommands()
        {
            logger.Debug("DrainCommands()");

            if (sendCommand != null)
            {
                sendCommand.SetResult(null, null);
                sendCommand = null;

                //sendCommand.Dispose();
                //sendCommand = null;
            }

            while (commandQueue.Count > 0)
            {
                TryGetCommand(out var command);

                if (command != null)
                {
                    command.SetResult(null, null);

                    //command.Dispose();
                    //command = null;
                }
            }
        }

        public async Task Connect(string host, int port = 25456)
        {
            logger.Debug("Connect() " + host + " " + port);

            if (state != ClientState.Disconnected)
            {
                throw new InvalidOperationException("Connect(...) invalid state: " + state);
            }

            state = ClientState.Connecting;
            StateChanged?.Invoke();

            await Task.Run(()=>
            {

                this.Host = host;
                this.Port = port;

                ClientProc();

            }).ConfigureAwait(false);

        }

        private void ClientProc()
        {
            logger.Trace("ClientProc BEGIN");

            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.ReceiveTimeout = 10000;
                socket.SendTimeout = 10000;
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

                logger.Trace($"socket.ConnectAsync(...) {Host} {Port}");

                var connectionTask = socket.ConnectAsync(Host, Port);

                Task.WaitAny(new[] { connectionTask }, 10000);

                if (!IsSocketConnected())
                {
                    throw new Exception("Connection error");
                }

                if (state == ClientState.Connecting)
                {
                    state = ClientState.Connected;
                    StateChanged?.Invoke();

                }

				CPDataHandler dataHandler = new CPDataHandler();

				while (true)
                {
                    bool socketConnected = IsSocketConnected();
                    if (!(socketConnected && state == ClientState.Connected))
                    {
                        logger.Debug("Connection break " + socketConnected + " " + state);
                        break;
                    }

                    int available = socket.Available;
                    if (available > 0)
                    {
                        IEnumerable<CPData> dataBuffer = null;
                        //IEnumerable<byte[]> dataLines = null;
                        //CPSocketHandler socketHandler = new CPSocketHandler();

                        Exception exception = null;
                        try
                        {
                            bool bufferProcessed = false;
                            do
                            {
                                byte[] recBuffer = new byte[1024];

                                int bytesRec = socket.Receive(recBuffer);

                                if (bytesRec > 0)
                                { //The ControlPoint protocol is text-based. Each command line terminates
                                  //with CR/LF.Tokens are space separated. All identifiers are case-sensitive.

                                    dataBuffer = dataHandler.ProcessBuffer(recBuffer, bytesRec);

                                    bufferProcessed = (dataBuffer != null && dataBuffer.Count() > 0); //!(socket.Available > 0);

                                    //var useBinnaryHandler = false;
                                    //if (sendCommand != null)
                                    //{
                                    //    var req = sendCommand.Request;
                                    //    useBinnaryHandler = req.BinnaryInResponse;
                                    //}
                                    //dataLines = socketHandler.ReadLines(recBuffer, bytesRec, useBinnaryHandler);
                                    //bufferProcessed = (dataLines != null);

                                }
                                else
                                {// socket closed...

                                    socketConnected = false;
                                    break;
                                }

                            }
                            while (!bufferProcessed);

                        }
                        catch (Exception ex)
                        {// FIXME: переделать обработку ошибок!!!
                            logger.Error(ex);
                            exception = ex;
                        }

                        if (dataBuffer != null)
                        {
                            foreach (var data in dataBuffer)
                            {
                                CPResponseBase cpResponse = null;

                                cpResponse = CPResponseBase.Create(data.TextData, data.BinData);

                                var responseType = cpResponse?.ResponseType ?? CPResponseType.Unknown;

                                if (responseType == CPResponseType.Response || responseType == CPResponseType.Galileo)
                                {
                                    ProcessResponse(cpResponse, exception);
                                }
                                else if (responseType == CPResponseType.Notify)
                                {
                                    ProcessNotification(cpResponse);
                                }
                                else
                                {
                                    ProcessResponse(null, exception);
                                }
                            }

                            dataBuffer = null;
                        }

                    }

                    socketConnected = IsSocketConnected();
                    if (!(socketConnected && state == ClientState.Connected))
                    {
                        logger.Debug("Connection break " + socketConnected + " " + state);
                        break;
                    }

                    { // Response must be received before the next Request is sent!
                        if (sendCommand == null)// обнуляется при получении ответа
                        {
                            if (TryGetCommand(out var command))
                            {
                                try
                                {
                                    var seq = command.Seq;
                                    var request = (CPRequest)command.Request;

                                    byte[] sendBuffer = request.GetBytes();
                                    if (!command.Aborted)
                                    {
                                        int bytesSent = socket.Send(sendBuffer);
                                        sendCommand = command;
                                        // logger.Trace(">> " + sendCommand.ToString() + "bytesSent: " + bytesSent);
                                    }
                                    else
                                    {
                                        logger.Trace($"Command #{seq} aborted");
                                    }
        
                                }
                                catch (Exception ex)
                                {
                                    logger.Error(ex);

                                    ProcessResponse(null, ex);
                                }
                            }
                        }
                    }

                    syncEvent.WaitOne(10);

                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);

                //throw ex;
            }
            finally
            {

                if (socket != null)
                {
                    if (socket.Connected)
                    {
                        socket.Shutdown(SocketShutdown.Both);
                    }

                    socket.Close();
                    socket = null;
                }

                DrainCommands();

                isAuthenticated = false;
                state = ClientState.Disconnected;
                StateChanged?.Invoke();
            }

            logger.Trace("ClientProc END");
        }

        private bool IsSocketConnected()
        {
            return !((socket.Poll(1, SelectMode.SelectRead) && (socket.Available == 0)) || !socket.Connected);
        }

        private void ProcessResponse(CPResponseBase cpResponse, Exception exception)
        {
            if (sendCommand == null)
            {
                throw new InvalidOperationException("sendCommand == null");
            }

            sendCommand.SetResult(cpResponse, exception);
            sendCommand = null;
        }

        private void ProcessNotification(CPResponseBase cpResponse)
        {
            NotificationReceived?.Invoke((CPNotification)cpResponse);
        }

        public async Task<bool> Authenticate(string user, string password, int timeout = 5000)
        {
            /*
             * Authentication is always necessary with an IP connection.
             * Authentication is only necessary under GalileoConnect if you are changing the login from the default values, 
             * as in changing the baud rate or other parameters.
             */
            if (isAuthenticated)
            {
                return true;
            }

            Credential = new NetworkCredential(user, password);

            var authRequest = new CPRequest($"{Credential.UserName}\r\n{Credential.Password}\r\n");

            var resp = await SendAsync(authRequest, timeout);
            resp.ThrowIfError();

            isAuthenticated = resp?.Result ?? false;
            return isAuthenticated;
        }

        public Task<CPResponseBase> SendAsync(CPRequest request, CancellationToken ct, int timeout = 10000)
        {
            return Task.Run(() => SendInternal(request, ct, timeout));
        }

        public Task<CPResponseBase> SendAsync(CPRequest request, int timeout = 10000)
        {
            return SendAsync(request, CancellationToken.None, timeout);
        }

        public CPResponseBase Send(CPRequest request, int timeout = -1)
        {
            return SendInternal(request, CancellationToken.None, timeout);
        }

        private CPResponseBase SendInternal(CPRequest request, CancellationToken ct, int timeout)
        {
            logger.Trace("SendInternal(...) " + timeout);

            if (!IsConnected)
            {
                throw new Exception("Not connected");
            }

            if (state != ClientState.Connected)
            {
                throw new Exception("Invalid state: " + state);
            }

            CPResponseBase response = null;

            InvokeCommand command = null;
            try
            {
                command = new InvokeCommand(request, (cSeq++));

                var result = StartCommand(command);
                if (result)
                {
                    bool cancel = false;
                    ct.Register(() =>
                    {
                        cancel = true;
                    });

                    Func<bool> cancelled = () => (state != ClientState.Connected || cancel);

                    response = command.WaitResult(timeout, cancelled) as CPResponseBase;
                }
                else 
                {
                    //...
                    logger.Warn("StartCommand(...) == " + result);
                }
            }
            finally
            {
                if (command != null)
                {
                    command.Dispose();
                    command = null;
                }

            }

            return response;
        }


        public void Disconnect()
        {
            logger.Debug("Disconnect()");

            if (state != ClientState.Connected)
            {//...

            }

            state = ClientState.Disconnecting;
            StateChanged?.Invoke();

            syncEvent?.Set();
        }


        public void Close()
        {
            logger.Debug("Close()");

            if (socket != null)
            {
                socket.Close();
                socket = null;
            }

            if (syncEvent != null)
            {
                syncEvent.Close();
                syncEvent = null;
            }

            state = ClientState.Closed;

            isAuthenticated = false;

            StateChanged?.Invoke();
        }

        class CPData
        {
            public CPData(byte[] textData, byte[] binData)
            {
                this.TextData = textData;
                this.BinData = binData;
            }

            public readonly byte[] TextData = null;
            public readonly byte[] BinData = null;
        }

        class CPDataHandler
        {
            private byte[] tempBuffer = new byte[32 * 1024];
            private int tempBufferOffset = 0;

            enum StreamState
            {
                WaitStartCode,
                WaitDLE,
                WaitCRLF,

                ReadResultCode,
                ReadBinnaryHeader,
                ReadBinnaryData,
            }

            private StreamState streamState = StreamState.WaitCRLF;

            private int binBufferLength = 0;
            private byte[] binBuffer = null;
            private int binBufferOffset = 0;

            private bool startCodeReceived = false;
            private bool binnaryDataMode = false;
            private bool carriageReturn = false;

            private byte[] textBuffer = null;

            public event Action<CPData> DataReady;
            private List<CPData> tempData = new List<CPData>();

            internal IEnumerable<CPData> ProcessBuffer(byte[] buffer, int bufferLength)
            {
                // =00000000 128 128<DLE>00800000.........<CR><LF>

                int bufferOffset = 0;
                while (true)
                {
                    // размер оставшихся в буфере данных
                    var dataSize = bufferLength - bufferOffset;

                    if (dataSize < 0)
                    {
                        throw new ArgumentException("Invalid data size: " + bufferLength + " " + bufferOffset);
                    }

                    if (dataSize == 0)
                    {
                        break;
                    }

                    if (tempBufferOffset > 0)
                    {// если остались не обработанные данные
                        int newBufLength = tempBufferOffset + dataSize;

                        byte[] newBuf = new byte[newBufLength];
                        Array.Copy(tempBuffer, 0, newBuf, 0, tempBufferOffset);
                        Array.Copy(buffer, bufferOffset, newBuf, tempBufferOffset, dataSize);
                        tempBufferOffset = 0;

                        buffer = newBuf;
                        bufferLength = newBufLength;
                    }



                    int position = 0;

                    if (streamState == StreamState.WaitStartCode ||
                        streamState == StreamState.WaitDLE ||
                        streamState == StreamState.WaitCRLF)
                    {//

                        for (int bufferIndex = bufferOffset; bufferIndex < bufferLength; bufferIndex++)
                        {
                            char ch = (char)buffer[bufferIndex];

                            if (ch == '=' || ch == ':')
                            {
                                if (streamState != StreamState.ReadBinnaryHeader ||
                                    streamState != StreamState.ReadBinnaryData)
                                {// если не в режиме чтения бинарных данных

                                    if (startCodeReceived)
                                    {// стартовый код получен повторно, что то пошло не так...
                                        //throw new InvalidDataException("Start code already received");
                                    }

                                    startCodeReceived = true;
                                }

                            }

                            if (ch == (char)0x10)
                            {// получили DLE, начинаем читать бинарь

                                if (!startCodeReceived)
                                {// не может быть без заголовка
                                    throw new InvalidDataException("Invalid data format. Start code missing");
                                }

                                if (streamState == StreamState.WaitCRLF)
                                {
                                    streamState = StreamState.WaitDLE;
                                }
                                else
                                {
                                    //throw new InvalidDataException("Invalid data format. <DLE> already received");
                                }

                                if (streamState == StreamState.WaitDLE)
                                {
                                    binnaryDataMode = true;
                                    var textSize = (bufferIndex - bufferOffset);
                                    if (textSize <= 0)
                                    {// просто бинарь без заголовка быть не может...

                                        throw new InvalidDataException("Invalid data format. Header size " + textSize);
                                    }

                                    textBuffer = new byte[textSize];
                                    Array.Copy(buffer, bufferOffset, textBuffer, 0, textBuffer.Length);
                                    streamState = StreamState.ReadBinnaryHeader;
                                    position = bufferIndex + 1;
                                    break;
                                }
                            }

                            if (ch == '\r')
                            {
                                carriageReturn = true;

                                continue;
                            }

                            if (ch == '\n')
                            {
                                if (streamState == StreamState.WaitCRLF)
                                {
                                    if (!carriageReturn)
                                    {
                                        //throw new InvalidDataException("Invalid data format. <CR> missing");
                                        //....
                                        Console.WriteLine("Invalid data format. <CR> missing");
                                    }

                                    if (!binnaryDataMode)
                                    {// только сообщение, без бинаря...
                                        var textSize = (bufferIndex - 1) - bufferOffset;
                                        if (textSize > 0)
                                        {
                                            textBuffer = new byte[textSize];
                                            Array.Copy(buffer, bufferOffset, textBuffer, 0, textBuffer.Length);
                                        }
                                        else
                                        {
                                            throw new InvalidDataException("Text data corrupt or not valid");
                                        }

                                    }

                                    byte[] crlf = { (byte)'\r', (byte)'\n' };
                                    var newBufSize = textBuffer.Length + crlf.Length;
                                    var newBuf = new byte[newBufSize];
                                    Array.Copy(textBuffer, newBuf, textBuffer.Length);
                                    Array.Copy(crlf, 0, newBuf, textBuffer.Length, crlf.Length);

                                    textBuffer = newBuf;

                                }

                                position = bufferIndex + 1;
                                bufferOffset = position;

                                var data = new CPData(textBuffer, binBuffer);

                                Reset();

                                DataReady?.Invoke(data);
                                tempData.Add(data);
                                //Console.WriteLine("======================================");

                                continue;
                            }

                            carriageReturn = false;
                        }
                    }

                    if (streamState == StreamState.ReadBinnaryHeader)
                    {// первые 4 байта после <DLE> размер bin-а

                        int bufferRemaining = bufferLength - position;
                        if (bufferRemaining > 4)
                        {
                            binBufferLength = BitConverter.ToInt32(buffer, position);

                            if (binBufferLength <= 0)
                            {
                                throw new InvalidDataException("Invalid binData size " + binBufferLength);
                            }

                            //Console.WriteLine("DataLength: " + DataLength + " pos: " + pos);
                            position += 4;

                            binBuffer = new byte[binBufferLength];
                            binBufferOffset = 0;

                            streamState = StreamState.ReadBinnaryData;
                        }
                        else
                        {// сохраняем в буффер то что есть и выходим
                            bufferOffset = position;

                            //Array.Copy(buffer, pos, tempBuffer, tempBufferOffset, bufferRemaining);
                            //tempBufferOffset += bufferRemaining;
                            //break;
                        }
                    }


                    if (streamState == StreamState.ReadBinnaryData)
                    {// собираем бинарные данные...

                        int bufferRemaining = bufferLength - position;
                        int dataRemaining = binBufferLength - binBufferOffset;
                        Debug.Assert(dataRemaining > 0, "dataRemaining>0");

                        if (bufferRemaining >= dataRemaining)
                        { // все копируем
                            Array.Copy(buffer, position, binBuffer, binBufferOffset, dataRemaining);

                            binBufferOffset += dataRemaining;
                            position += dataRemaining;

                            // ждем конец сообщения
                            streamState = StreamState.WaitCRLF;

                            //Console.WriteLine("WaitCRLF " + pos);
                            bufferOffset = position;
                            continue;

                        }
                        else
                        {// не удалось прочитать все бинарные данные, сохраняем то что есть в буфер и выходим 

                            Array.Copy(buffer, position, binBuffer, binBufferOffset, bufferRemaining);
                            binBufferOffset += bufferRemaining;

                            var _resultData = new List<CPData>(tempData);
                            tempData.Clear();
                            return _resultData;
                            //return;
                        }
                    }

                    int _dataRemaining = bufferLength - bufferOffset;
                    if (_dataRemaining > 0)
                    {  // данных не достаточно, копируем в буфер					
                        Array.Copy(buffer, bufferOffset, tempBuffer, tempBufferOffset, _dataRemaining);
                        tempBufferOffset += _dataRemaining;
                    }

                    break;
                }

                var resultData = new List<CPData>(tempData);
                tempData.Clear();

                return resultData;

            }

            private void Reset()
            {
                textBuffer = null;
                binBuffer = null;
                binBufferLength = 0;
                binBufferOffset = 0;
                binnaryDataMode = false;
                carriageReturn = false;
                startCodeReceived = false;
                //...
            }
        }

        /*
		class CPData
		{
			public byte[] Message = null;
			public byte[] Bin = null;
		}

		class CPDataHandler
		{
			enum StreamState
			{
				WaitStartCode,
				WaitDLE,
				WaitCRLF,

				ReadResultCode,
				ReadBinnaryHeader,
				ReadBinnaryData,
			}


			private byte[] tempBuffer = new byte[16 * 1024];
			private int tempBufferOffset = 0;

			private StreamState streamState = StreamState.WaitCRLF;

			private int DataLength = 0;
			private byte[] DataBuffer = null;
			private int DataOffset = 0;
			private bool dleMode = false;
			private byte[] MessageBuffer = null;

			public event Action<CPData> DataReady;
			internal void ProcessBuffer(byte[] buf, int offset, int length)
			{
				// =00000000 128 128<DLE>00800000.........<CR><LF>

				while (true)
				{
					if (offset >= length)
					{
						//Console.WriteLine("!!!!!!!!!!!!!!!!!!! " + offset + " >= " + length);

						break;
						//return;
					}

					if (tempBufferOffset > 0)
					{// если остались не обработанные данные
						int len = length - offset;
						int newSize = tempBufferOffset + len;

						byte[] newBuf = new byte[newSize];
						Array.Copy(tempBuffer, 0, newBuf, 0, tempBufferOffset);
						Array.Copy(buf, offset, newBuf, tempBufferOffset, len);
						tempBufferOffset = 0;

						buf = newBuf;
						length = newSize;
					}



					int pos = 0;

					if (streamState == StreamState.WaitStartCode ||
						streamState == StreamState.WaitDLE ||
						streamState == StreamState.WaitCRLF)
					{//

						for (int i = offset; i < length; i++)
						{
							char ch = (char)buf[i];

							//if (streamState == StreamState.WaitStartCode ||
							//	streamState == StreamState.WaitDLE || 
							//	streamState == StreamState.WaitCRLF)
							{


								if (ch == (char)0x10)
								{
									if (streamState == StreamState.WaitCRLF)
									{
										streamState = StreamState.WaitDLE;
									}

									if (streamState == StreamState.WaitDLE)
									{
										dleMode = true;

										var messageSize = i - offset;
										MessageBuffer = new byte[messageSize];
										Array.Copy(buf, offset, MessageBuffer, 0, MessageBuffer.Length);

										var message = Encoding.UTF8.GetString(MessageBuffer);
										//Console.WriteLine("MessageBody: " + message);

										//Console.WriteLine("<DLE> at pos: " + i);
										streamState = StreamState.ReadBinnaryHeader;
										pos = i + 1;
										break;
									}
								}


								if (ch == '\r')
								{
									continue;
								}

								if (ch == '\n')
								{
									if (streamState == StreamState.WaitCRLF)
									{
										if (!dleMode)
										{
											var messageSize = (i - 1) - offset;

											MessageBuffer = new byte[messageSize];
											Array.Copy(buf, offset, MessageBuffer, 0, MessageBuffer.Length);

											//var message = Encoding.UTF8.GetString(MessageBuffer);
											//Console.WriteLine("MessageBody: " + message);
										}


										byte[] crlf = { (byte)'\r', (byte)'\n' };
										var newBufSize = MessageBuffer.Length + crlf.Length;
										var newBuf = new byte[newBufSize];
										Array.Copy(MessageBuffer, newBuf, MessageBuffer.Length);
										Array.Copy(crlf, 0, newBuf, MessageBuffer.Length, crlf.Length);

										MessageBuffer = newBuf;

										var message = Encoding.UTF8.GetString(MessageBuffer);
										//Console.WriteLine("MessageBody: " + message);

									}

									//Console.WriteLine("Data: " + DataLength + " " + DataOffset);
									//Console.WriteLine("<CR><LF> at pos: " + i);
									pos = i + 1;

									offset = pos;

									var data = new CPData
									{
										Message = MessageBuffer,
										Bin = DataBuffer,
									};

									MessageBuffer = null;
									DataBuffer = null;
									dleMode = false;
									DataLength = 0;
									DataOffset = 0;

									DataReady?.Invoke(data);

									//...
									Console.WriteLine("======================================");


									continue;
								}
							}

						}
					}

					if (streamState == StreamState.ReadBinnaryHeader)
					{// первые 4 байта после <DLE> размер ранных

						//Console.WriteLine("ReadBinnaryHeader " + pos);

						int len = length - pos;
						if (len > 4)
						{
							DataLength = BitConverter.ToInt32(buf, pos);
							//Console.WriteLine("DataLength: " + DataLength + " pos: " + pos);
							pos += 4;

							DataBuffer = new byte[DataLength];
							DataOffset = 0;

							streamState = StreamState.ReadBinnaryData;
						}
						else
						{// сохраняем в буффер то что есть и выходим

							Array.Copy(buf, pos, tempBuffer, tempBufferOffset, len);
							tempBufferOffset += len;
							break;
						}
					}


					if (streamState == StreamState.ReadBinnaryData)
					{
						//Console.WriteLine("ReadBinnaryData " + pos);

						int len = length - pos;
						int dataRemaining = DataLength - DataOffset;
						if (len >= dataRemaining)
						{ // сразу все копируем
							Array.Copy(buf, pos, DataBuffer, DataOffset, dataRemaining);

							DataOffset += dataRemaining;
							pos += dataRemaining;

							//Debug.Assert(DataLength == DataOffset, "DataLength == DataOffset");

							// ждем конец сообщения
							streamState = StreamState.WaitCRLF;

							//Console.WriteLine("WaitCRLF " + pos);
							offset = pos;
							continue;

						}
						else
						{
							Array.Copy(buf, pos, DataBuffer, DataOffset, len);
							DataOffset += len;

							return;
						}
					}

					int _dataRemaining = length - offset;
					if (_dataRemaining > 0)
					{  // данных не достаточно, копируем в буфер					
						Array.Copy(buf, offset, tempBuffer, tempBufferOffset, _dataRemaining);
						tempBufferOffset += _dataRemaining;
					}

					break;
				}


			}


		}
        */


        class CPSocketHandler
        {
            private byte[] tempBuffer = null;
            private bool carriageReturn = false;

            private List<byte[]> dataLines = new List<byte[]>();

            public IEnumerable<byte[]> ReadLines(byte[] buffer, int size, bool binnaryDataMode = false)
            {
                List<byte[]> resultBuffer = null;
                int dataRemaining = size;
                int startIndex = 0;

                for (int i = 0; i < size; i++)
                {
                    var ch = (char)buffer[i];

                    if (ch == '\r')
                    {
                        carriageReturn = true;
                        continue;
                    }
                    else if (ch == '\n' && carriageReturn)
                    {
                        var endIndex = i;
                        var dataSize = (endIndex - startIndex) + 1;

                        int offset = 0;
                        byte[] dataBuffer = null;

                        if (tempBuffer == null)
                        {
                            dataBuffer = new byte[dataSize];
                        }
                        else
                        {
                            var totalSize = dataSize + tempBuffer.Length;
                            dataBuffer = new byte[totalSize];

                            Array.Copy(tempBuffer, 0, dataBuffer, offset, tempBuffer.Length);
                            offset += tempBuffer.Length;

                            tempBuffer = null;
                        }

                        Array.Copy(buffer, startIndex, dataBuffer, offset, dataSize);

                        startIndex = endIndex + 1;// next char...

                        //оставшиеся в буфере данные
                        dataRemaining = (size - startIndex);

                        dataLines.Add(dataBuffer);

                        if (binnaryDataMode)
                        {
                            File.WriteAllBytes("test_16x16.raw", dataBuffer);
                        }

                        if (dataRemaining == 0)
                        {// весь буфер прочитан

                            resultBuffer = new List<byte[]>(dataLines);
                            dataLines.Clear();

                            break;
                        }
                        else
                        {// остались данные нужно дочитать...

                        }
                        carriageReturn = false;
                    }
                    else
                    {
                        if (carriageReturn)
                        {
                            carriageReturn = false;
                        }
                    }
                }

                Debug.Assert(dataRemaining >= 0, "dataRemaining >= 0");

                if (dataRemaining > 0)
                {// остались не обработаные данные сохраняем их в буффер...

                    // logger.Trace("dataRemaining == " + dataRemaining);

                    int offset = 0;
                    if (tempBuffer != null)
                    {
                        var tempSize = tempBuffer.Length + dataRemaining;
                        var newTempBuffer = new byte[tempSize];

                        Array.Copy(tempBuffer, 0, newTempBuffer, offset, tempBuffer.Length);
                        offset += tempBuffer.Length;

                        tempBuffer = newTempBuffer;
                    }
                    else
                    {
                        tempBuffer = new byte[dataRemaining];
                    }

                    Array.Copy(buffer, startIndex, tempBuffer, offset, dataRemaining);
                }

                //if(dataLines.Count> 0)
                //{
                //    resultBuffer = new List<byte[]>(dataLines);
                //    dataLines.Clear();

                //}

                return resultBuffer;
            }

		}


        class InvokeCommand
        {
            public InvokeCommand(CPRequest request, ushort seq)
            {
                this.Request = request;
                this.Seq = seq;
            }

            public readonly ushort Seq = 0;
            public readonly CPRequest Request = null;


            private CPResponseBase response = null;
            private Exception exception = null;

            private ManualResetEvent waitEvent = new ManualResetEvent(false);

            private volatile bool aborted = false;
            public bool Aborted => aborted;

            public object WaitResult(int timeout = -1, Func<bool> canceled = null)
            {
                if (disposed)
                {
                    throw new ObjectDisposedException("InvokeCommand");
                }

                Exception ex = null;

                var startTicks = Stopwatch.GetTimestamp();
                Func<long> ElapsedMilliseconds = new Func<long>(() =>
                {
                    return (long)(((Stopwatch.GetTimestamp() - startTicks) / (double)Stopwatch.Frequency) * 1000);
                });

                long msec = 0;
                //Stopwatch sw = Stopwatch.StartNew();
                logger.Trace($"Request #{Seq} started");
                bool result = false;
                do
                {
                    result = waitEvent.WaitOne(300);
                    if (result)
                    {
                        logger.Trace($"Request #{Seq} completed " + ElapsedMilliseconds());
                        break;
                    }

                    if (canceled?.Invoke() ?? false)
                    {
                        ex = new OperationCanceledException();
                        aborted = true;
                        logger.Trace($"Request #{Seq} canceled");
                        break;
                    }

                    if (timeout > 0)
                    {
                        msec = ElapsedMilliseconds();
                        if (msec > timeout)
                        {
                            ex = new TimeoutException();
                            aborted = true;
                            logger.Trace($"Request #{Seq} timed out: {msec} {timeout}");
                            break;
                        }
                    }

                    logger.Trace($"#{Seq} waiting for result...");

                } while (!result);

                if (exception == null)
                {
                    exception = ex;
                }

                if (exception != null)
                {
                    throw exception;
                }

                if (response == null)
                {
                    var message = $"Request #{Seq} dropped";
                    if (Request != null)
                    {
                        message = message + " \"" + Request.ToString() + "\"";
                    }

                    throw new CPException(message);
                }

                return response;

            }

            public void SetResult(CPResponseBase resp, Exception ex)
            {
                if (disposed )
                {
                    logger.Trace("SetResult(...) " + disposed + " " + Aborted);
                    return;
                }

                this.response = resp;
                this.exception = ex;


                logger.Trace($"<< #{Seq} {resp}");

                waitEvent?.Set();
            }

            public override string ToString()
            {
                return $"#{Seq} {Request}";
            }

            private bool disposed = false;
            public void Dispose()
            {
                if (waitEvent != null)
                {
                    waitEvent.Dispose();
                    waitEvent = null;
                }

                disposed = true;

            }

		}

	}



    public class CPRequest
    {
        public CPRequest(string req)
        {
            this.RequestString = req;
        }

        public CPRequest(string objName, string method, params object[] values)
        {
            this.ObjectName = objName;
            this.Method = method;
            this.ValueList = string.Join(" ", values); // "{ " + string.Join(" ", values) + " }";

            this.RequestString = $"+{ObjectName} {Method} {ValueList}\r\n";
        }


        public CPRequest(string objName, string method, string valueList = "")
        {
            this.ObjectName = objName;
            this.Method = method;
            this.ValueList = valueList;

            this.RequestString = $"+{ObjectName} {Method} {ValueList}\r\n";
        }

        public readonly string ObjectName = "";
        public readonly string Method = "";
        public readonly string ValueList = "";

        public string RequestString { get; private set; }

        public bool BinnaryInResponse { get; set; }

        public byte[] GetBytes()
        {

            if (string.IsNullOrEmpty(RequestString))
            {
                throw new InvalidOperationException("Empty request");
            }

            return Encoding.UTF8.GetBytes(RequestString);
        }

        public override string ToString()
        {
            return RequestString;
        }
    }

    public enum CPResponseType
    {
        Unknown,
        Response,
        Notify,
        Galileo,
    }

    public abstract class CPResponseBase
    {
        protected CPResponseBase(string resp)
        {
            this.ResponseString = resp;
        }

        public readonly string ResponseString = "";
        public virtual bool Result { get; } = true;

        public abstract CPResponseType ResponseType { get; }
        public abstract void ThrowIfError();

        public byte[] RawData { get; set; }

        public static CPResponseBase Create(byte[] bytes)
        {
            const byte DataLinkEscape = 0x10;
            const byte CarriageReturn = (byte)'\r';
            const byte LineFeed = (byte)'\n';

            var endIndex = bytes.Length - 2;
            if (bytes[endIndex] != CarriageReturn || bytes[endIndex + 1] != LineFeed)
            {
                //invalid format...
                throw new FormatException("Invalid response. <CR><LF> missing");
            }

            for (int i = 0; i < bytes.Length; i++)
            {// FIXME: нужно только для метода GetPreview() который не документирован!
                // поэтому лучше проверять только после вызова GetPreview...
                if (bytes[i] == DataLinkEscape)
                {// после DLE начинаются bin данные
                 //=00000000 128 96DLE...rgb555...\r\n

                    int offset = 0;
                    var startIndex = i;
                    byte[] headerBytes = new byte[startIndex];
                    Array.Copy(bytes, offset, headerBytes, 0, headerBytes.Length);
                    offset += headerBytes.Length + 1;

                    var dataSize = endIndex - startIndex - 1;

                    var rawData = new byte[dataSize];

                    Array.Copy(bytes, offset, rawData, 0, rawData.Length);
					var crlf = new byte[] {(byte)'\r', (byte)'\n' };

					headerBytes = headerBytes.Concat(crlf).ToArray();

					//var respStr = Encoding.ASCII.GetString(headerBytes, 0, headerBytes.Length) + "\r\n";

                    return Create(headerBytes, rawData);

                }
            }

            return Create(bytes);
        }


        public static CPResponseBase Create(byte[] respData, byte[] binData = null)
        {
			var respStr = Encoding.UTF8.GetString(respData, 0, respData.Length);

			if (string.IsNullOrEmpty(respStr))
            {
                throw new ArgumentException("Empty response");
            }

            if (!respStr.EndsWith("\r\n"))
            {
                throw new FormatException("Invalid response <CR><LF> missing");
            }


            CPResponseBase response = null;

            if (respStr.StartsWith("="))
            {// ответ на запрос от сервера
                response = new CPResponse(respStr);
            }
            else if (respStr.StartsWith(":"))
            {// события сервера
                response = new CPNotification(respStr);
            }
            else
            {
                if (respStr.StartsWith("OK") || respStr.StartsWith("ER"))
                {// ответ от Galileo Connect
                    response = new GCResponse(respStr);

                }
                else
                {
                    throw new FormatException("Invalid  GalileoConnect response");
                }
            }

            response.RawData = binData;

            return response;

        }


        public override string ToString()
        {
            return ResponseString;
        }
    }


    public class CPResponse : CPResponseBase
    {
        private const int ResultCodeLength = 8;
        private const int MinResponseLength = ResultCodeLength + 3; //"=00000000\r\n";


        public CPResponse(string resp) : base(resp)
        {
            if (!resp.StartsWith("=") )
            {
                throw new FormatException("Invalid response format. Start code missing" + resp);
            }

            if (!resp.EndsWith("\r\n"))
            {
                throw new FormatException("Invalid response format <CR><LF> missing" + resp);
            }

            if ( resp.Length < MinResponseLength)
            {
                throw new FormatException("Invalid response format. Response length < " + MinResponseLength);
            }

            //"="
            int offset = 1;

            var codeStr = resp.Substring(offset, ResultCodeLength);
            offset += ResultCodeLength;

            ResultCode = int.Parse(codeStr, System.Globalization.NumberStyles.HexNumber);

            var valueList = "";
            if (resp.Length > offset)
            {
                valueList = resp.Substring(offset);

                valueList = valueList.TrimEnd('\r', '\n');
                valueList = valueList.TrimStart(' ');
                valueList = valueList.TrimEnd(' ');
            }

            ValueList = valueList;
        }

        public override CPResponseType ResponseType => CPResponseType.Response;

        public int ResultCode { get; private set; } = 0;

        public string ValueList { get; private set; } = "";

        public override bool Result => (ResultCode == (uint)ResultCodes.S_OK); // || ResultCode == (uint)ResultCodes.S_FALSE);//(ResultCode == 0);

        public bool Success => (ResultCode == (uint)ResultCodes.S_OK || ResultCode == (uint)ResultCodes.S_FALSE);

        public override void ThrowIfError()
        {
            if (!Success)
            {
                throw new CPException((ResultCodes)ResultCode);
            }
        }

    }


    public class CPNotification : CPResponseBase
    {
        //":Notify WindowsState { 1 { { 10002 } 2 1 5120 254 244 684 507 { -1 } } }\r\n"
        public CPNotification(string resp) : base(resp)
        {
            if (!resp.StartsWith(":"))
            {
                throw new FormatException("Invalid notification format. Start code missing");
            }

            if (!resp.EndsWith("\r\n"))
            {
                throw new FormatException("Invalid notification format. <CR><LF> missing");
            }

            resp = resp.TrimStart(':', ' ');
            resp = resp.TrimEnd('\r', '\n');

            // Parse object, method...
            int offset = 0;
            var index = resp.IndexOf(" ", offset);
            if (index > offset)
            {
                ObjectName = resp.Substring(offset, index);
                offset = index + 1;

                index = resp.IndexOf(" ", offset);
                if (index > offset)
                {
                    var len = (index - offset);
                    Method = resp.Substring(offset, len);
                    offset = index + 1;

                    ValueList = resp.Substring(offset);
                }
            }
        }

        public readonly string ObjectName = "";
        public readonly string Method = "";
        public readonly string ValueList = "";

        public override CPResponseType ResponseType => CPResponseType.Notify;

        public override void ThrowIfError()
        {
           // throw new NotImplementedException();
        }

        public override string ToString()
        {
            return ":" + ObjectName + " " + Method + " " + string.Join(" ", ValueList) + "\r\n";
        }
    }

    // нужно только для авторизации...
    public class GCResponse : CPResponseBase
    {
        public GCResponse(string resp) : base(resp)
        {//ControlPoint Protocol Manual p.32
            if (resp.StartsWith("OK"))
            {//GalileoConnect positive responses start with OK, followed by optional text data with the result of the command.
                //OK localhost 25456 localuser <CR><LF>
                success = true;

                Data = resp.Substring(2);
                Data = Data.TrimStart(' ');
                Data = Data.TrimEnd('\r', '\n');
            }
            else if (resp.StartsWith("ER"))
            {// error...
                //GalileoConnect negative responses start with ER, followed by optional description of the error condition.
                //ER Socket error <CR><LF>
                success = false;
                Data = resp.Substring(2);
                Data = Data.TrimStart(' ');
                Data = Data.TrimEnd('\r', '\n');
            }
            else
            {
                throw new ArgumentException(resp);
            }
        }

        public string Data { get; private set; } = "";
        private bool success = false;

        public override bool Result => success;
        public override CPResponseType ResponseType => CPResponseType.Galileo;

        public override void ThrowIfError()
        {
            if (!Result)
            {
                throw new Exception(Data);
            }
        }

    }
}
