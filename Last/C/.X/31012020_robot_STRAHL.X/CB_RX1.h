#ifndef CB_RX1_H
#define	CB_RX1_H

int CB_RX1_GetDataSize(void);
int CB_RX1_GetRemainingSize(void);
void __attribute__((interrupt, no_auto_psv)) _U1RXInterrupt(void);
unsigned char CB_RX1_IsDataAvailable(void);
unsigned char CB_RX1_Get(void);
void CB_RX1_Add(unsigned char value);

#endif	/* CB_RX_H */

