# Graphigrade (Frontend)

Minimal React + Vite app with Bootstrap and JWT-based auth context.

Features
- Welcome screen with Login and Register.
- Student dashboard: view assigned exercises.
- Teacher dashboard: view exercises, students, groups; create exercise; assign student->group and exercise->group (mocked).
- Clean separation of API calls in `src/services/api.js`.
- Login/Register wired to backend OpenAPI endpoints `/api/Auth/login` and `/api/Auth/register`.

Development
1. Install dependencies
2. Start dev server

Configuration

- Set the backend base URL via Vite env: create a `.env` file (or `.env.local`) in the project root with:

	VITE_API_BASE_URL=http://localhost:5000

	Adjust the URL/port to your backend. If unset, the default is `http://localhost:5000`.

Notes

- `login(username, password)` expects the backend to return `{ username, jwtToken }` as per OpenAPI.
- `register({ username, password })` expects `201` with `{ username, jwtToken? }`. If a token is returned, the app auto-logs in; otherwise it redirects to the login page.
- Other API calls (exercises/groups) are currently mocked and can be migrated later.
