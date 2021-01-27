using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using EvArgsLibrary;
using GUI;
using ExtendedSerialPort;
using MessageDecoder;
using SharpDX.XInput;
using XboxPolarGeneratorNs;


namespace RobotConsole
{
    class RobotConsole
    {
        //static Controller xboxOneController = new Controller(UserIndex.One);
        //static ManualResetEvent XboxPollBlock = new ManualResetEvent(false);
        static XboxPolarGenerator xboxPolarGenerator;

        static ExtendedSerialPort.ReliableSerialPort port = new ReliableSerialPort("COM9", 115200, System.IO.Ports.Parity.None, 8, System.IO.Ports.StopBits.One);
        static MessageDecoder.msgDecoder decoder = new msgDecoder();
        static MessageProcessor.msgProcessor processor = new MessageProcessor.msgProcessor();
        static MessageEncoder.Encoder encoder = new MessageEncoder.Encoder();
        static interfaceRobot UI;
        static Thread sendSpeedThread;
        static bool usingUI;
        
        static void Main(string[] args)
        {
           
            
            port.DataReceived += decoder.DecodeMessage;
            decoder.OnDataDecodedEvent += processor.ProcessMessage;
            processor.onPositionDataProcessedEvent += Processor_onPositionDataProcessedEvent;
            processor.OnErrorMessageProcessedEvent += Processor_OnErrorMessageProcessedEvent;

            usingUI = true;

            sendSpeedThread = new Thread(sendSpeedThreadProc);

            port.Open();

            xboxPolarGenerator = new XboxPolarGenerator(2f, (float)-(1 * Math.PI), 4000, 5);
            xboxPolarGenerator.OnPolarVectGeneratedEvent += XboxPolarGenerator_OnPolarVectGeneratedEvent;

            if (usingUI)
                StartRobotInterface();

            //sendSpeedThread.Start();
            //XboxPollBlock.Set();

            Thread.CurrentThread.Join();
         }

        private static void Processor_OnErrorMessageProcessedEvent(object sender, OnErrorMessageProcessedArgs e)
        {
            Console.Write("\n[{0}] Got error from robot, payload: ", DateTime.Now);
            foreach(byte b in e.PayloadData)
            {
                Console.Write(b.ToString("X2") + " - ");
            }
            Console.WriteLine();
        }

        private static void XboxPolarGenerator_OnPolarVectGeneratedEvent(object sender, OnPolarVectGeneratedEventArgs e)
        {
            encoder.SendConsignePolaire(port, e.LinearSpeed, e.AngularSpeed);
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     Console.WriteLine("Linear: {0}  Angular: {1}", e.LinearSpeed, e.AngularSpeed);
        }

        static void sendSpeedThreadProc()
        {
                encoder.SendConsignePolaire(port, 0f, 0f);
        }

        private static void Processor_onPositionDataProcessedEvent(object sender, EvArgsLibrary.PositionDataArgs e)
        {
            /*
            Console.WriteLine("X: " + e.XposFromOdometry);    
            Console.WriteLine("Y: " + e.YposFromOdometry);    
            Console.WriteLine("VitesseLin: " + e.VitesseLineaireFromOdometry);    
            Console.WriteLine("VitesseAng: " + e.VitesseAngulaireFromOdometry);    
            Console.WriteLine("AngleRad: " + e.AngleRadFromOdometry);
            Console.WriteLine("VitesseGauche: " + e.VitesseGauche);
            Console.WriteLine("VitesseDroit: " + e.VitesseDroit);
            */
        }

        static Thread ui_thread;
        static void StartRobotInterface()
        {
            ui_thread = new Thread(() =>
            {
                UI = new interfaceRobot();
                processor.onPositionDataProcessedEvent += UI.UpdateLocation;
                UI.ShowDialog();
            });
            ui_thread.SetApartmentState(ApartmentState.STA);
            ui_thread.Start();
        }

       // static Timer ControllerPolling = new Timer((c) =>
       //{
       //     XboxPollBlock.WaitOne();
       //     float vitesseLineaireConsigne = 0;
       //     float vitesseLineaireConsigneFactor = 3f/255f; //1.5 m/s max

       //     float vitesseAngulaireConsigne = 0;
       //     float vitesseAngulaireConsigneFactor = 3* (float)Math.PI / 255f; //1.5 m/s max

       //    State controllerState;
       //    if (xboxOneController.IsConnected)
       //    {
       //        controllerState = xboxOneController.GetState();
       //        vitesseLineaireConsigne = controllerState.Gamepad.RightTrigger * vitesseLineaireConsigneFactor;
       //        vitesseAngulaireConsigne = controllerState.Gamepad.LeftTrigger * vitesseAngulaireConsigneFactor;
       //        Console.WriteLine("linear: {0}   angular: {1}", vitesseLineaireConsigne, vitesseAngulaireConsigne);
       //        encoder.SendConsignePolaire(port, vitesseLineaireConsigne, vitesseAngulaireConsigne);
       //    }
       //}, null, 0, 10);

    }
}

