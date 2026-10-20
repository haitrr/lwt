import { NextResponse, type NextRequest } from 'next/server';
import { TOKEN_COOKIE } from '@/lib/config';

const PUBLIC_PATHS = ['/login', '/register'];

// Send signed-out visitors to /login, mirroring the old app's <Redirect to="/login" />.
export function proxy(req: NextRequest) {
  const signedIn = req.cookies.has(TOKEN_COOKIE);
  const isPublic = PUBLIC_PATHS.includes(req.nextUrl.pathname);
  if (!signedIn && !isPublic) {
    return NextResponse.redirect(new URL('/login', req.url));
  }
  return NextResponse.next();
}

export const config = {
  matcher: ['/((?!api|_next/static|_next/image|favicon.ico).*)'],
};
