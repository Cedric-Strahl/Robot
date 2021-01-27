#include <xc.h>
#include "Robot.h"
#include "QEI.h"
#include "UART.h"
#include "timer.h"
#include "Utilities.h"
#include "PWM.h"
#include <math.h>

extern Correcteur CorrLin;
extern Correcteur CorrAngl;

void InitQEI1()
{
    QEI1IOCbits.SWPAB = 1; //QEAX and QEBx are swapped
    QEI1GECL = 0xFFFF;
    QEI1GECH = 0xFFFF;
    QEI1CONbits.QEIEN = 1; //Enable QEI Module
}

void InitQEI2()
{
    QEI2IOCbits.SWPAB = 1;
    QEI2GECL = 0xFFFF; 
    QEI2GECH = 0XFFFF;
    QEI2CONbits.QEIEN = 1;
}

void QEIUpdateData()
{
    //save old values
    float QeiDroitPosition_T_1 = robotState.QeiDroitPosition;
    float QeiGauchePosition_T_1 = robotState.QeiGauchePosition;
    
    //refresh position values
    long QEI1RawValue = (long)POS1CNTL;
    QEI1RawValue += ((long)POS1HLD << 16);
    
    long QEI2RawValue = (long)POS2CNTL;
    QEI2RawValue += ((long)POS2HLD << 16);
    
    //convert to mm
    robotState.QeiDroitPosition = ((float)POINT_TO_METER*QEI1RawValue);
    robotState.QeiGauchePosition = ((float)-POINT_TO_METER*QEI2RawValue);
    
    //computing position deltas
    float delta_d = robotState.QeiDroitPosition - QeiDroitPosition_T_1;
    float delta_g = robotState.QeiGauchePosition - QeiGauchePosition_T_1;
    
    //delta_tetha = atan((atan(delta_d - delta_d)/DISTROUES);
    float delta_theta = (delta_d - delta_g)/(float)DISTROUES;
    //double ds = (delta_d + delta_g) / 2; //delta_s
    
    //speed computation
    //ATTENTION: multiplier par la freq d'échantillonage
    
    robotState.vitesseDroitFromOdometry = delta_d*(float)FREQ_ECH_QEI;
    robotState.vitesseGaucheFromOdometry = delta_g*(float)FREQ_ECH_QEI;
    robotState.vitesseLineaireFromOdometry = (robotState.vitesseDroitFromOdometry + robotState.vitesseGaucheFromOdometry)/2;
    robotState.vitesseAngulaireFromOdometry = delta_theta*(float)FREQ_ECH_QEI;
    
    //mise à jour du positionnement terrain à t-1
    robotState.xPosFromOdometry_1 = robotState.xPosFromOdometry;
    robotState.yPosFromOdometry_1 = robotState.yPosFromOdometry;
    robotState.angleRadianFromOdometry_1 = robotState.angleRadianFromOdometry;
    
    //calcul des positions dans le referentiel du terrain
    float theta_rad = robotState.angleRadianFromOdometry_1 + delta_theta;
    robotState.xPosFromOdometry = robotState.xPosFromOdometry_1 + (robotState.vitesseAngulaireFromOdometry/(float)FREQ_ECH_QEI)*cos(theta_rad);
    robotState.yPosFromOdometry = robotState.yPosFromOdometry_1 + (robotState.vitesseLineaireFromOdometry/(float)FREQ_ECH_QEI)*sin(theta_rad);
    robotState.angleRadianFromOdometry = theta_rad;
    
    if(robotState.angleRadianFromOdometry > (float)PI) robotState.angleRadianFromOdometry -= 2*(float)PI;
    if(robotState.angleRadianFromOdometry < (float)-PI) robotState.angleRadianFromOdometry += 2*(float)PI;
}

 void SendPositionData()
{
     
    unsigned char positionPayload[48];
    getBytesFromInt32(positionPayload, 0, timestamp);
    
    getBytesFromFloat(positionPayload, 4, (float)robotState.xPosFromOdometry);
    getBytesFromFloat(positionPayload, 8, (float)robotState.yPosFromOdometry);
    
    getBytesFromFloat(positionPayload, 12, (float)robotState.angleRadianFromOdometry);
    
    getBytesFromFloat(positionPayload, 16, (float)robotState.vitesseLineaireFromOdometry);
    getBytesFromFloat(positionPayload, 20, (float)robotState.vitesseAngulaireFromOdometry);
    
    //debug
    getBytesFromFloat(positionPayload, 24, (float)CorrLin.Kp);
    getBytesFromFloat(positionPayload, 28, (float)CorrLin.Ki);
    
    getBytesFromFloat(positionPayload, 32, (float)robotState.vitesseAngulaireConsigne);
    getBytesFromFloat(positionPayload, 36, (float)robotState.vitesseLineaireConsigne);
    
    getBytesFromFloat(positionPayload, 40, (float)CorrAngl.Kp);
    getBytesFromFloat(positionPayload, 44, (float)CorrAngl.Ki);
    
    getBytesFromFloat(positionPayload, 48, (float)CorrLin.Error);
    getBytesFromFloat(positionPayload, 52, (float)CorrAngl.Error);
    
    getBytesFromFloat(positionPayload, 56, (float)CorrLin.P_Output);
    getBytesFromFloat(positionPayload, 60, (float)CorrAngl.P_Output);
    
    getBytesFromFloat(positionPayload, 64, (float)CorrLin.I_Output);
    getBytesFromFloat(positionPayload, 68, (float)CorrAngl.I_Output);
    
    getBytesFromFloat(positionPayload, 72, (float)CorrLin.D_Output);
    getBytesFromFloat(positionPayload, 76, (float)CorrAngl.D_Output);
    
    getBytesFromFloat(positionPayload, 80, (float)CorrLin.Output);
    getBytesFromFloat(positionPayload, 84, (float)CorrAngl.Output);

    UartEncodeAndSendMessage(CMD_POSITION_DATA, 88, positionPayload);

}