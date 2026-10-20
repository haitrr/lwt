export async function register() {
  if (process.env.NEXT_RUNTIME === 'nodejs') {
    const { startTextProcessing } = await import('./server/processing');
    startTextProcessing();
  }
}
