using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class SynchronousSocketListener
{

    // Incoming data from the client.  
    public static string data = null;
    public static bool connection = false;
    public static bool connectionTest = false;
    public static string localIPv4 = "";

    static string GetLocalIPv4()
    {
        foreach (var ip in Dns.GetHostAddresses(Dns.GetHostName()))
        {
            if (ip.AddressFamily == AddressFamily.InterNetwork) // Filtra solo IPv4
            {
                return ip.ToString();
            }
        }
        return null; // Nessun IPv4 trovato
    }

    static async void clientTest(Socket listener)
    {
        if (connection == false)
            return;
        await Task.Delay(5000); // Aspetta per 5 secondi
        if (connectionTest == false)
        {
            Console.WriteLine("Tempo di attesa dal client terminato. Nessuna connessione ricevuta.");
            connection = false;
        }
        else
        {
            connectionTest = false; 
        }
    }

    public static void StartListening()
    {
        
        // Data buffer for incoming data.  
        byte[] bytes = new Byte[1024];

        // Establish the local endpoint for the socket.  
        // Dns.GetHostName returns the name of the   
        // host running the application.  

        IPAddress ipAddress = System.Net.IPAddress.Parse(localIPv4);
        IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 5000);

        // Create a TCP/IP socket.  
        Socket listener = new Socket(ipAddress.AddressFamily,
            SocketType.Stream, ProtocolType.Tcp);

        // Bind the socket to the local endpoint and   
        // listen for incoming connections.  
        try
        {
            listener.Bind(localEndPoint);
            listener.Listen(10);

            // Start listening for connections.  
            while (true)
            {
                if (connection == false)
                Console.WriteLine("Waiting for a connection...");
                // Program is suspended while waiting for an incoming connection.  
                Socket handler = listener.Accept();
                data = null;
                connection = true;
                connectionTest = true;

                // An incoming connection needs to be processed.  
                while (true)
                {
                    int bytesRec = handler.Receive(bytes);
                    data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    if (data.IndexOf("<EOF>") > -1)
                    {
                        break;
                    }
                }

                if (data == "Messaggio di test dal client " + localIPv4 + "<EOF>")
                    connectionTest = true;
                // Show the data on the console.  
                Console.WriteLine("Text received : {0}", data);

                // Echo the data back to the client.  
                byte[] msg = Encoding.ASCII.GetBytes(data);

                handler.Send(msg);
                if (data.ToUpper()=="TERMINA CONNESSIONE<EOF>")
                {
                    handler.Shutdown(SocketShutdown.Both);
                    handler.Close();
                    connection = false;
                    connectionTest = false;
                }
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }

        Console.WriteLine("\nPress ENTER to continue...");
        Console.Read();

    }

    public static int Main(String[] args)
    {
        localIPv4 = GetLocalIPv4();
        StartListening();
        return 0;
    }
}
