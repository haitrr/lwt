import type { NextConfig } from 'next';

const nextConfig: NextConfig = {
  cacheComponents: true,
  partialPrefetching: true,
  output: 'standalone',
  // Word lists, dictionaries and migrations are read from disk at runtime, so tracing can't find them on its own.
  outputFileTracingIncludes: {
    '/*': ['./src/server/languages/data/**', './node_modules/kuromoji/dict/**', './drizzle/**', './dictionary/**'],
  },
};

export default nextConfig;
