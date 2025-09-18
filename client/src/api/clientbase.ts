/* eslint-disable @typescript-eslint/no-explicit-any */
import { useAuthStore } from 'stores/auth-store';

export class ClientBase {
  authStore: any;

  constructor() {
    this.authStore = useAuthStore();
  }

  async transformOptions(options: RequestInit): Promise<RequestInit> {
    const token = await this.authStore.getIdToken();
    options.headers = new Headers({
      'Content-Type': 'application/json; charset=UTF-8',
      Authorization: `Bearer ${token}`,
      'Cache-Control': 'no-cache',
      Pragma: 'no-cache',
    });
    return options;
  }

  getBaseUrl(defaultUrl?: string, baseUrl?: string): string {
    if (baseUrl || defaultUrl) {
      return import.meta.env.VITE_BASE_URL || '';
    }
    return import.meta.env.VITE_BASE_URL || '';
  }
}
