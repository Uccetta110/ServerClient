using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public class SynchronousSocketClient
{
    static string localIpAddressInput;

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

    static public async void clientTest(Socket handler)
    {
        await Task.Delay(5000); // Aspetta per 5 secondi

        // Invia un messaggio di test al client
        string testMessage = "Messaggio di test dal client " + localIpAddressInput + "<EOF>";
        byte[] msg = Encoding.ASCII.GetBytes(testMessage);

        // Assicurati di avere accesso al socket handler
        // Potresti dover passare il socket handler come parametro a questa funzione
        handler.Send(msg);
    }

    public static void StartClient(string ipAddressInput, string stringSend)
    {
        // Data buffer for incoming data.  
        byte[] bytes = new byte[1024];

        // Connect to a remote device.  
        try
        {
            // Establish the remote endpoint for the socket.  
            // This example uses port 11000 on the local computer.  
            IPAddress ipAddress = System.Net.IPAddress.Parse(ipAddressInput);
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 5000);

            // Create a TCP/IP  socket.  
            Socket sender = new Socket(ipAddress.AddressFamily,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect the socket to the remote endpoint. Catch any errors.  
            try
            {
                sender.Connect(remoteEP);

                Console.WriteLine("Socket connected to {0}",
                    sender.RemoteEndPoint.ToString());

                // Encode the data string into a byte array.  
                byte[] msg = Encoding.ASCII.GetBytes(stringSend + "<EOF>");

                // Send the data through the socket.  
                int bytesSent = sender.Send(msg);

                // Receive the response from the remote device.  
                int bytesRec = sender.Receive(bytes);
                Console.WriteLine("Echoed test = {0}",
                    Encoding.ASCII.GetString(bytes, 0, bytesRec));

                // Release the socket.  
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public static int Main(String[] args)
    {
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        Console.WriteLine("Vuoi usare l'indirizzo IP locale? (s/n)");
        string confirmation = Console.ReadLine();
        string ipAddressInput;
        if (confirmation.ToLower() == "s")
        {
            ipAddressInput = GetLocalIPv4();
        }
        else
        {
            Console.WriteLine("Inserisci l'indirizzo IP: ");
            ipAddressInput = Console.ReadLine();
            while (true)
            {
                Console.WriteLine("L'indirizzo IP è giusto: ");
                Console.WriteLine(ipAddressInput + " (s/n)");
                string confirmation1 = Console.ReadLine();
                if (confirmation1.ToLower() == "s")
                {
                    break;
                }
                else
                {
                    Console.WriteLine("Inserisci l'indirizzo IP corretto: ");
                    ipAddressInput = Console.ReadLine();
                }
            }
        }
        localIpAddressInput = ipAddressInput;


        while (true)
        {
            Console.WriteLine("Inserisci la stringa da mandare al server: ");
            string stringSend = Console.ReadLine();
            StartClient(localIpAddressInput, stringSend);
            Console.WriteLine("Premere un tasto per continuare ");
            Console.ReadLine();
        }

        return 0;
    }

    static void OnProcessExit(object sender, EventArgs e)
    {
        // Invia il messaggio finale
        StartClient(localIpAddressInput, "TERMINA CONNESSIONE");
    }
}
