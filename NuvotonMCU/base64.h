/****************************************************************************
 * @file     base64.h
 * @version  V1.00
 * $Revision: 1 $
 * $Date: 2018/04/03 12:00p $
 * @brief    Header file for base64 decoding
 *
 * @note
 * 
 *
 ******************************************************************************/
#ifndef _BASE64_H_
#define _BASE64_H_

//
// External Function declaration
//     
extern void Init_decoding_base64(void);
extern uint32_t Process_input_base64_byte(uint8_t input_byte);

#endif /* !_BASE64_H_ */
