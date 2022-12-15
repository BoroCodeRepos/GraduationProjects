#ifndef RINGBUFFER_RINGBUFFER_H_
#define RINGBUFFER_RINGBUFFER_H_

#include "../common.h"

typedef struct
{
	byte * Buffer;
	size_t Count;
	size_t Size;
	size_t Head;
	size_t Tail;
} RingBuffer_t;

extern void   RingBuffer_Init(RingBuffer_t * Q, byte * Buffer, size_t Size);
extern void   RingBuffer_Clear(RingBuffer_t * Q);
extern bool   RingBuffer_Insert(RingBuffer_t * Q, byte Data);
extern bool   RingBuffer_Get(RingBuffer_t * Q, byte * Data);
extern byte*  RingBuffer_GetAsString(RingBuffer_t * Q, byte * Buffer, byte Delimiter);
extern bool   RingBuffer_IsFull(RingBuffer_t * Q);
extern byte   RingBuffer_LastInsert(RingBuffer_t * Q);
extern size_t RingBuffer_Capacity(RingBuffer_t * Q);
extern size_t RingBuffer_Size(RingBuffer_t * Q);


#endif /* RINGBUFFER_RINGBUFFER_H_ */
