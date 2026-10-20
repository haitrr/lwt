export async function register() {
  if (process.env.NEXT_RUNTIME === 'nodejs' && process.env.DATABASE_URL) {
    const { migrateDatabase } = await import('./server/db/migrate');
    await migrateDatabase();
    const { startTextProcessing } = await import('./server/processing');
    startTextProcessing();
  }
}
