export async function register() {
  if (process.env.NEXT_RUNTIME === 'nodejs' && process.env.NODE_ENV === 'production') {
    // Refuse to start with a weak session secret. Throwing here would leave the server up, answering every request
    // with a 500, so exit instead.
    const { jwtSecret } = await import('./server/auth/secret');
    try {
      jwtSecret();
    } catch (e) {
      console.error((e as Error).message);
      process.exit(1);
    }
  }
  if (process.env.NEXT_RUNTIME === 'nodejs' && process.env.DATABASE_URL) {
    const { migrateDatabase } = await import('./server/db/migrate');
    await migrateDatabase();
    const { startTextProcessing } = await import('./server/processing');
    startTextProcessing();
    const { importDictionaries } = await import('./server/dictionaryImport');
    importDictionaries().catch((e) => console.error('Importing the built-in dictionaries failed', e));
  }
}
