// Minimal Firebase initialization for phone auth.
// Replace the config values with your Firebase project's config.
import { initializeApp } from 'firebase/app';
import { getAuth } from 'firebase/auth';

const firebaseConfig = {
  apiKey: 'AIzaSyCCYWPfeo37z8zinJ8D_QX0B8xdBStk1S8',
  authDomain: 'pg-management-67d23.firebaseapp.com',
  projectId: 'pg-management-67d23',
  storageBucket: 'pg-management-67d23.firebasestorage.app',
  messagingSenderId: '668605958634',
  appId: '1:668605958634:web:b955b42eb23f209281ca93',
  measurementId: 'G-ZRZ8C4SG1P',
};

const app = initializeApp(firebaseConfig);
export const firebaseAuth = getAuth(app);

export default app;
