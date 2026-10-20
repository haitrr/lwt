import type { Tx } from './db';
import { schema } from './db';

// The logs table doubles as the "last read text" source. Data is JSON with the PascalCase
// keys Newtonsoft produced, kept identical so old and new rows read the same.
export async function addLog(
  tx: Pick<Tx, 'insert'>,
  event: 'textCreated' | 'textRead' | 'termCreated' | 'termEdited',
  data: Record<string, unknown>,
  userName: string,
  entity?: { type: 'Text' | 'Term'; id: number },
) {
  await tx.insert(schema.logs).values({
    event,
    data: JSON.stringify(data),
    userName,
    entityType: entity?.type ?? null,
    entityId: entity?.id ?? null,
  });
}
