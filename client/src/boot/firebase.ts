import { defineBoot } from '#q-app/wrappers';
import { initializeApp, type FirebaseApp } from 'firebase/app';
import { getAuth, type Auth } from 'firebase/auth';
import { useAuthStore } from 'src/stores/auth-store'; // Adjusted path for Vite/Quasar alias

// Type for API response
interface ClientConfig {
  firebaseApiKey: string;
  firebaseAuthDomain: string;
  firebaseProjectId: string;
  firebaseStorageBucket: string;
  firebaseMessagingSenderId: string;
  firebaseAppId: string;
  firebaseMeasurementId: string;
}

// Type for window environment variables (keeping for backwards compatibility)
declare global {
  interface Window {
    __ENV__?: Record<string, string>;
  }
}

// Function to fetch configuration from API
const fetchClientConfig = async (): Promise<ClientConfig> => {
  try {
    const response = await fetch('/api/config/client');
    if (!response.ok) {
      throw new Error(`Failed to fetch config: ${response.status}`);
    }
    return await response.json();
  } catch (error) {
    console.error('Failed to fetch client config from API:', error);
    // Fallback to environment variables or window object
    return {
      firebaseApiKey: import.meta.env.VITE_FIREBASE_API_KEY || window.__ENV__?.VITE_FIREBASE_API_KEY || '',
      firebaseAuthDomain: import.meta.env.VITE_FIREBASE_AUTH_DOMAIN || window.__ENV__?.VITE_FIREBASE_AUTH_DOMAIN || '',
      firebaseProjectId: import.meta.env.VITE_FIREBASE_PROJECT_ID || window.__ENV__?.VITE_FIREBASE_PROJECT_ID || '',
      firebaseStorageBucket: import.meta.env.VITE_FIREBASE_STORAGE_BUCKET || window.__ENV__?.VITE_FIREBASE_STORAGE_BUCKET || '',
      firebaseMessagingSenderId: import.meta.env.VITE_FIREBASE_MESSAGING_SENDER_ID || window.__ENV__?.VITE_FIREBASE_MESSAGING_SENDER_ID || '',
      firebaseAppId: import.meta.env.VITE_FIREBASE_APP_ID || window.__ENV__?.VITE_FIREBASE_APP_ID || '',
      firebaseMeasurementId: import.meta.env.VITE_FIREBASE_MEASUREMENT_ID || window.__ENV__?.VITE_FIREBASE_MEASUREMENT_ID || '',
    };
  }
};

// Export placeholder instances (will be properly initialized in boot function)
let firebaseApp: FirebaseApp | null = null;
let firebaseAuth: Auth | null = null;

// "async" is optional;
// more info on params: https://v2.quasar.dev/quasar-cli-vite/boot-files
export default defineBoot(async ({ app, router, store }) => {
  // Fetch configuration from API
  const config = await fetchClientConfig();

  const firebaseConfig = {
    apiKey: config.firebaseApiKey,
    authDomain: config.firebaseAuthDomain,
    projectId: config.firebaseProjectId,
    storageBucket: config.firebaseStorageBucket,
    messagingSenderId: config.firebaseMessagingSenderId,
    appId: config.firebaseAppId,
  };

  // Initialize Firebase app with runtime config
  firebaseApp = initializeApp(firebaseConfig);
  firebaseAuth = getAuth(firebaseApp);

  // Option 1: Provide directly to the Vue app instance (Vue 3, recommended for services)
  app.provide('firebaseAuth', firebaseAuth);

  const authStore = useAuthStore(store);

  // Pass the Firebase auth instance to the store
  authStore.setFirebaseAuth(firebaseAuth);

  await authStore.initAuth(); // Call the initAuth action

  console.log('Firebase auth state initialized and ready.');

  // --- Navigation Guards ---
  // These run after the initial auth state check is complete
  // Navigation Guard: Protect routes based on auth state
  router.beforeEach((to, from, next) => {
    if (to.meta.requiresAuth && !authStore.isAuthenticated) {
      // If route requires auth and user is not authenticated, redirect to login
      next({ name: 'login', query: { redirect: to.fullPath } });
    } else if (to.meta.hideForAuth && authStore.isAuthenticated) {
      // If route should be hidden for authenticated users (e.g., login/register page)
      next({ name: 'home' }); // Redirect to home or dashboard
    } else {
      next(); // Proceed to route
    }
  });
});

export { firebaseApp, firebaseAuth };
