/* eslint-disable @typescript-eslint/no-misused-promises */
import { defineStore, acceptHMRUpdate } from 'pinia';
import type { User, Auth } from 'firebase/auth';
import {
  signInWithEmailAndPassword,
  signOut,
  onAuthStateChanged,
  GoogleAuthProvider,
  signInWithPopup,
} from 'firebase/auth';
import type { UserResponse } from 'src/api/client';

export const useAuthStore = defineStore('auth', {
  state: () => ({
    user: null as User | null, // Stores Firebase user object
    userProfile: null as UserResponse | null, // Stores user profile from API
    token: null as string | null, // Stores Firebase ID Token (JWT)
    isAuthenticated: false,
    authLoading: true, // To track initial auth state check
    authError: null as string | null, // To store any auth errors
    firebaseAuth: null as Auth | null, // Store the auth instance
  }),

  getters: {
    isLoggedIn: (state) => state.isAuthenticated,
    //isAdmin: (state) => state.user?.claims?.role === 'Admin',
  },

  actions: {
    // Initialize with the Firebase auth instance from the boot file
    setFirebaseAuth(auth: Auth) {
      this.firebaseAuth = auth;
    },

    async initAuth() {
      if (!this.firebaseAuth) {
        console.error('Firebase auth not initialized');
        return;
      }

      return new Promise((resolve) => {
        onAuthStateChanged(this.firebaseAuth!, async (user) => {
          if (user) {
            this.user = user;
            this.isAuthenticated = true;
            // Get the ID token and store it
            this.token = await user.getIdToken();
            console.log('User logged in:', user.uid, 'Token:', this.token);

            // Optional: Force token refresh every ~50 minutes for long sessions
            // to ensure backend always gets a fresh token.
            // Firebase SDK automatically refreshes every hour, but you might want to proactively.
            setInterval(
              async () => {
                if (this.user) {
                  this.token = await this.user.getIdToken(true); // Force refresh
                  console.log('ID Token refreshed:', this.token);
                }
              },
              50 * 60 * 1000,
            ); // Every 50 minutes
          } else {
            this.user = null;
            this.userProfile = null;
            this.token = null;
            this.isAuthenticated = false;
            console.log('User logged out.');
          }
          this.authLoading = false;
          resolve(null); // Resolve the promise once auth state is determined
        });
      });
    },

    async loginUserWithEmail({ email, password }: { email: string; password: string }) {
      if (!this.firebaseAuth) {
        throw new Error('Firebase auth not initialized');
      }

      this.authError = null;
      const userCredential = await signInWithEmailAndPassword(this.firebaseAuth, email, password);
      this.user = userCredential.user;
      this.isAuthenticated = true;
      this.token = await this.user.getIdToken();
      // Optional: Persist token (e.g., in localStorage, though the SDK manages it internally)
      // localStorage.setItem('firebaseIdToken', this.token);
      return true;
    },

    async loginUserWithGoogle() {
      if (!this.firebaseAuth) {
        throw new Error('Firebase auth not initialized');
      }

      this.authError = null;
      const provider = new GoogleAuthProvider();
      const result = await signInWithPopup(this.firebaseAuth, provider);
      this.user = result.user;
      this.isAuthenticated = true;
      this.token = await this.user.getIdToken();
      return true;
    },

    async logoutUser() {
      if (!this.firebaseAuth) {
        throw new Error('Firebase auth not initialized');
      }

      await signOut(this.firebaseAuth);
      this.user = null;
      this.userProfile = null;
      this.token = null;
      this.isAuthenticated = false;
      // Clear any stored token
      // localStorage.removeItem('firebaseIdToken');
      return true;
    },

    // Action to set user profile data
    setUserProfile(profile: UserResponse) {
      this.userProfile = profile;
    },

    // Action to get the latest ID token for API calls
    async getIdToken() {
      if (!this.user) {
        return null;
      }
      // Force refresh the token if needed. The SDK handles caching.
      const newToken = await this.user.getIdToken(true);
      this.token = newToken; // Update store with latest token
      return newToken;
    },
  },
});

if (import.meta.hot) {
  import.meta.hot.accept(acceptHMRUpdate(useAuthStore, import.meta.hot));
}
