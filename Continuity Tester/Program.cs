using System;
using System.IO.Ports;

namespace Continuity_Tester
{
    class Program
    {
        static SerialPort port;
        static string info = "Continuity tester / circuit indicator - https://github.com/pharmafirma/piepser";
        static string intro = "This program allows you to (ab)use any serial port as an electrical continuity tester. ";
        static string hint = "Use the RX and TX pins of {0} as your test probes. Press Ctrl+C to close the program. ";
        static string disclaimer = "*** Don't fry your computer: NEVER MEASURE CIRCUITS UNDER VOLTAGE! ***";
        static contactState lastState = contactState.UGLY;
        static contactState state = contactState.UGLY;
        static uint[] statistics = { 0, 0, 0, 0 };

        static void Main(string[] args)
        {
            // say hello
            Console.WriteLine(info);
            Console.WriteLine(intro);
            Console.WriteLine(hint, "your serial port");
            Console.WriteLine(disclaimer);
            Console.WriteLine();

            // init serial port
            port = new SerialPort();
            port.WriteTimeout = 100;
            port.ReadTimeout = 100;

            // Ask user and try to open port
            while (true)
            {
                try
                {
                    port.PortName = AskPortName();
                    port.Open();
                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error opening serial port: ");
                    Console.WriteLine(e.ToString());
                }
            }

            // do the magic
            while (true)
            {
                // perform the actual continuity test
                try
                {
                    port.WriteLine("42 ");
                    statistics[(int)contactState.UGLY]++;
                    try
                    {
                        string readstring = port.ReadLine();
                        if (readstring == "42 ")
                        {
                            // entire message received - good contact
                            state = contactState.GOOD;
                            statistics[(int)contactState.GOOD]++;
                        }
                        else
                        {
                            // something else received - bad contact
                            state = contactState.BAD;
                            statistics[(int)contactState.BAD]++;

                        }
                    }
                    catch (TimeoutException)
                    {
                        // nothing received - no contact
                        state = contactState.NONE;
                        statistics[(int)contactState.NONE]++;
                    }
                }
                catch
                {
                    // an error occurred, e.g. USB com adapter disconnected
                    state = contactState.ERROR;
                }

                // print new state to console
                if (state != lastState || statistics[(int)contactState.UGLY] % 100 == 0)
                {
                    switch (state)
                    {
                        case contactState.GOOD:
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.ForegroundColor = ConsoleColor.Black;
                            ConsoleClear();
                            Console.WriteLine("GOOD CONTACT :)");
                            break;
                        case contactState.BAD:
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.ForegroundColor = ConsoleColor.Black;
                            ConsoleClear();
                            Console.WriteLine("BAD CONTACT :/");
                            break;
                        case contactState.NONE:
                            Console.BackgroundColor = ConsoleColor.Black;
                            Console.ForegroundColor = ConsoleColor.White;
                            ConsoleClear();
                            Console.WriteLine("NO CONTACT :(");
                            break;
                        default:
                            Console.BackgroundColor = ConsoleColor.DarkYellow;
                            Console.ForegroundColor = ConsoleColor.Black;
                            ConsoleClear();
                            Console.WriteLine("!!! COM PORT ERROR !!!");
                            return;
                    }
                    lastState = state;
                }
            }
        }

        // Display list of COM ports and prompt user to enter a port.
        public static string AskPortName(string defaultPortName = "")
        {
            string portName;
            string[] foundPorts;

            // print available ports
            foundPorts = SerialPort.GetPortNames();
            Console.WriteLine("Available Ports:");
            foreach (string s in foundPorts)
            {
                Console.WriteLine("   {0}", s);
            }

            // for user's convenience, set a default value
            if (defaultPortName == "" && foundPorts.Length > 0)
            {
                defaultPortName = foundPorts[0];
            }

            Console.Write("Which serial port shall I use (press enter for default {0})? ", defaultPortName);
            portName = Console.ReadLine();
            // check if only enter was pressed
            if (portName == "")
            {
                portName = defaultPortName;
            }
            return portName;
        }

        // I don't like duplicate code
        public static void ConsoleClear()
        {
            Console.Clear();
            Console.WriteLine(hint, port.PortName);
            Console.WriteLine(disclaimer);
            Console.WriteLine("Statistics: Tests performed = {0}, good contact = {1}, no contact = {2}, bad contact = {3}",
                statistics[(int)contactState.UGLY],
                statistics[(int)contactState.GOOD],
                statistics[(int)contactState.NONE],
                statistics[(int)contactState.BAD]
                );
            Console.WriteLine();
        }

        enum contactState
        {
            GOOD = 0,
            BAD = 1,
            UGLY = 2, // not initialised, also used for the sent counter
            NONE = 3, // no contact
            ERROR,
        }
    }
}
