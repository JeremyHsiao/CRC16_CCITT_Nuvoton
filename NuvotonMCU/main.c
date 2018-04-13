/**************************************************************************//**
 * @file     main.c
 * @version  V1.00
 * $Revision: 1 $
 * $Date: 14/07/17 10:00p $
 * @brief    Uart driver demo sample.
 *
 * @note
 * Copyright (C) 2014 Nuvoton Technology Corp. All rights reserved.
 *
 ******************************************************************************/
#include <stdio.h>
#include <string.h>
#include "N575.h"
#include "uart_app.h"
#include "buffer.h"
#include "base64.h"

// For testing CRC demo code
#include <stdio.h>
#include <stdlib.h>
// END - or testing CRC demo code

/* Buffer size, this buffer for uart receive & send data. */
#define UART_BUF_SIZE      64

void SYS_Init(void)
{
    /*---------------------------------------------------------------------------------------------------------*/
    /* Init System Clock                                                                                       */
    /*---------------------------------------------------------------------------------------------------------*/
    /* Unlock protected registers */
    SYS_UnlockReg();

    /* Enable External OSC49M */
    CLK_EnableXtalRC(CLK_PWRCTL_HIRCEN_Msk);

    /* Switch HCLK clock source to HXT */
    CLK_SetHCLK(CLK_CLKSEL0_HCLKSEL_HIRC, CLK_CLKSEL0_HIRCFSEL_48M, CLK_CLKDIV0_HCLK(1));

    /* Enable IP clock */
    CLK_EnableModuleClock(UART_MODULE);

    /* Update System Core Clock */
    /* User can use SystemCoreClockUpdate() to calculate SystemCoreClock. */
    SystemCoreClockUpdate();

    /*---------------------------------------------------------------------------------------------------------*/
    /* Init I/O Multi-function                                                                                 */
    /*---------------------------------------------------------------------------------------------------------*/
    /* Set GPG multi-function pins for UART0 RXD and TXD */
	SYS->GPA_MFP  = (SYS->GPA_MFP & (~SYS_GPA_MFP_PA8MFP_Msk) ) | SYS_GPA_MFP_PA8MFP_UART_TX;
	SYS->GPA_MFP  = (SYS->GPA_MFP & (~SYS_GPA_MFP_PA9MFP_Msk) ) | SYS_GPA_MFP_PA9MFP_UART_RX;

    /* Lock protected registers */
    SYS_LockReg();
}

// For testing CRC demo code
#define MAX_PACKET_TEST 510
#define CRC_PKT 32
uint8_t CRCdata[MAX_PACKET_TEST+2];
// END - For testing CRC demo code

/* Main */
int main(void)
{
//	uint8_t u8Buffer[UART_BUF_SIZE];
	
    /* Init System, IP clock and multi-function I/O */
    SYS_Init();

    Initialize_buffer();
    UART_init();
    /* Init UART to 115200-8n1 for print message */
        
    OutputString_with_newline("\r\n+----------------------+\r");
    OutputString_with_newline    ("| N575 Uart + CRC Demo |\r");
    OutputString_with_newline    ("+----------------------+\r");
	OutputString_with_newline    ("Press any key to test.\r");
	
    Init_decoding_base64();
    
	while(1)
	{
//// For testing CRC demo code
//        int32_t  i,j,TestPassed=1;
//		uint16_t CalcCRC;
//        uint32_t *u32ptr;
//// END-For testing CRC demo code
//        
//// For testing CRC demo code
//        // Fill data array
//		for (i=0; i<MAX_PACKET_TEST; i++)
//			CRCdata[i]= rand();

//        CRC_Open();

//		// Test a 64byte CRC
//		u32ptr = (uint32_t *)CRCdata;
//        CRC_Init( CRC_LSB, CRC_PKT);
//		CalcCRC = CRC_Calc( u32ptr, CRC_PKT);
//        OutputString("Calculate CRC of ");
//		for (i=0;i<CRC_PKT;i++)
//			OutputHexValue_uint8(CRCdata[i]);
//        OutputString(" is ");
//        OutputHexValue_uint16(CalcCRC);
//        OutputString_with_newline("\r");

//		// Test by adding CRC to packet and recalculating
//		CRCdata[i++] = CalcCRC >> 8;
//		CRCdata[i++] = CalcCRC & 0xFF;
//		u32ptr = (uint32_t *)CRCdata;
//        CRC_Init( CRC_LSB, CRC_PKT+2);
//		CalcCRC = CRC_Calc( u32ptr, CRC_PKT+2);
//		if (CalcCRC != 0)
//			OutputString_with_newline("CRC Failed check\r");
//		else
//			OutputString_with_newline("CRC Passed check\r");

//		OutputString_with_newline("\n\rCalculate CRC of various packets and check results\r");
//		for (j=2; j<=MAX_PACKET_TEST ; j+=2)
//		{
//        // Do CRC for j bytes
//            u32ptr = (uint32_t *)CRCdata;
//            CRC_Init( CRC_LSB, j);
//			CalcCRC = CRC_Calc( u32ptr, j);

//            OutputString(".");
//            if (j%64==0)
//            {
//                OutputString("\r\nCRC ");
//                OutputDecValue(j);
//                OutputString(" bytes is ");
//                OutputHexValue_uint16(CalcCRC);
//                OutputString("\r\n ");
//            }
//            
//            // Now check CRC
//            CRCdata[j++] =  CRC->CHECKSUM >> 8;
//            CRCdata[j++] =  CRC->CHECKSUM & 0xff;
//            u32ptr = (uint32_t *)CRCdata;
//            CRC_Init( CRC_LSB, j);
//            CalcCRC = CRC_Calc( u32ptr, j);
//            j-=2;
//			if (CalcCRC != 0)
//			{
//				OutputString_with_newline("CRC Fail\r");
//				TestPassed =0;
//			}
//            CRCdata[j++] = rand();
//            CRCdata[j++] = rand();
//            j-=2;
//		}
//		if (TestPassed)
//			OutputString_with_newline("\n\rCRC driver sample passed all tests.\r");
//		else
//			OutputString_with_newline("\n\rCRC driver sample failed.\r");

//		CRC_Close();
//// END-For testing CRC demo code

//        if(!uart_input_queue_empty_status())
//        {
//            uint8_t input_ch;
//            input_ch = uart_input_dequeue();
//            while(uart_output_enqueue(input_ch)==0) {}
//        }
        
        while (!uart_input_queue_empty_status()) 
        {
            uint32_t decode_value;
            uint8_t temp_byte;
            
            temp_byte = uart_input_dequeue();
            decode_value = Process_input_base64_byte(temp_byte);
            
            if(decode_value!=0)     // result (or error)
            {
                temp_byte = (decode_value>>24);
            
                switch(temp_byte)
                {
                    case 0x81:
                        OutputHexValue(decode_value&0xff);
                        break;
                    case 0x82:
                        OutputHexValue(decode_value&0xffff);
                        break;
                    case 0x83:
                        OutputHexValue(decode_value&0xffffff);
                        break;
                    default:
                        // Data corruption - 0xff
                        OutputString_with_newline(" - corrupted\r");
                        break;
                }
            }
        }
	}
}

/*** (C) COPYRIGHT 2014 Nuvoton Technology Corp. ***/
