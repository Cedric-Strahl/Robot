#ifndef QEI_H
#define	QEI_H
#include "Utilities.h"

#define DIAM_CODEUR_ROUE 0.0426 //en m
#define POINT_TO_METER  (PI*DIAM_CODEUR_ROUE)/8192
#define DISTROUES 0.220//0.1536//0.2185 //0.18762 roue codeur -> roue pivot
#define FREQ_ECH_QEI 250 //Hz, affects robot acceleration via PWMUpdateSpeed() in timer1


void InitQEI1(void); //init QEI1
void InitQEI2(void); //init QEI2

void QEIUpdateData(void); //update odometry data

void SendPositionData(void); //sends odometry data via uart(24bytes payload)

#endif



