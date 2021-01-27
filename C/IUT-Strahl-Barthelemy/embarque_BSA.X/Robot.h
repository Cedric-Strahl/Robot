/* 
 * File:   Robot.h
 * Author: TP-EO-1
 *
 * Created on 6 février 2020, 15:26
 */

#ifndef ROBOT_H
#define	ROBOT_H

#define SPEED 0x0040
#define TEXT 0x0080
#define LED 0x0020

#define CODE_LED_ORANGE 1
#define CODE_LED_BLEUE 2
#define CODE_LED_BLANCHE 3


typedef struct robotStateBITS
{
    union{
        struct{
            unsigned char taskEnCours;
            
            float vitesseGaucheConsigne;
            float vitesseGaucheCommandeCourante;
            float vitesseDroiteConsigne;
            float vitesseDroiteCommandeCourante;
          
            float vitesseLineaireConsigne;
            float vitesseAngulaireConsigne;

            float QeiDroitPosition;
            float QeiGauchePosition;
            
            float vitesseDroitFromOdometry;
            float vitesseGaucheFromOdometry;
            float vitesseLineaireFromOdometry;
            float vitesseAngulaireFromOdometry;
            
            float xPosFromOdometry_1;
            float xPosFromOdometry;
            float yPosFromOdometry_1;
            float yPosFromOdometry;
            float angleRadianFromOdometry_1;
            float angleRadianFromOdometry;
            float delta_d;
            float delta_g;
            float delta_theta;
            float ds;
        };
    };
}ROBOT_STATE_BITS;
extern volatile ROBOT_STATE_BITS robotState;
#endif /*ROBOT_H*/