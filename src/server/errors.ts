/** An error whose message is safe to show the user (the old API's 400/403/404 responses). */
export class UserError extends Error {
  constructor(
    message: string,
    public status: 400 | 403 | 404 = 400,
  ) {
    super(message);
  }
}
