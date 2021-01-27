using System;
using Robot;

/// <summary>
/// All of the event args, mendatory in every class that uses events
/// </summary>

namespace EvArgsLibrary
{
    //ExtendedSerialPort: DataReceivedEvent
    public class DataReceivedArgs : EventArgs
    {
        public byte[] Data { get; set; }
    }

    //MessageDecoder: DataDecodedEvent
    public class DataDecodedArgs : EventArgs
    {
        public ushort DecodedFunction { get; set; }
        public ushort DecodedPayloadLength { get; set; }
        public byte[] DecodedPayload { get; set; }
        public byte DecodedCheckSum { get; set; }
        public bool CheckSumErrorOccured { get; set; }
    }

    //MessageProcessor: Processed data is text event
    public class TextDataProcessedArgs : EventArgs
    {
        public string ProcessedText { get; set; }
    }

    //MessageProcessor: Processed data is speed event
    public class SpeedDataProcessedArgs : EventArgs
    {
        public sbyte SpeedGauche { get; set; }
        public sbyte SpeedDroit { get; set; }
        public robot.MotorWays WayGauche { get; set; }
        public robot.MotorWays WayDroit { get; set; }
    }

    //messageProcessor : Error message received
    public class OnErrorMessageProcessedArgs : EventArgs
    {
        public byte[] PayloadData;
    }

    //MessageProcessor: Processed data is InfaredDistance event
    public class IrDataProcessedArgs : EventArgs
    {
        public byte[] Distance { get; set; }
    }

    //MessageProcessor: processed data ErrorOccured event
    public class CheckSumErrorOccuredArgs : EventArgs { }

    //PortSupervisor: available ports changed event
    public class AvailablePortChangedArgs : EventArgs
    {
        public string[] AvailableSerialPorts { get; set; }
    }

    //HerkulexReceptManager: packetFromServo decoded event
    public class HerkulexIncommingPacketDecodedArgs : EventArgs
    {
        public byte PacketSize { get; set; }
        public byte PID { get; set; }
        public byte CMD { get; set; }
        public byte CheckSum1 { get; set; }
        public byte CheckSum2 { get; set; }
        public byte[] PacketData { get; set; }
    }

    /*
     * Data redirection, virtual loopback
     */

    //DataRedirector: data redirection bridge (to any)
    public class RedirectedDataArgs : EventArgs
    {
        public byte[] Data { get; set; }
    }

    //HerkulexController: data redirection bridge (to data redirector)
    public class RedirectSentDataOutputArgs : EventArgs
    {
        public byte[] Data { get; set; }
    }

    public class PositionDataArgs : EventArgs

    {
        public ulong Timestamp;
        public float XPosFromodo;
        public float YPosFromodo;
        public float AngleRadOdo;
        public float VitesseLineaireOdo;
        public float VitesseAngulaireOdo;
        public float Kp_Lin;
        public float Ki_Lin;
        public float VitesseAngulaireConsigneFromRobot;
        public float VitesseLineaireConsigneFromRobot;
        public float Kp_Angl;
        public float Ki_Angl;
        public float Erreur_Lin;
        public float Erreur_Angl;
        public float P_out_lin;
        public float P_out_angl;
        public float I_out_lin;
        public float I_out_angl;
        public float D_out_lin;
        public float D_out_angl;
        public float Lin_out;
        public float Angl_out;

    }
}
