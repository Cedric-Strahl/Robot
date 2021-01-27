#include <stdio.h>
#include <stdlib.h>
#include <xc.h>
#include <libpic30.h>
#include "ChipConfig.h"
#include "IO.h"
#include "timer.h"
#include "PWM.h"
#include "ADC.h"
#include "main.h"
#include "Robot.h"
#include "UART.h"
#include "CB_TX1.h"
#include "CB_RX1.h"
#include "QEI.h"
#include "Utilities.h"

unsigned char rcvState = Waiting;
unsigned short msgDecodedFunction;
unsigned short msgDecodedPayloadLength;
unsigned char msgDecodedPayload[1024]; //128 bytes amximum, use ReadData()
unsigned char receivedCheckSum;
unsigned char CheckSumErrorOccured;
unsigned char calculatedCheckSum;
unsigned short msgDecodedPayloadIndex;
unsigned char messageAvailable = 0;
unsigned char LED_CODE;
unsigned char LED_STATE;

void processMessage();

void DecodeLoop()
{

    if(CB_RX1_IsDataAvailable())
    {
        int i;
        for(i = 0; i < CB_RX1_GetDataSize(); i++)
        {
            unsigned char c = CB_RX1_Get();
            switch(rcvState)
            {
                case Waiting:
                    if(c == 0xFE)
                        rcvState = FunctionMSB;

                    break;

                case FunctionMSB:
                    msgDecodedFunction = (unsigned short)(c << 8);
                    rcvState = FunctionLSB;
                    break;

                case FunctionLSB:
                    msgDecodedFunction += (unsigned short)(c << 0);
                    rcvState = PayloadLengthMSB;
                    break;

                case PayloadLengthMSB:
                    msgDecodedPayloadLength = (unsigned short)(c << 8);
                    rcvState = PayloadLengthLSB;
                    break;

                case PayloadLengthLSB:
                    msgDecodedPayloadLength += (unsigned short)(c << 0);
                    if (msgDecodedPayloadLength > 0)
                    {
                        msgDecodedPayloadIndex = 0;
                        rcvState = Payload;
                    }
                    else
                        rcvState = CheckSum; //if no payload, skip to CheckSum state

                    break;

                case Payload:
                    if (msgDecodedPayloadIndex < msgDecodedPayloadLength)
                    {
                        msgDecodedPayload[msgDecodedPayloadIndex] = c;
                        msgDecodedPayloadIndex++;
                        if (msgDecodedPayloadIndex == msgDecodedPayloadLength)
                            rcvState = CheckSum;
                    }
                    break;

                case CheckSum:
                    receivedCheckSum = c;
                    calculatedCheckSum = CalculateChecksum(msgDecodedFunction, msgDecodedPayloadLength, msgDecodedPayload);
                    if (calculatedCheckSum == receivedCheckSum)
                    {
                        CheckSumErrorOccured = 0;
                        messageAvailable = 1;
                        processMessage();
                    }
                    else
                    {
                       CheckSumErrorOccured = 1;
                       
                       messageAvailable = 0;
                    }
                    
                    rcvState = Waiting;

                    break;

                default:
                    rcvState = Waiting;
                    break;
            }
        }
    }

}

void processMessage()
{       
            switch(msgDecodedFunction)
            {
                case CMD_SPEED_CONSIGNE_POLAIRE:
                {
                    float linearSpeed = getFloat(msgDecodedPayload, 0); //first 4 bytes
                    float angularSpeed = getFloat(msgDecodedPayload, 4); //last 4 bytes
                    robotState.vitesseLineaireConsigne = linearSpeed* 3.3475;
                    robotState.vitesseAngulaireConsigne = angularSpeed* 3.3475;
                }
                break;
            }
            
            if(CheckSumErrorOccured)
            {
                CheckSumErrorOccured = 0;
            }
            
            messageAvailable = 0;
}

unsigned long TSample = 0;
extern Correcteur CorrLin;
extern Correcteur CorrAngl;
volatile unsigned long enteredTimeStamp=0;
int main(void) 
{
    //init stuff
    InitOscillator();
    InitTimer23();
    InitTimer1();
    InitTimer4();
    InitIO();
    InitPWM();
    InitADC1();
    InitUART();
    InitQEI1();
    InitQEI2();
    
    //vitesse angulaire = 5_58cm*13.5cm_10min
    //vitesse linéaire = 0.5_2m_13.39s


    //PI tunning avec ziegler-Nichols
    CorrLin.Kp = 1.9;//9/2; 
    CorrLin.Ki = 0;//200
    
    CorrAngl.Kp = 0.6;//0.8/2;
    CorrAngl.Ki = 0;//50
    
    robotState.vitesseAngulaireConsigne = 0* 3.3475 ; //rad/s
    robotState.vitesseLineaireConsigne = 0*3.3475;// m/s
    
    while(1)
    { 
        DecodeLoop();
        if(timestamp>enteredTimeStamp+100)
        {
            enteredTimeStamp=timestamp;
            LED_BLANCHE = !LED_BLANCHE;
        }
    }
}
        
         /*
 * loopback
        int i ;
        for ( i =0; i< CB_RX1_GetDataSize ( ) ; i++)
        {
            unsigned char c = CB_RX1_Get ( ) ;
            SendMessage(&c , 1 ) ;
        }
         * */
