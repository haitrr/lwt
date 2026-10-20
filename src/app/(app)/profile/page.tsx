import { LANGUAGES } from '@/lib/languages';
import { requireSession } from '@/server/auth/session';
import { getLanguageSettings, getUser } from '@/server/users';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import DictionarySettingsForm from './DictionarySettingsForm';
import PasswordForm from './PasswordForm';

export const metadata = { title: 'Profile' };

export default async function ProfilePage() {
  const { userId } = await requireSession();
  const [user, settings] = await Promise.all([getUser(userId), getLanguageSettings(userId)]);
  return (
    <div className="flex max-w-3xl flex-col gap-6">
      <div className="flex items-center gap-3">
        <span
          className="grid size-12 place-items-center rounded-full bg-primary text-xl font-semibold text-primary-foreground"
          aria-hidden
        >
          {user.userName.slice(0, 1).toUpperCase()}
        </span>
        <h1 className="text-2xl font-semibold tracking-tight">{user.userName}</h1>
      </div>
      <Card>
        <CardHeader>
          <CardTitle>Dictionary</CardTitle>
          <CardDescription>Which language to look up meanings in, for each language you read.</CardDescription>
        </CardHeader>
        <CardContent>
          <DictionarySettingsForm languages={LANGUAGES} settings={settings} />
        </CardContent>
      </Card>
      <Card>
        <CardHeader>
          <CardTitle>Change password</CardTitle>
        </CardHeader>
        <CardContent>
          <PasswordForm />
        </CardContent>
      </Card>
    </div>
  );
}
