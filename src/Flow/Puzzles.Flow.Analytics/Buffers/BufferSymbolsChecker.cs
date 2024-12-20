#if USE_ARRAY_POOL && USE_NATIVE_MEMORY
#warning If both symbols 'USE_ARRAY_POOL' and 'USE_NATIVE_MEMORY' are set, symbol 'USE_NATIVE_MEMORY' will be ignored.
#elif !USE_ARRAY_POOL && !USE_NATIVE_MEMORY
#error You must set at least one symbol 'USE_ARRAY_POOL' and 'USE_NATIVE_MEMORY'.
#endif
