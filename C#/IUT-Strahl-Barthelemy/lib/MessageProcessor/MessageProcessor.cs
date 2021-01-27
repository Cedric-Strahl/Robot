using System;
using System.Text;
using EvArgsLibrary;
using Robot;

/// <summary>
/// this class is used to process the previously decoded message and output events
/// depending on the message for a simpler, higer level use.
/// </summary>

namespace MessageProcessor
{

    internal enum CMD_CODE
    {
        TEXT = 0x0080,
        INFRARED = 0x0030,
        RAW_SPEED = 0x0040,
        ODOMETRY_DATA = 0x0061,
        ERROR = 0xE0E0
    }

    public class msgProcessor
    { 
        //contains the processor state machine
        public void ProcessMessage(object Sender, DataDecodedArgs e)
        {
            ushort function = e.DecodedFunction;
            byte[] irDistance = new byte[5];

            robot.MotorWays wayGauche;
            robot.MotorWays wayDroit;

            sbyte speedGauche;
            sbyte speedDroit;

            if (e.CheckSumErrorOccured)
                OnCheckSumErrorOccured();
            else 
            { 
                switch(function)
                {
                    case (ushort)CMD_CODE.TEXT: //TextMessageReceived
                        OnTextMessageProcessed(Encoding.UTF8.GetString(e.DecodedPayload));
                        break;

                    case (ushort)CMD_CODE.INFRARED: //Infared distance data received, depreciated
                        for (int i = 0; i < e.DecodedPayload.Length; i++)
                            irDistance[i] = e.DecodedPayload[i];

                        OnIrMessageProcessed(irDistance);
                        break;

                    case (ushort)CMD_CODE.RAW_SPEED: //depreciated, using odometry data instead
                        speedGauche = (sbyte)e.DecodedPayload[0];
                        speedDroit = (sbyte)e.DecodedPayload[1];

                        if (speedGauche >= 0) //if 0, saying foward...
                            wayGauche = robot.MotorWays.Avance;
                        else                                   //first bit is a 0, going reverse
                            wayGauche = robot.MotorWays.Recule;

                        if(speedDroit >= 0)   //first bit is a 1, going foward
                            wayDroit = robot.MotorWays.Avance;
                        else                                   //first bit is a 0, going reverse
                            wayDroit = robot.MotorWays.Recule;

                        OnSpeedMessageProcessed(speedGauche, speedDroit,
                                                wayGauche, wayDroit);

                        break;

                    case (ushort)CMD_CODE.ODOMETRY_DATA: //odometry data received
                        ulong timestamp = 0;

                        timestamp =  (ulong)(e.DecodedPayload[0] << 24);
                        timestamp += (ulong)(e.DecodedPayload[1] << 16);
                        timestamp += (ulong)(e.DecodedPayload[2] << 8);
                        timestamp += (ulong)(e.DecodedPayload[3] << 0);

                        float xPosFromodo = BitConverter.ToSingle(e.DecodedPayload, 4);
                        float yPosFromodo = BitConverter.ToSingle(e.DecodedPayload, 8);
                        float angleRadOdo = BitConverter.ToSingle(e.DecodedPayload, 12);
                        float vitesseLneaireOdo = BitConverter.ToSingle(e.DecodedPayload, 16);
                        float vitesseAngulaireOdo = BitConverter.ToSingle(e.DecodedPayload, 20);
                        //
                        float kp_Lin = BitConverter.ToSingle(e.DecodedPayload, 24);
                        float ki_Lin = BitConverter.ToSingle(e.DecodedPayload, 28);

                        float vitesseAngulaireConsigneFromRobot = BitConverter.ToSingle(e.DecodedPayload, 32);
                        float vitesseLineaireConsigneFromRobot = BitConverter.ToSingle(e.DecodedPayload, 36);

                        float kp_Angl = BitConverter.ToSingle(e.DecodedPayload, 40);
                        float ki_Angl = BitConverter.ToSingle(e.DecodedPayload, 44);

                        float erreur_Lin = BitConverter.ToSingle(e.DecodedPayload, 48);
                        float erreur_Angl = BitConverter.ToSingle(e.DecodedPayload, 52);

                        float p_out_lin = BitConverter.ToSingle(e.DecodedPayload, 56);
                        float p_out_angl = BitConverter.ToSingle(e.DecodedPayload, 60);
                        float i_out_lin = BitConverter.ToSingle(e.DecodedPayload, 64);
                        float i_out_angl = BitConverter.ToSingle(e.DecodedPayload, 68);
                        float d_out_lin = BitConverter.ToSingle(e.DecodedPayload, 72);
                        float d_out_angl = BitConverter.ToSingle(e.DecodedPayload, 76);

                        float lin_out = BitConverter.ToSingle(e.DecodedPayload, 80);
                        float angl_out = BitConverter.ToSingle(e.DecodedPayload, 84);

                        onPositionDataReceived(timestamp, xPosFromodo, yPosFromodo, angleRadOdo, vitesseLneaireOdo,
                                               vitesseAngulaireOdo, kp_Lin, ki_Lin, vitesseAngulaireConsigneFromRobot,
                                               vitesseLineaireConsigneFromRobot, kp_Angl, ki_Angl, erreur_Lin, erreur_Angl, p_out_lin,
                                               p_out_angl, i_out_lin, i_out_angl, d_out_lin, d_out_angl, lin_out, angl_out);
                        break;

                    case (ushort)CMD_CODE.ERROR:
                        OnErrorMessageProcessed(e.DecodedPayload);
                        break;
                }
            }

        }


        public event EventHandler<PositionDataArgs> onPositionDataProcessedEvent;
        public event EventHandler<TextDataProcessedArgs> OnTextMessageProcessedEvent;
        public event EventHandler<IrDataProcessedArgs> OnIrMessageProcessedEvent;
        public event EventHandler<SpeedDataProcessedArgs> OnSpeedMessageProcessedEvent;
        public event EventHandler<CheckSumErrorOccuredArgs> OnCheckSumErrorOccuredEvent;
        public event EventHandler<OnErrorMessageProcessedArgs> OnErrorMessageProcessedEvent;



        //le robot revoie un message d'erreur
        public virtual void OnErrorMessageProcessed(byte[] payloadData)
        {
            var handler = OnErrorMessageProcessedEvent;
            if (handler != null)
            {
                handler(this, new OnErrorMessageProcessedArgs
                {
                    PayloadData = payloadData
                });
            }
        }

        public virtual void onPositionDataReceived(ulong timestamp, float xPosFromodo, float yPosFromodo, float angleRadOdo, float vitesseLneaireOdo,
                                               float vitesseAngulaireOdo, float kp_Lin, float ki_Lin, float vitesseAngulaireConsigneFromRobot,
                                               float vitesseLineaireConsigneFromRobot, float kp_Angl, float ki_Angl, float erreur_Lin, float erreur_Angl, float p_out_lin,
                                               float p_out_angl, float i_out_lin, float i_out_angl, float d_out_lin, float d_out_angl, float lin_out, float angl_out)
        {
            var handler = onPositionDataProcessedEvent;
            if(handler != null)
            { 
                handler(this, new PositionDataArgs
                {
                    Timestamp = timestamp,
                    XPosFromodo = xPosFromodo,
                    YPosFromodo = yPosFromodo,
                    AngleRadOdo = angleRadOdo,
                    VitesseLineaireOdo = vitesseLneaireOdo,
                    VitesseAngulaireOdo = vitesseAngulaireOdo,
                    Kp_Lin = kp_Lin,
                    Ki_Lin = ki_Lin,
                    VitesseAngulaireConsigneFromRobot = vitesseAngulaireConsigneFromRobot,
                    VitesseLineaireConsigneFromRobot = vitesseLineaireConsigneFromRobot,
                    Kp_Angl = kp_Angl,
                    Ki_Angl = ki_Angl,
                    Erreur_Lin = erreur_Lin,
                    Erreur_Angl = erreur_Angl,
                    P_out_lin = p_out_lin,
                    P_out_angl = p_out_angl,
                    I_out_lin = i_out_lin,
                    I_out_angl = i_out_angl,
                    D_out_lin = d_out_lin,
                    D_out_angl = d_out_angl,
                    Lin_out = lin_out,
                    Angl_out = angl_out,
                });
            }
        }

        public virtual void OnCheckSumErrorOccured()
        {
            var handler = OnCheckSumErrorOccuredEvent;
            if(handler != null)
            {
                handler(this, new CheckSumErrorOccuredArgs{});
            }
        }

        public virtual void OnIrMessageProcessed(byte[] distance)
        {
            var handler = OnIrMessageProcessedEvent;
            if(handler != null)
            {
                handler(this, new IrDataProcessedArgs
                {
                    Distance = distance
                });
            }
        }

        public virtual void OnTextMessageProcessed(string text)
        {
            var handler = OnTextMessageProcessedEvent;
            if(handler != null)
            {
                handler(this, new TextDataProcessedArgs
                {
                    ProcessedText = text
                });
            }
        }

        public virtual void OnSpeedMessageProcessed(sbyte speedGauche, sbyte speedDroit, robot.MotorWays wayGauche, robot.MotorWays wayDroit)
        {
            var handler = OnSpeedMessageProcessedEvent;
            if(handler != null)
            {
                handler(this, new SpeedDataProcessedArgs
                {
                    SpeedGauche = speedGauche,
                    SpeedDroit = speedDroit,
                    WayGauche = wayGauche,
                    WayDroit = wayDroit
                });
            }
        }
    }
}
