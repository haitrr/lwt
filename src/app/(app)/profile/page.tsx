import { LANGUAGES } from '@/lib/languages';
import { requireSession } from '@/server/auth/session';
import { getLanguageSettings, getUser } from '@/server/users';
import DictionarySettingsForm from './DictionarySettingsForm';
import PasswordForm from './PasswordForm';

export const metadata = { title: 'Profile' };

export default async function ProfilePage() {
  const { userId } = await requireSession();
  const [user, settings] = await Promise.all([getUser(userId), getLanguageSettings(userId)]);
  return (
    <div className="stack">
      <h1>{user.userName}</h1>
      <section>
        <h2>Dictionary</h2>
        <p>Which language to look up meanings in, for each language you read.</p>
        <DictionarySettingsForm languages={LANGUAGES} settings={settings} />
      </section>
      <section>
        <h2>Change password</h2>
        <PasswordForm />
      </section>
    </div>
  );
}
