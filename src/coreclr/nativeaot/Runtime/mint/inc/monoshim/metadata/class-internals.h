#ifndef _MONOSHIM_METADATA_CLASS_INTERNALS_H
#define _MONOSHIM_METADATA_CLASS_INTERNALS_H

typedef enum {
    MONO_WRAPPER_NONE = 0,
    MONO_WRAPPER_SYNCHRONIZED,
    MONO_WRAPPER_DYNAMIC_METHOD,

    MONO_WRAPPER_NUM
} MonoWrapperType;

#endif
