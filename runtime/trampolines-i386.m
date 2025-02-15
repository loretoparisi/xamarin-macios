﻿
#if defined(__i386__)

#include <stdint.h>
#include <stdlib.h>
#include <stdio.h>
#include <string.h>

#include <objc/objc.h>
#include <objc/runtime.h>
#include <objc/message.h>

#include "trampolines-internal.h"
#include "xamarin/runtime.h"
#include "runtime-internal.h"
#include "trampolines-i386.h"

//#define TRACE
#ifdef TRACE
#define LOGZ(...) fprintf (stderr, __VA_ARGS__);
#else
#define LOGZ(...) ;
#endif

static void
throw_mt_exception (char *msg)
{
	MonoException *ex = xamarin_create_exception (msg);
	xamarin_free (msg);
	mono_raise_exception (ex);
}

#ifdef TRACE
static void
dump_state (struct CallState *state, id self, SEL sel)
{
	fprintf (stderr, "type: %u is_stret: %i self: %p SEL: %s eax: 0x%x edx: 0x%x esp: 0x%x -- double_ret: %f float_ret: %f\n",
		state->type, (state->type & Tramp_Stret) == Tramp_Stret, self, sel_getName (sel), state->eax, state->edx, state->esp,
		state->double_ret, state->float_ret);
}
#else
#define dump_state(...)
#endif

static void
param_iter_next (enum IteratorAction action, void *context, const char *type, size_t size, void *target)
{
	struct ParamIterator *it = (struct ParamIterator *) context;
	
	if (action == IteratorStart) {
		bool is_stret = (it->state->type & Tramp_Stret) == Tramp_Stret;
		// skip past the pointer to the previous function, and the two (three in case of stret) first arguments (id + SEL).
		it->stack_next = (uint8_t *) (it->state->esp + (is_stret ? 16 : 12));
		if (is_stret) {
			it->stret = *(uint8_t **) (it->state->esp + 4);
			it->state->eax = (uint32_t) it->stret;
		} else {
			it->stret = NULL;
		}
		LOGZ("initialized parameter iterator to %p stret to %p\n", it->stack_next, it->stret);
		return;
	} else if (action == IteratorEnd) {
		return;
	}

	// target must be at least pointer sized, and we need to zero it out first.
	if (target != NULL)
		*(uint32_t *) target = 0;

	// passed on the stack
	if (target != NULL) {
		LOGZ("read %lu bytes from stack pointer at %p\n", size, it->stack_next);
		memcpy (target, it->stack_next, size);
	} else {
		LOGZ("skipped over %lu bytes from stack pointer at %p\n", size, it->stack_next);
	}
	// increment stack pointer
	it->stack_next += size;
	// and round up to 4 bytes.
	if (size % 4 != 0)
		it->stack_next += 4 - size % 4;
}

static void
marshal_return_value (void *context, const char *type, size_t size, void *vvalue, MonoType *mtype, bool retain, MonoMethod *method)
{
	MonoObject *value = (MonoObject *) vvalue;
	struct ParamIterator *it = (struct ParamIterator *) context;

	LOGZ (" marshalling return value %p as %s\n", value, type);

	it->state->double_ret = 0;

	switch (type [0]) {
	case _C_FLT:
		// single floating point return value
		it->state->float_ret = *(float *) mono_object_unbox (value);
		break;
	case _C_DBL:
		// double floating point return value
		it->state->double_ret = *(double *) mono_object_unbox (value);
		break;
	case _C_STRUCT_B:
		if (it->state->type == Tramp_DoubleStret || it->state->type == Tramp_StaticDoubleStret) {
			void *unboxed = mono_object_unbox (value);
			memcpy ((void *) it->stret, unboxed, size);
			break;
		}
	
		if (size > 4 && size <= 8) {
			// returned in %eax and %edx
			void *unboxed = mono_object_unbox (value);

			// read the struct into 2 32bit values.
			uint32_t v[2];
			v[0] = *(uint32_t *) unboxed;
			// read as much as we can of the second value
			unboxed = 1 + (uint32_t *) unboxed;
			if (size == 8) {
				v[1] = *(uint32_t *) unboxed;
			} else if (size == 6) {
				v[1] = *(uint16_t *) unboxed;
			} else if (size == 5) {
				v[1] = *(uint8_t *) unboxed;
			}
			it->state->eax = v[0];
			it->state->edx = v[1];
		} else if (size == 4) {
			it->state->eax = *(uint32_t *) mono_object_unbox (value);
		} else if (size > 8) {
			// Passed in memory. it->stret points to caller-allocated memory.
			memcpy ((void *) it->stret, mono_object_unbox (value), size);
		} else {
			throw_mt_exception (xamarin_strdup_printf ("Xamarin.iOS: Cannot marshal struct return type %s (size: %i)\n", type, (int) size));
		}
		break;
	// For primitive types we get a pointer to the actual value
	case _C_BOOL: // signed char
	case _C_CHR:
	case _C_UCHR:
		it->state->eax = *(uint8_t *) mono_object_unbox (value);
		break;
	case _C_SHT:
	case _C_USHT:
		it->state->eax = *(uint16_t *) mono_object_unbox (value);
		break;
	case _C_INT:
	case _C_UINT:
		it->state->eax = *(uint32_t *) mono_object_unbox (value);
		break;
	case _C_LNG:
	case _C_ULNG:
	case _C_LNG_LNG:
	case _C_ULNG_LNG:
		*(uint64_t *) &it->state->eax = *(uint64_t *) mono_object_unbox (value);
		break;
	
	// For pointer types we get the value itself.
	case _C_CLASS:
	case _C_SEL:
	case _C_ID:
	case _C_CHARPTR:
	case _C_PTR:
		if (value == NULL) {
			it->state->eax = 0;
			break;
		}

		it->state->eax = (uint32_t) xamarin_marshal_return_value (mtype, type, value, retain, method);
		break;
	case _C_VOID:
		break;
	case '|': // direct pointer value
	default:
		if (size == 4) {
			it->state->eax = (uint32_t) value;
		} else {
			throw_mt_exception (xamarin_strdup_printf ("Xamarin.iOS: Cannot marshal return type %s (size: %i)\n", type, (int) size));
		}
		break;
	}
	
}

void
xamarin_arch_trampoline (struct CallState *state)
{
	enum TrampolineType type = (enum TrampolineType) state->type;
	bool is_stret = (type & Tramp_Stret) == Tramp_Stret;
	int offset = is_stret ? 1 : 0;
	id self = ((id *) state->esp) [offset + 1];
	SEL sel = ((SEL *) state->esp) [offset + 2];
	dump_state (state, self, sel);
	struct ParamIterator iter;
	iter.state = state;
	xamarin_invoke_trampoline (type, self, sel, param_iter_next, marshal_return_value, &iter);
	dump_state (state, self, sel);
}

#endif /* __i386__ */