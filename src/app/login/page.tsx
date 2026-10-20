import AuthForm from '../AuthForm';
import { login } from '@/lib/auth';

export const metadata = { title: 'Login' };

export default function LoginPage() {
  return <AuthForm title="Login" action={login} alternate={{ href: '/register', label: 'Create an account' }} />;
}
