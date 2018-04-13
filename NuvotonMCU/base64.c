/**************************************************************************//**
 * @file     base64.c
 * @version  V1.00
 * $Revision: 1 $
 * $Date: 18/04/03 11:55a $
 * @brief    base64 decode function -- will be tested with encode function Convert.ToBase64String() of C#
 *
 * @note
 *
 *
 ******************************************************************************/
#include "N575.h"
#include "base64.h"

typedef enum {
    WAIT_1ST_BASE64 = 0,
    WAIT_2ND_BASE64,
    WAIT_3RD_BASE64,
    WAIT_4TH_BASE64,
    WAIT_ONE_MORE_EQUAL_SIGN,
    ERROR_BASE64_STATE,
    MAX_BASE64_STATE
} BASE64_DECODING_STATE;

static const int8_t decoding[] = {62,-1,-1,-1,63,52,53,54,55,56,57,58,59,60,61,-1,-1,-1,-2,-1,-1,-1,0,1,2,3,4,5,6,7,8,9,10,11,12,13,14,15,16,17,18,19,20,21,22,23,24,25,-1,-1,-1,-1,-1,-1,26,27,28,29,30,31,32,33,34,35,36,37,38,39,40,41,42,43,44,45,46,47,48,49,50,51};
static BASE64_DECODING_STATE current_base64_decoding_state;
static uint32_t working_value;
    
void Init_decoding_base64(void)
{
    current_base64_decoding_state = WAIT_1ST_BASE64;
    working_value = 0;
}

uint32_t Process_input_base64_byte(uint8_t input_byte)
{
    uint32_t return_value = 0;
    
    if ((input_byte < '+') || (input_byte > 'z')) 
    {
        Init_decoding_base64();
        return_value = 0xffffffff;      // Error occured
    }
    else
    {
        int8_t  temp_value;
        // shift range into decoding table range 
        input_byte -= '+';
        temp_value = decoding[input_byte];
        if (temp_value>=0)
        {
            input_byte = (uint8_t) temp_value;   
            switch (current_base64_decoding_state)
            {
                case WAIT_1ST_BASE64:
                    working_value = input_byte;
                    working_value <<= 6;            // 2 lines to prevent overflow of shifting (uint8_t) data to left by 6 bits
                    current_base64_decoding_state++;
                    break;
                case WAIT_2ND_BASE64:
                case WAIT_3RD_BASE64:
                    working_value |= input_byte;
                    working_value <<= 6;
                    current_base64_decoding_state++;
                    break;
                case WAIT_4TH_BASE64:
                    return_value = working_value | input_byte;
                    return_value |= 0x83000000;     // indicate that return data is valid & 3 bytes of data
                    working_value = 0;
                    current_base64_decoding_state = WAIT_1ST_BASE64;
                    break;
                default:
                    // Error
                    Init_decoding_base64();
                    return_value = 0xffffffff;      // Error occured
                    break;
            }
        }
        else if (temp_value==-2)
        {
            // got "=", end of data
            switch (current_base64_decoding_state)
            {
                case WAIT_3RD_BASE64:
                    return_value = working_value>>4;
                    return_value |= 0x81000000;     // indicate that return data is valid & 1 byte of data
                    working_value = 0;
                    current_base64_decoding_state = WAIT_ONE_MORE_EQUAL_SIGN;
                    break;
                case WAIT_4TH_BASE64:
                    return_value = working_value>>2;
                    return_value |= 0x82000000;     // indicate that return data is valid & 2 byte of data
                    working_value = 0;
                    current_base64_decoding_state = WAIT_1ST_BASE64;
                    break;
                default:
                    // Error - 
                    Init_decoding_base64();
                    return_value = 0xffffffff;      // Error occured
                    break;
            }
        }
        else 
        {
            // Shouldn't be here -- must be coding error somewhere
            Init_decoding_base64();
            return_value = 0xffffffff;      // Error occured
        }
    }
    
    return return_value;
}
