#ifndef QEI_H
#define	QEI_H
#include "Utilities.h"

#define DIAM_CODEUR_ROUE 0.0426 //en m
#define POINT_TO_METER  (PI*DIAM_CODEUR_ROUE)/8192
#define DISTROUES 0.1536//0.2185
#define FREQ_ECH_QEI 250 //Hz, affects robot acceleration via PWMUpdateSpeed() in timer1

void SendPositionData(void); //sends odometry data via uart(24bytes payload)
void QEIUpdateData(void); //update odometry data

void InitQEI1(void); //init QEI1
void InitQEI2(void); //init QEI2

#endif



