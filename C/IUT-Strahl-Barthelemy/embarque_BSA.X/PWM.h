/* 
 * File:   PWM.h
 * Author: TP-EO-1
 *
 * Created on 6 février 2020, 15:48
 */

#ifndef PWM_H
#define	PWM_H


#define PWMPER 40.0

//adapter les unités de la consigne (mesuré)
#define COEFF_VITESSE_ANGULAIRE_PERCENT 31
#define COEFF_VITESSE_LINEAIRE_PERCENT 20

typedef struct _Correcteur
{
    float Kp;
    float Ki;
    float Kd;
    
    float Error;
    float P_Output;
    float I_Output;
    float D_Output;
    
    float Output;
    
    float AdaptConsigne;
    
}Correcteur;


void UpdateAsserv(void); 
void PWMSetSpeedConsigne(float vitesseEnPourcents, unsigned char moteur);
void InitPWM(void);
void PWMUpdateSpeed(void);

#endif	/* PWM_H */

