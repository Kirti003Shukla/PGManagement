Firebase Phone Auth setup (localhost demo)

Overview
- This project includes a minimal Firebase phone-auth frontend flow in `src/app/auth/phone-auth` and a Firebase init file at `src/app/shared/firebase/firebase.ts`.
- The Flow uses Firebase test phone numbers so you can demo OTP on localhost without sending real SMS.

Steps to configure and run

1) Install Firebase SDK

```bash
npm install firebase
```

2) Create Firebase project
- Go to https://console.firebase.google.com/ and create a new project.

3) Enable Phone authentication
- In the Firebase console, go to Authentication -> Sign-in method -> Phone and enable it.

4) Add a test phone number
- In the same Phone settings, add a phone number and a test verification code (e.g. +1 555-555-0100 -> 123456). This prevents real SMS and works on localhost.

5) Add your Firebase config
- Open `src/app/shared/firebase/firebase.ts` and replace the placeholder values with your project's config (found in Project Settings -> SDK setup)

6) Allow localhost domain
- Add `localhost` to Authorized Domains in Firebase Console (Authentication -> Settings)

7) Run the app

```bash
npm start
```

8) Open the Phone Auth page
- Navigate to `http://localhost:4200/phone-auth` (or your dev server port) and try the test phone number and code.

Notes
- The UI no longer stores the Firebase ID token in `localStorage`. Backend calls fetch a fresh ID token from the current Firebase user and send it as `Authorization: Bearer <token>`.
- For production, verify ID tokens in your backend using Firebase Admin SDK and issue your own JWTs or sessions.

If you want, I can scaffold a small .NET endpoint to verify the ID token and map to local users — tell me if you'd like that next.