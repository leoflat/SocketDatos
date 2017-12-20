//using System;

//namespace SocketDatos
//{
//    class Program
//    {
//        static void Main(string[] args)
//        {
//            Console.WriteLine("Hello World!");
//        }
//    }
//}



using System; // using System.N"et;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
//
using System.Dynamic;
using Newtonsoft.Json;
using System.IO;//guardar archivo

namespace SocketDatos
{
    class Servidor
    {
        // Thread signal.
        static int NumTransmision = 0;
        public static ManualResetEvent allDone = new ManualResetEvent(false);


        static void Main(string[] args)//public void conectar() ////cambio realizado para prueba
        {


            
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);// (SERVICO, TIPODATO, TIPO TCP)
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 1234);
            //IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Parse("172.31.17.41"), 8004); //(IP , PUERTO)   //IPADRESS.PARSE convierte una cadena de texto en una ip valida
            //127.0.0.1 es una direccion local de la computadora. 
            //miPrimerSocket.Bind(direccion);//bind establecemos direccion dl socket
            

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    // Set the event to nonsignaled state.
                    allDone.Reset();
                    
                    Console.WriteLine("\n\n\t\t\t\t TRANSMISON  # {0} \n", NumTransmision);
                    NumTransmision = NumTransmision + 1;
                    // Start an asynchronous socket to listen for connections.
                    Console.WriteLine("Waiting for a connection...");
                    
                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.WriteLine("\nPress ENTER to continue...");
            Console.Read();


            //miPrimerSocket.Listen(5);//listen cantidad de direcciones con ese socket
            //Console.WriteLine("Escuchando...");
            //Socket Escuchar = miPrimerSocket.Accept();//se crea un nuevo socket, este metodo accept devuelve un socket listo para trabajar en tu programa aplicacion y el cliente;
            //Console.WriteLine("Escuchado con exito");

            //byte[] ByRec = new byte[255];
            //while (true)
            //{
            //    int a = Escuchar.Receive(ByRec, 0, ByRec.Length, 0); //objeto socket metodo recibe array, desde donde, tama;o que tiene, con signo o sin signo)
            //    Array.Resize(ref ByRec, a); //le dice el nuevo tama;o del array con la longitud de a
            //    Console.WriteLine("CLIENTE DICE: " + Encoding.Default.GetString(ByRec)); //mostramos lo recibido  metodos estaticos de clase encoding que convierte el array en caracteres
            //    //hilo

            //}
           //listener.Close();
            //Console.WriteLine("presione cualquier tecla para salir");
            //Console.ReadKey();

        }

        public static void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public static void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                 //conteo veces de transmision
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                //string[] Lines = content.Split(new char[] { '\r' }, StringSplitOptions.RemoveEmptyEntries);
                //foreach (string Str in Lines)
                //{
                //    string[] Fields = Str.Split(new char[] { ';' }, StringSplitOptions.None);//llena el vector fields con todo el string que este separado por comas
                //                                                                             //Console.WriteLine("POS : " + Fields[n]);
                //    for (int i = 0; i < Fields.Length; i++)
                //    {
                //        Console.WriteLine("POS {0} : \t {1}", i , Fields[i]);
                //    }
                //                                //Console.WriteLine("Primera Prueba");
                //                                //Console.ReadKey();
                //    if (Fields != null)
                //    {
                //        //Console.WriteLine("esta vacia");
                //        ProcessRxData(Fields);
                //    }

                //                                //Console.WriteLine("segunda impresion");
                //                                //Console.ReadKey();
                //                                //for (int i = 0; i < Fields.Length; i++)
                //                                //{
                //                                //    Console.WriteLine("POS : " + Fields[i]);
                //                                //}
                //                                //Console.ReadKey();

                //}

                if (content.IndexOf("ST300") > -1)   //if (content.IndexOf("<EOF>") > -1) { //EOF INDICA FINAL DEL MENSAJE
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                    Console.WriteLine("\n IMPRESION DE LA TRAMA DE ENTRADA \n");
                    Console.WriteLine("\n Read {0} bytes from socket. \n Data : {1}", content.Length, content);
                    // Echo the data back to the client.
                    //Send(handler, "");
                }
                else
                {
                    // Not all data received. Get more.
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
                    //////////////////////this.Invoke(new TCPSocketServer.SocketClosedHandler(ProcessClientClose), Params);//PRUEBA
                }
            }
        }

        private static void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.


            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine($"Sent {bytesSent} bytes to client.");

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }






        /////////////////////////////////////////
        static void ProcessRxData(string[] Fields)//se cambio de-> private void ProcessRxData(string[] Fields, SocketMessageReceivedArgs Arg)
        {
            UInt16 Index = 0;
            if (Fields != null)
            {
                while (Index < Fields.Length)
                {
                    if (Fields[Index].Length >= 5)
                    {
                        string Series = Fields[Index].Substring(0, 5);
                        string Type = Fields[Index++].Substring(5);
                        if ((Series != null) && (Type != null))
                        {
                            if (Series.Length == 5)
                            {
                                switch (Series)
                                {
                                    //case "SA200":
                                    //    Index = ProcessST200(Type, Index, Fields, Arg);
                                    //    break;
                                    case "ST300":
                                        Index = ProcessST300(Type, Index, Fields);
                                        break;
                                    //case "ST940":
                                    //    Index = ProcessST940(Index, Fields, Arg);
                                    //    break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    else
                    {
                        Index++;
                    }
                }
            }

        }


        static UInt16 ProcessST300(string Type, UInt16 Index, string[] Fields)
        {
            string Id = Fields[Index++];
            if (Id == "Res")
            {
                Index = ProcessCommandResponseST300(Type, Index, Fields);
            }
            else
            {
                //string Ip = Arg.Ip.ToString();
                string Version = string.Empty;
                string Date = string.Empty;
                string Time = string.Empty;
                string Cell = string.Empty;
                string Lat = string.Empty;
                string Lon = string.Empty;
                string Speed = string.Empty;
                string Course = string.Empty;
                string Sat = string.Empty;
                string Fix = string.Empty;
                string Odo = string.Empty;
                string Vol = string.Empty;
                string Io = string.Empty;
                string Data = string.Empty;
                string Add = string.Empty;
                string Model = string.Empty;
                string DriveHour = string.Empty;
                string BackupVol = string.Empty;
                string MsgType = string.Empty;
                if (Type != "ALV")
                {
                    if (Fields.Length > Index + 13)
                    {
                        Model = Fields[Index++];
                        Version = Fields[Index++];
                        Date = Fields[Index++];
                        Time = Fields[Index++];
                        Cell = Fields[Index++];
                        Lat = Fields[Index++];
                        Lon = Fields[Index++];
                        Speed = Fields[Index++];
                        Course = Fields[Index++];
                        Sat = Fields[Index++];
                        Fix = Fields[Index++];
                        Odo = Fields[Index++];
                        Vol = Fields[Index++];
                        Io = Fields[Index++];
                        Data = string.Empty;
                        Add = string.Empty;

                        switch (Model)
                        {
                            case "01":
                                Model = "ST300RI";
                                break;
                            case "02":
                                Model = "ST340";
                                break;
                            case "03":
                                Model = "ST340LC";
                                break;
                            case "04":
                                Model = "ST300H";
                                break;
                            default:
                                break;
                        }

                        switch (Type)
                        {
                            case "STT":
                                if (Fields.Length <= Index + 1)// + 4)
                                {
                                    return Index;
                                }
                                Type = "STATUS";
                                Add = Fields[Index++];
                                Data = "Msg #: " + Fields[Index++];
                                if (Fields.Length > Index + 2)
                                {
                                    DriveHour = Fields[Index++];
                                    BackupVol = Fields[Index++];
                                    MsgType = Fields[Index++];
                                }
                                switch (MsgType)
                                {
                                    case "0":
                                        MsgType = "Real Time";
                                        break;
                                    case "1":
                                        MsgType = "Storage";
                                        break;
                                    default:
                                        MsgType = string.Empty;
                                        break;
                                }
                                switch (Add)
                                {
                                    case "1":
                                        Add = "Idle Mode";
                                        break;
                                    case "2":
                                        Add = "Active Mode";
                                        break;
                                    case "3":
                                        Add = "Distance";
                                        break;
                                    case "4":
                                        Add = "Angle";
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case "ALT":
                                if (Fields.Length <= Index)// + 3)
                                {
                                    return Index;
                                }
                                Type = "ALERT";
                                Data = Fields[Index++];
                                if (Fields.Length > Index + 2)
                                {
                                    DriveHour = Fields[Index++];
                                    BackupVol = Fields[Index++];
                                    MsgType = Fields[Index++];
                                }
                                switch (MsgType)
                                {
                                    case "0":
                                        MsgType = "Real Time";
                                        break;
                                    case "1":
                                        MsgType = "Storage";
                                        break;
                                    default:
                                        MsgType = string.Empty;
                                        break;
                                }
                                switch (Data)
                                {
                                    case "1":
                                        Add = "Over SPEED_LIMIT";
                                        break;
                                    case "2":
                                        Add = "End Over SPEED";
                                        break;
                                    case "3":
                                        Add = "GPS Antenna Disconnect";
                                        break;
                                    case "4":
                                        Add = "GPS Antenna Reconnect";
                                        break;
                                    case "5":
                                        Add = "Geo-Fence Out";
                                        break;
                                    case "6":
                                        Add = "Geo-Fence In";
                                        break;
                                    case "8":
                                        Add = "GPS Antenna Shorted";
                                        break;
                                    case "9":
                                        Add = "Enter Deep Sleep";
                                        break;
                                    case "10":
                                        Add = "Exit Deep Sleep";
                                        break;
                                    case "13":
                                        Add = "Backup Battery Error";
                                        break;
                                    case "14":
                                        Add = "Vehicle Battery Low";
                                        break;
                                    case "15":
                                        Add = "Shocked";
                                        break;
                                    case "16":
                                        Add = "Collision";
                                        break;
                                    case "18":
                                        Add = "Route Deviation";
                                        break;
                                    case "19":
                                        Add = "Enter Into Route";
                                        break;
                                    case "33":
                                        Add = "Ignition ON";
                                        break;
                                    case "34":
                                        Add = "Ignition OFF";
                                        break;
                                    case "40":
                                        Add = "Main Power Connect";
                                        break;
                                    case "41":
                                        Add = "Main Power Disconnect";
                                        break;
                                    case "44":
                                        Add = "Backup Battery Connect";
                                        break;
                                    case "45":
                                        Add = "Backup Battery Disconnect";
                                        break;
                                    case "46":
                                        Add = "Fast Acceleration DPA";
                                        break;
                                    case "47":
                                        Add = "Fast Acceleration DPA";
                                        break;
                                    case "48":
                                        Add = "Sharp Turn DPA";
                                        break;
                                    case "49":
                                        Add = "Over Speed DPA";
                                        break;
                                    case "50":
                                        Add = "Jamming Detected";
                                        break;
                                    case "68":
                                        Add = "Stop Limit at Ign. ON";
                                        break;
                                    case "69":
                                        Add = "Moving After Stop Limit";
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case "EMG":
                                if (Fields.Length <= Index)// + 3)
                                {
                                    return Index;
                                }
                                Type = "EMERGENCY";
                                Data = Fields[Index++];
                                if (Fields.Length > Index + 2)
                                {
                                    DriveHour = Fields[Index++];
                                    BackupVol = Fields[Index++];
                                    MsgType = Fields[Index++];
                                }
                                switch (MsgType)
                                {
                                    case "0":
                                        MsgType = "Real Time";
                                        break;
                                    case "1":
                                        MsgType = "Storage";
                                        break;
                                    default:
                                        MsgType = string.Empty;
                                        break;
                                }
                                switch (Data)
                                {
                                    case "1":
                                        Add = "Panic Button";
                                        break;
                                    case "2":
                                        Add = "Parking Lock";
                                        break;
                                    case "3":
                                        Add = "Main Power Lost";
                                        break;
                                    case "5":
                                        Add = "Anti-Theft";
                                        break;
                                    case "6":
                                        Add = "Anti-Theft Door";
                                        break;
                                    case "7":
                                        Add = "Motion";
                                        break;
                                    case "8":
                                        Add = "Anti-Theft Shock";
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case "EVT":
                                if (Fields.Length <= Index)// + 3)
                                {
                                    return Index;
                                }
                                Type = "EVENT";
                                Data = Fields[Index++];
                                if (Fields.Length > Index + 2)
                                {
                                    DriveHour = Fields[Index++];
                                    BackupVol = Fields[Index++];
                                    MsgType = Fields[Index++];
                                }
                                switch (MsgType)
                                {
                                    case "0":
                                        MsgType = "Real Time";
                                        break;
                                    case "1":
                                        MsgType = "Storage";
                                        break;
                                    default:
                                        MsgType = string.Empty;
                                        break;
                                }
                                switch (Data)
                                {
                                    case "1":
                                        Add = "Input1 LOW";
                                        break;
                                    case "2":
                                        Add = "Input1 HIGH";
                                        break;
                                    case "3":
                                        Add = "Input2 LOW";
                                        break;
                                    case "4":
                                        Add = "Input2 HIGH";
                                        break;
                                    case "5":
                                        Add = "Input3 LOW";
                                        break;
                                    case "6":
                                        Add = "Input3 HIGH";
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            case "UEX":
                                {
                                    if (Fields.Length <= Index + 2)
                                    {
                                        return Index;
                                    }
                                    string CheckSum;
                                    Type = "DATA EVENT";
                                    Add = "Rx Bytes: " + Fields[Index++];
                                    Data = Fields[Index++];
                                    CheckSum = Fields[Index++];
                                    if (Fields.Length > Index + 2)
                                    {
                                        DriveHour = Fields[Index++];
                                        BackupVol = Fields[Index++];
                                        MsgType = Fields[Index++];
                                    }
                                    switch (MsgType)
                                    {
                                        case "0":
                                            MsgType = "Real Time";
                                            break;
                                        case "1":
                                            MsgType = "Storage";
                                            break;
                                        default:
                                            MsgType = string.Empty;
                                            break;
                                    }
                                }
                                break;
                            default:
                                break;
                        }

                    }
                }
                else
                {
                    Type = "ALIVE";
                }
                //int Channel;
                //if (Units.TryGetValue(Id, out Channel))
                //{
                //    if (Channel != Arg.Id)
                //    {
                //        if (Units.Remove(Id))
                //        {
                //            Units.Add(Id, Arg.Id);
                //        }
                //    }
                //}
                //else
                //{
                //    Units.Add(Id, Arg.Id);
                //}
                //AddRow(Ip, "ST300", Id, Type, Model, Version, Date, Time, Cell, Lat, Lon, Speed, Course, Sat, Fix, Odo, Vol, Io, Data, Add);
                //AddProcessedFileRow(Ip, "ST300", Id, Type, Model, Version, Date, Time, Cell, Lat, Lon, Speed, Course, Sat, Fix, Odo, Vol, Io, Data, Add, DriveHour, BackupVol, MsgType);
                Console.WriteLine("\n CONVERSION   OBJETO  A   JSON \n");
                dynamic myObject = new ExpandoObject();
                myObject.name = "ST300";
                myObject.Id = Id;
                myObject.Type = Type;
                myObject.Model = Model;
                myObject.Version = Version;
                myObject.Date = Date;
                myObject.Time = Time;
                myObject.Cell = Cell;
                myObject.Lat = Lat;
                myObject.Lon = Lon;
                myObject.Speed = Speed;
                myObject.Course = Course;
                myObject.Sat = Sat;
                myObject.Fix = Fix;
                myObject.Odo = Odo;
                myObject.Vol = Vol;
                myObject.Io = Io;
                myObject.Data = Data;
                myObject.Add = Add;
                myObject.DriveHour = DriveHour;
                myObject.BackupVol = BackupVol;
                myObject.MsgType = MsgType;
                ///JSON OBJETO -> JSON   (SERIALIZACION)//////////////////////////////////////////////////
                string json1 = JsonConvert.SerializeObject(myObject);
                Console.WriteLine(json1);
                                            //Console.ReadKey();
                ///PRUEBA JSON -> OBJETO (DESERIALIZACION)//////////////////////////////////////////////////
                            //Console.WriteLine("\n IMPRIMIR JSO \n");
                            //JsonTextReader reader = new JsonTextReader(new StringReader(json1));

                            //while (reader.Read())
                            //{
                            //    if (reader.Value != null)
                            //    {
                            //        Console.WriteLine("Token: {0}, \n                          Value: {1}", reader.TokenType, reader.Value);
                            //        //Console.WriteLine("Value: {0}", reader.Value);
                            //    }
                            //    else
                            //    {
                            //        Console.WriteLine("Token: {0}", reader.TokenType);
                            //    }
                            //}
                                            //Console.ReadKey();
            }

            return Index;
        }

        static UInt16 ProcessCommandResponseST300(string Type, UInt16 Index, string[] Fields)
        {
            return Index;
        }
    
        ///////////////////////////////////////////
    }
}
