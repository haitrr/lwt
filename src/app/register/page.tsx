import AuthForm from '../AuthForm';
import { register } from '@/lib/auth';

export const metadata = { title: 'Register' };

export default function RegisterPage() {
  return (
    <AuthForm
      title="Create an account"
      subtitle="Read texts in any language and track the words you learn."
      submitLabel="Register"
      action={register}
      alternate={{ prompt: 'Already have an account?', href: '/login', label: 'Login' }}
      newPassword
    />
  );
}
