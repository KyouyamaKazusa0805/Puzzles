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
#error Dynamic allocation is not supported with 'ArrayPool<TreeNode>' and 'NativeMemory.Alloc' pattern. Consider configuring symbol 'USE_NEW_ARRAY' to apply 'new TreeNode[]'.
#endif
