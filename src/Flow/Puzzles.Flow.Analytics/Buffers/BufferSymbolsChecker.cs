// This file doesn't contain any possible APIs. This file only checks for symbols configured in csproj file.

#if USE_NEW_ARRAY && USE_ARRAY_POOL && USE_NATIVE_MEMORY
#warning If both symbols 'USE_ARRAY_POOL', 'USE_NEW_ARRAY' and 'USE_NATIVE_MEMORY' are set, symbols 'USE_ARRAY_POOL' and 'USE_NATIVE_MEMORY' will be ignored.
#elif USE_ARRAY_POOL && USE_NATIVE_MEMORY
#warning If both symbols 'USE_ARRAY_POOL' and 'USE_NATIVE_MEMORY' are set, symbol 'USE_NATIVE_MEMORY' will be ignored.
#elif USE_NEW_ARRAY && USE_ARRAY_POOL
#warning If both symbols 'USE_ARRAY_POOL' and 'USE_NEW_ARRAY' are set, symbol 'USE_ARRAY_POOL' will be ignored.
#elif USE_NEW_ARRAY && USE_NATIVE_MEMORY
#warning If both symbols 'USE_NATIVE_MEMORY' and 'USE_NEW_ARRAY' are set, symbol 'USE_NATIVE_MEMORY' will be ignored.
#elif !USE_ARRAY_POOL && !USE_NATIVE_MEMORY && !USE_NEW_ARRAY
#error You must set at least one symbol 'USE_ARRAY_POOL', 'USE_NEW_ARRAY' or 'USE_NATIVE_MEMORY'.
#endif

#if DYNAMIC_ALLOCATION && (USE_ARRAY_POOL || USE_NATIVE_MEMORY)
#warning Today the source code has a potential bug on growing memory for such allocation rule. Please be careful to modify code. It is suggested that you use symbol 'USE_NEW_ARRAY' to create a new array.
#endif
