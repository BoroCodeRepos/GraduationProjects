#include "RingBuffer.h"

void RingBuffer_Init(RingBuffer_t * Q, byte * Buffer, size_t Size)
{
	// initialization variables
	Q->Buffer = Buffer;
	Q->Size   = Size;
	Q->Head   = (size_t)0;
	Q->Tail   = (size_t)0;
	Q->Count  = (size_t)0;
}

void RingBuffer_Clear(RingBuffer_t * Q)
{
	Q->Head  = (size_t)0;
	Q->Tail  = (size_t)0;
	Q->Count = (size_t)0;
}

bool RingBuffer_Insert(RingBuffer_t * Q, byte Data)
{
	size_t NextHead = Q->Head + 1;
	if (NextHead == Q->Size)
	{
		// Buffer circulation
		NextHead = (size_t)0;
	}
	if (NextHead == Q->Tail)
	{
		// Buffer is full
		return false;
	}

	// inserting data
	Q->Buffer[NextHead] = Data;
	Q->Head = NextHead;
	Q->Count++;

	// everything OK
	return true;
}

bool RingBuffer_Get(RingBuffer_t * Q, byte * Data)
{
	if (Q->Head == Q->Tail)
	{
		// buffer empty
		return false;
	}

	// next byte
	Q->Tail++;
	if (Q->Tail == Q->Size)
	{
		// buffer circulation
		Q->Tail = (size_t)0;
	}

	*Data = Q->Buffer[Q->Tail];
	Q->Count--;

	return true;
}

byte * RingBuffer_GetAsString(RingBuffer_t * Q, byte * Buffer, byte Delimiter)
{
	byte Data;
	size_t i = 0;
	while (RingBuffer_Get(Q, &Data))
	{
		if (Data == Delimiter)
		{
			break;
		}
		Buffer[i] = Data;
		i++;
	}
	Buffer[i] = 0;
	return Buffer;
}

bool RingBuffer_IsFull(RingBuffer_t * Q)
{
	size_t NextHead = Q->Head + 1;
	if (NextHead == Q->Size)
	{
		// Buffer circulation
		NextHead = (size_t)0;
	}
	if (NextHead == Q->Tail)
	{
		// Buffer is full
		return true;
	}
	return false;
}

size_t RingBuffer_Capacity(RingBuffer_t * Q)
{
	return Q->Size;
}

size_t RingBuffer_Size(RingBuffer_t * Q)
{
	return Q->Count;
}

byte RingBuffer_LastInsert(RingBuffer_t * Q)
{
	return Q->Buffer[Q->Head];
}
