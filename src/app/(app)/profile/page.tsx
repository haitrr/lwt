import { LANGUAGES } from '@/lib/languages';
import { requireSession } from '@/server/auth/session';
import { getLanguageSettings, getUser } from '@/server/users';
import DictionarySettingsForm from './DictionarySettingsForm';
import PasswordForm from './PasswordForm';
import styles from './profile.module.css';

export const metadata = { title: 'Profile' };

export default async function ProfilePage() {
  const { userId } = await requireSession();
  const [user, settings] = await Promise.all([getUser(userId), getLanguageSettings(userId)]);
  return (
    <div className="stack narrow">
      <div className={styles.user}>
        <span className={styles.avatar} aria-hidden>
          {user.userName.slice(0, 1).toUpperCase()}
        </span>
        <h1>{user.userName}</h1>
      </div>
      <section className="card stack">
        <div>
          <h2>Dictionary</h2>
          <p className="subtle" style={{ margin: 0 }}>
            Which language to look up meanings in, for each language you read.
          </p>
        </div>
        <DictionarySettingsForm languages={LANGUAGES} settings={settings} />
      </section>
      <section className="card stack">
        <h2>Change password</h2>
        <PasswordForm />
      </section>
    </div>
  );
}
