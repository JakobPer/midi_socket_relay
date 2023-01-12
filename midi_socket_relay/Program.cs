using Commons.Music.Midi;
using WebSocketSharp.Server;
using CoreMidi;
using System.Net.Sockets;
using System.Net;
using System;
using System.Text;
using System.Runtime.CompilerServices;
using System.Net.WebSockets;

/// <summary>
/// We just need a stub
/// </summary>
internal class MIDIService : WebSocketBehavior
{
}

internal class Program
{
    private static void Main(string[] args)
    {
        var server = new WebSocketServer(IPAddress.Parse("127.0.0.1"), 9000);
        server.AddWebSocketService<MIDIService>("/");
        var access = MidiAccessManager.Default;

        Console.WriteLine("MIDI Inputs:");

        foreach (var input in access.Inputs)
        {
            Console.WriteLine("ID: {1}\tName: {0}", input.Name, input.Id);
        }

        Console.WriteLine("Enter input ID: ");

        var line = Console.ReadLine();

        try
        {
            var midiIn = access.OpenInputAsync(line).Result;

            Console.WriteLine("Input selected.");

            server.Start();
            server.WebSocketServices.TryGetServiceHost("/", out var serviceHost);

            midiIn.MessageReceived += (sender, e) =>
            {
                Console.WriteLine(BitConverter.ToString(e.Data, e.Start, e.Length));

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < e.Length; i++)
                {
                    sb.Append((char)e.Data[i + e.Start]);
                }
                serviceHost.Sessions.Broadcast(sb.ToString());
            };

        }
        catch (Exception e)
        {
            Console.WriteLine("Could not open midi device!");
            Console.WriteLine(e.Message);
            System.Environment.Exit(-1);
        }


        Console.ReadKey();
        server.Stop();
    }
}