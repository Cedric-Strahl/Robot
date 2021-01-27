//Partie PWM
#include <xc.h> // library xc.h inclut tous les uC
#include "IO.h"
#include "PWM.h"
#include "Robot.h"
#include "Utilities.h"
#include "QEI.h"


Correcteur CorrLin;
Correcteur CorrAngl;

unsigned char acceleration = 20;

void InitPWM(void)
{
    PTCON2bits.PCLKDIV = 0b000; //Divide by 1
    PTPER = 100*PWMPER; //P�riode en pourcentage

    //R�glage PWM moteur 1 sur hacheur 1
    IOCON1bits.POLH = 1; //High = 1 and active on low =0
    IOCON1bits.POLL = 1; //High = 1
    IOCON1bits.PMOD = 0b01; //Set PWM Mode to Redundant
    FCLCON1 = 0x0003; //D�sactive la gestion des faults

    //Reglage PWM moteur 2 sur hacheur 6
    IOCON6bits.POLH = 1; //High = 1
    IOCON6bits.POLL = 1; //High = 1
    IOCON6bits.PMOD = 0b01; //Set PWM Mode to Redundant
    FCLCON6 = 0x0003; //D�sactive la gestion des faults

    /* Enable PWM Module */
    PTCONbits.PTEN = 1;
}

/*
void PWMSetSpeed (float vitesseEnPourcents, unsigned char moteur)
{
    if(moteur == MOTEUR_GAUCHE)
    {
        robotState.vitesseGaucheCommandeCourante = vitesseEnPourcents;
            if(vitesseEnPourcents>0)
            {
                //Avant
                MOTEUR_GAUCHE_ENH = 0; //Pilotage de la pin en mode IO
                MOTEUR_GAUCHE_INH = 1; //Mise � 1 de la pin
                MOTEUR_GAUCHE_ENL = 1; //Pilotage de la pin en mode PWM
            }
            else
            {
                //Arrriere
                MOTEUR_GAUCHE_ENL = 0; //Pilotage de la pin en mode IO
                MOTEUR_GAUCHE_INL = 1; //Mise � 1 de la pin
                MOTEUR_GAUCHE_ENH = 1; //Pilotage de la pin en mode PWM
            }
        MOTEUR_GAUCHE_DUTY_CYCLE = Abs(robotState.vitesseGaucheCommandeCourante*PWMPER);
        
    }
    else
    {
     robotState.vitesseDroiteCommandeCourante = vitesseEnPourcents;
            if(vitesseEnPourcents<0)
        {
            //Avant
            MOTEUR_DROIT_ENH = 0; //Pilotage de la pin en mode IO
            MOTEUR_DROIT_INH = 1; //Mise � 1 de la pin
            MOTEUR_DROIT_ENL = 1; //Pilotage de la pin en mode PWM
        }
        else
        {
            //Arrriere
            MOTEUR_DROIT_ENL = 0; //Pilotage de la pin en mode IO
            MOTEUR_DROIT_INL = 1; //Mise � 1 de la pin
            MOTEUR_DROIT_ENH = 1; //Pilotage de la pin en mode PWM
        }
    MOTEUR_DROIT_DUTY_CYCLE = Abs(robotState.vitesseDroiteCommandeCourante*PWMPER);
    }
    
}
*/

void PWMUpdateSpeed()
{
    // Cette fonction est appel�e sur timer et permet de suivre des rampes d?acc�l�ration
    if (robotState.vitesseDroiteCommandeCourante < robotState.vitesseDroiteConsigne)
    robotState.vitesseDroiteCommandeCourante = Min(
    robotState.vitesseDroiteCommandeCourante + acceleration,
    robotState.vitesseDroiteConsigne);
    if (robotState.vitesseDroiteCommandeCourante > robotState.vitesseDroiteConsigne)
    robotState.vitesseDroiteCommandeCourante = Max(
    robotState.vitesseDroiteCommandeCourante - acceleration,
    robotState.vitesseDroiteConsigne);

    if (robotState.vitesseDroiteCommandeCourante > 0)
    {
    MOTEUR_DROIT_ENL = 0; //pilotage de la pin en mode IO
    MOTEUR_DROIT_INL = 1; //Mise � 1 de la pin
    MOTEUR_DROIT_ENH = 1; //Pilotage de la pin en mode PWM
    }
    else
    {
    MOTEUR_DROIT_ENH = 0; //pilotage de la pin en mode IO
    MOTEUR_DROIT_INH = 1; //Mise � 1 de la pin
    MOTEUR_DROIT_ENL = 1; //Pilotage de la pin en mode PWM
    }
    MOTEUR_DROIT_DUTY_CYCLE = Abs(robotState.vitesseDroiteCommandeCourante)*PWMPER;

    if (robotState.vitesseGaucheCommandeCourante < robotState.vitesseGaucheConsigne)
    robotState.vitesseGaucheCommandeCourante = Min(
    robotState.vitesseGaucheCommandeCourante + acceleration,
    robotState.vitesseGaucheConsigne);
    if (robotState.vitesseGaucheCommandeCourante > robotState.vitesseGaucheConsigne)
    robotState.vitesseGaucheCommandeCourante = Max(
    robotState.vitesseGaucheCommandeCourante - acceleration,
    robotState.vitesseGaucheConsigne);

    if (robotState.vitesseGaucheCommandeCourante < 0)
    {
    MOTEUR_GAUCHE_ENL = 0; //pilotage de la pin en mode IO
    MOTEUR_GAUCHE_INL = 1; //Mise � 1 de la pin
    MOTEUR_GAUCHE_ENH = 1; //Pilotage de la pin en mode PWM
    }
    else
    {
    MOTEUR_GAUCHE_ENH = 0; //pilotage de la pin en mode IO
    MOTEUR_GAUCHE_INH = 1; //Mise � 1 de la pin
    MOTEUR_GAUCHE_ENL = 1; //Pilotage de la pin en mode PWM
    }
    MOTEUR_GAUCHE_DUTY_CYCLE = Abs(robotState.vitesseGaucheCommandeCourante) * PWMPER;
}

void PWMSetSpeedConsigne(float vitesseEnPourcents, unsigned char moteur)
{
    if(moteur == MOTEUR_GAUCHE)
        robotState.vitesseGaucheConsigne = vitesseEnPourcents;
    else
        robotState.vitesseDroiteConsigne = vitesseEnPourcents;
}



void UpdateAsserv()
{
    //corections PI
    CorrLin.Error = robotState.vitesseLineaireConsigne - robotState.vitesseLineaireFromOdometry;
    CorrLin.P_Output = (CorrLin.Kp * CorrLin.Error) ;
    CorrLin.I_Output = ((CorrLin.Error * CorrLin.Ki) / FREQ_ECH_QEI);
    CorrLin.I_Output = LimitToInterval(CorrLin.I_Output, -600, 600);
    
    CorrLin.Output = (CorrLin.P_Output + CorrLin.I_Output)*COEFF_VITESSE_LINEAIRE_PERCENT;
    
    CorrAngl.Error = robotState.vitesseAngulaireConsigne - robotState.vitesseAngulaireFromOdometry;
    CorrAngl.P_Output = (CorrAngl.Kp * CorrAngl.Error);
    CorrAngl.I_Output = ((CorrAngl.Error * CorrAngl.Ki) / FREQ_ECH_QEI);
    CorrAngl.I_Output = LimitToInterval(CorrAngl.I_Output, -600, 600);
    
    CorrAngl.Output = (CorrAngl.P_Output + CorrAngl.I_Output)*COEFF_VITESSE_ANGULAIRE_PERCENT;
    
    //generation des consignes droite et gauche
    robotState.vitesseDroiteConsigne = CorrLin.Output + (DISTROUES/2)*CorrAngl.Output;
    robotState.vitesseDroiteConsigne = LimitToInterval(robotState.vitesseDroiteConsigne, -100, 100);
    
    robotState.vitesseGaucheConsigne = CorrLin.Output - (DISTROUES/2)*CorrAngl.Output;
    robotState.vitesseGaucheConsigne = LimitToInterval(robotState.vitesseGaucheConsigne, -100, 100);
}