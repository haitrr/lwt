import AuthForm from '../AuthForm';
import { login } from '@/lib/auth';

export const metadata = { title: 'Login' };

export default function LoginPage() {
  return (
    <AuthForm
      title="Welcome back"
      subtitle="Sign in to continue reading."
      submitLabel="Login"
      action={login}
      alternate={{ prompt: 'New here?', href: '/register', label: 'Create an account' }}
    />
  );
}
