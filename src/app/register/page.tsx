import AuthForm from '../AuthForm';
import { register } from '@/lib/auth';

export const metadata = { title: 'Register' };

export default function RegisterPage() {
  return <AuthForm title="Register" action={register} alternate={{ href: '/login', label: 'Already have an account?' }} />;
}
