#ifndef UART_H
#define	UART_H

#ifdef	__cplusplus
extern "C" {
#endif

void InitUART ( void ) ;
//void SendMessageDirect(unsigned char* message, int length);
//void __attribute__((interrupt , no_auto_psv)) _U1RXInterrupt(void);

#ifdef	__cplusplus
}
#endif

#endif	/* UART_H */
